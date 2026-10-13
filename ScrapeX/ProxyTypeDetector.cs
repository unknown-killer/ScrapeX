using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapeX
{
    public sealed partial class ProxyTypeDetector
    {
        public sealed class LocalCapacityException : IOException
        { public LocalCapacityException(Exception inner) : base("Local socket capacity exhausted; the endpoint must be retried, not marked failed.", inner) { } }
        private Task<IPAddress[]> targetAddresses;
        private string resolvedHost;
        public enum ProxyType { Http, Socks5, Socks4, Unknown }
        public sealed class ProxyTestResult
        {
            internal ResultWriter DetailWriter { get; set; }
            public string Endpoint { get; set; }
            public ProxyType Type { get; set; }
            public long SpeedMs { get; set; }
            public long TcpPingMs { get; set; } = -1;
            public string Detail { get; set; }
            public DateTime CheckedAt { get; set; }
            public bool HttpsVerified { get; set; }
            public bool HttpWorking { get; set; }
            public ProxyType HttpType { get; set; } = ProxyType.Unknown;
            public ProxyType HttpsType { get; set; } = ProxyType.Unknown;
            public long HttpLatencyMs { get; set; } = -1;
            public long HttpsLatencyMs { get; set; } = -1;
            public string HttpsDetail { get; set; }
            public string TlsProtocol { get; set; }
            public string Country { get; set; } = "Pending";
            public string CountryCode { get; set; }
            public string CountryDetail { get; set; }
            public string SecurityLabel { get { return HttpsVerified ? "HTTPS VERIFIED" : HttpWorking ? "HTTP ONLY / TLS FAILED" : "FAILED"; } }
        }
        public async Task<ProxyTestResult> CheckAsync(ProxyEndpoint proxy, Uri httpTarget, Uri httpsTarget, int timeoutMs, CancellationToken token, bool retryAlternate = false, bool tcpPrechecked = false)
        {
            if (httpTarget.Scheme != "http" || httpsTarget.Scheme != "https") throw new ArgumentException("Separate HTTP and HTTPS targets are required.");
            if (timeoutMs < 1) throw new ArgumentOutOfRangeException(nameof(timeoutMs));
            token.ThrowIfCancellationRequested();
            if (!tcpPrechecked)
            {
                var unreachable = await CheckTcpAsync(proxy, timeoutMs, token).ConfigureAwait(false);
                if (unreachable != null) return unreachable;
            }
            // Every protocol receives its full configured budget after TCP admission; queue time is not charged.
            int remaining = timeoutMs;
            // Both targets and all protocol candidates run concurrently. A winner cancels its sibling sockets.
            var secureTask = RaceProtocols(proxy, httpsTarget, remaining, token);
            var plainTask = RaceProtocols(proxy, httpTarget, remaining, token);
            await Task.WhenAll(secureTask, plainTask).ConfigureAwait(false);
            var secure = await secureTask.ConfigureAwait(false);
            var plain = await plainTask.ConfigureAwait(false);
            if (retryAlternate)
            {
                var secureRetry = secure.Type == ProxyType.Unknown && httpsTarget.IdnHost == "www.google.com"
                    ? RaceProtocols(proxy, new Uri("https://www.microsoft.com/"), timeoutMs, token) : Task.FromResult(secure);
                var plainRetry = plain.Type == ProxyType.Unknown && httpTarget.IdnHost == "www.google.com"
                    ? RaceProtocols(proxy, new Uri("http://connectivitycheck.gstatic.com/generate_204"), timeoutMs, token) : Task.FromResult(plain);
                await Task.WhenAll(secureRetry, plainRetry).ConfigureAwait(false);
                if (!ReferenceEquals(secure, secureRetry.Result)) { secureRetry.Result.Detail = "Primary: " + secure.Detail + " | Alternate microsoft.com: " + secureRetry.Result.Detail; secure = secureRetry.Result; }
                if (!ReferenceEquals(plain, plainRetry.Result)) { plainRetry.Result.Detail = "Primary: " + plain.Detail + " | Alternate gstatic.com: " + plainRetry.Result.Detail; plain = plainRetry.Result; }
            }
            bool tlsOk = secure.Type != ProxyType.Unknown, httpOk = plain.Type != ProxyType.Unknown;
            return new ProxyTestResult
            {
                Endpoint = proxy.Key, Type = tlsOk ? secure.Type : plain.Type,
                HttpsVerified = tlsOk, HttpWorking = httpOk, HttpType = plain.Type, HttpsType = secure.Type,
                HttpLatencyMs = plain.SpeedMs, HttpsLatencyMs = secure.SpeedMs,
                TcpPingMs = tlsOk ? secure.TcpPingMs : plain.TcpPingMs,
                SpeedMs = tlsOk ? secure.SpeedMs : plain.SpeedMs, TlsProtocol = tlsOk ? secure.TlsProtocol : null,
                HttpsDetail = secure.Detail, Detail = "HTTPS: " + secure.Detail + " | HTTP: " + plain.Detail, CheckedAt = DateTime.Now
            };
        }
        public static async Task<ProxyTestResult> CheckTcpAsync(ProxyEndpoint proxy, int timeoutMs, CancellationToken token)
        {
            using (var gate = new TcpClient())
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                deadline.CancelAfter(timeoutMs);
                using (deadline.Token.Register(() => gate.Close()))
                {
                    try { await Connect(gate, proxy).ConfigureAwait(false); deadline.Token.ThrowIfCancellationRequested(); return null; }
                    catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException || ex is OperationCanceledException || ex is IOException)
                    {
                        token.ThrowIfCancellationRequested();
                        ThrowIfLocalCapacityFailure(ex);
                        return new ProxyTestResult { Endpoint = proxy.Key, Type = ProxyType.Unknown, SpeedMs = -1, CheckedAt = DateTime.Now, Detail = "TCP: " + (deadline.IsCancellationRequested ? "Timed out" : ex.Message), HttpsDetail = "TCP connection failed" };
                    }
                }
            }
        }
        private async Task<ProxyTestResult> RaceProtocols(ProxyEndpoint proxy, Uri target, int timeoutMs, CancellationToken token)
        {
            using (var race = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                var tasks = new[] { ProxyType.Http, ProxyType.Socks5, ProxyType.Socks4 }
                    .OrderBy(t => t == proxy.PreferredType ? 0 : 1)
                    .Select(t => new ProxyTypeDetector().DetectCoreAsync(proxy, target, timeoutMs, race.Token, t)).ToList();
                var all = tasks.ToArray();
                var errors = new System.Collections.Generic.List<string>();
                try
                {
                    while (tasks.Count > 0)
                    {
                        var completed = await Task.WhenAny(tasks).ConfigureAwait(false);
                        tasks.Remove(completed);
                        var result = await completed.ConfigureAwait(false);
                        if (result.Type != ProxyType.Unknown) return result;
                        errors.Add(result.Detail);
                    }
                    return new ProxyTestResult { Endpoint = proxy.Key, Type = ProxyType.Unknown, SpeedMs = -1, Detail = string.Join("; ", errors), CheckedAt = DateTime.Now };
                }
                finally
                {
                    race.Cancel();
                    // Observe every loser before releasing this worker, including cancellation exceptions.
                    try { await Task.WhenAll(all).ConfigureAwait(false); } catch (OperationCanceledException) { }
                    token.ThrowIfCancellationRequested();
                }
            }
        }
        public Task<ProxyTestResult> DetectAsync(ProxyEndpoint proxy, Uri target, int timeoutMs, CancellationToken token)
        { return DetectCoreAsync(proxy, target, timeoutMs, token, null); }
        private async Task<ProxyTestResult> DetectCoreAsync(ProxyEndpoint proxy, Uri target, int timeoutMs, CancellationToken token, ProxyType? onlyType)
        {
            string error = "No supported protocol";
            var failures = new System.Collections.Generic.List<string>();
            foreach (var type in (onlyType.HasValue ? new[] { onlyType.Value } : new[] { ProxyType.Http, ProxyType.Socks5, ProxyType.Socks4 }).OrderBy(t => t == proxy.PreferredType ? 0 : 1))
            {
                token.ThrowIfCancellationRequested();
                var watch = Stopwatch.StartNew();
                using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(token))
                using (var client = new TcpClient())
                {
                    deadline.CancelAfter(timeoutMs);
                    using (deadline.Token.Register(() => client.Close()))
                    {
                        Stream stream = null;
                        try
                        {
                            await Connect(client, proxy).ConfigureAwait(false);
                            long tcpPing = watch.ElapsedMilliseconds;
                            stream = new ProxyReadStream(client.GetStream());
                            string tlsProtocol = null;
                            if (type == ProxyType.Socks5) await Socks5(stream, target, deadline.Token).ConfigureAwait(false);
                            else if (type == ProxyType.Socks4) await Socks4(stream, target, deadline.Token).ConfigureAwait(false);
                            else if (target.Scheme == "https")
                            {
                                await Send(stream, "CONNECT " + target.IdnHost + ":" + target.Port + " HTTP/1.1\r\nHost: " + target.Authority + "\r\n\r\n", deadline.Token).ConfigureAwait(false);
                                if (await ReadStatus(stream, deadline.Token).ConfigureAwait(false) != 200) throw new IOException("CONNECT rejected");
                            }
                            if (target.Scheme == "https")
                            {
                                var tls = new SslStream(stream, false);
                                stream = new ProxyReadStream(tls);
                                // Platform certificate and hostname validation remain enabled.
                                await tls.AuthenticateAsClientAsync(target.IdnHost, null, SslProtocols.Tls12, true).ConfigureAwait(false);
                                tlsProtocol = tls.SslProtocol.ToString();
                            }
                            using (stream)
                            {
                                var path = type == ProxyType.Http && target.Scheme == "http" ? target.AbsoluteUri : target.PathAndQuery;
                                await Send(stream, "GET " + path + " HTTP/1.1\r\nHost: " + target.Authority + "\r\nUser-Agent: ScrapeX/2.0\r\nAccept: */*\r\nConnection: close\r\n\r\n", deadline.Token).ConfigureAwait(false);
                                int status = await ReadStatus(stream, deadline.Token).ConfigureAwait(false);
                                // A response inside authenticated TLS proves the tunnel works even when the origin blocks the exit IP.
                                if (tlsProtocol == null && (status < 200 || status >= 300)) throw new IOException("Target HTTP " + status);
                                if (tlsProtocol == null && target.AbsolutePath.EndsWith("/generate_204", StringComparison.Ordinal) && status != 204)
                                    throw new IOException("Expected HTTP 204; possible interception");
                                deadline.Token.ThrowIfCancellationRequested();
                                return new ProxyTestResult { Endpoint = proxy.Key, Type = type, TcpPingMs = tcpPing, SpeedMs = watch.ElapsedMilliseconds, Detail = "HTTP " + status + (status >= 300 ? " (TLS verified; destination restriction/redirect)" : ""), TlsProtocol = tlsProtocol, CheckedAt = DateTime.Now };
                            }
                        }
                        catch (Exception ex) when (ex is IOException || ex is SocketException || ex is AuthenticationException || ex is ObjectDisposedException || ex is OperationCanceledException || ex is NotSupportedException || ex is InvalidOperationException)
                        {
                            token.ThrowIfCancellationRequested();
                            ThrowIfLocalCapacityFailure(ex);
                            error = deadline.IsCancellationRequested ? "Timed out" : ex.Message;
                            failures.Add(type + ": " + error);
                        }
                        finally { stream?.Dispose(); }
                    }
                }
            }
            return new ProxyTestResult { Endpoint = proxy.Key, Type = ProxyType.Unknown, SpeedMs = -1, Detail = string.Join("; ", failures), CheckedAt = DateTime.Now };
        }
        private static Task Connect(TcpClient client, ProxyEndpoint proxy)
        {
            // Capture the socket: Framework TcpClient.EndConnect dereferences Client after Close has nulled it.
            var socket = client.Client;
            if (socket == null) throw new ObjectDisposedException(nameof(client));
            socket.NoDelay = true;
            // These short-lived probes own no application session to preserve. Abortive close avoids
            // accumulating thousands of TIME_WAIT ports after headers-only checks and cancellation.
            socket.LingerState = new LingerOption(true, 0);
            return Task.Factory.FromAsync((callback, state) => socket.BeginConnect(IPAddress.Parse(proxy.Host), proxy.Port, callback, state), socket.EndConnect, null);
        }
        private static void ThrowIfLocalCapacityFailure(Exception error)
        {
            if (IsLocalCapacityFailure(error)) throw new LocalCapacityException(error);
        }
        internal static bool IsLocalCapacityFailure(Exception error)
        {
            for (var current = error; current != null; current = current.InnerException)
            {
                if (current is LocalCapacityException) return true;
                var socket = current as SocketException;
                if (socket != null && (socket.SocketErrorCode == SocketError.NoBufferSpaceAvailable || socket.SocketErrorCode == SocketError.TooManyOpenSockets || socket.SocketErrorCode == SocketError.AddressAlreadyInUse || socket.SocketErrorCode == SocketError.AddressNotAvailable))
                    return true;
            }
            return false;
        }
        private static Task Send(Stream s, string text, CancellationToken ct)
        {
            var bytes = Encoding.ASCII.GetBytes(text);
            return s.WriteAsync(bytes, 0, bytes.Length, ct);
        }
        private static async Task<byte[]> Read(Stream s, int count, CancellationToken ct)
        {
            var data = new byte[count]; int offset = 0;
            while (offset < count)
            {
                int n = await s.ReadAsync(data, offset, count - offset, ct).ConfigureAwait(false);
                if (n == 0) throw new IOException("Connection closed before response");
                offset += n;
            }
            return data;
        }
        private static async Task<int> ReadStatus(Stream s, CancellationToken ct)
        {
            var buffered = s as ProxyReadStream;
            if (buffered != null)
            {
                for (int interim = 0; interim <= 8; interim++)
                {
                    string block = await buffered.ReadHeaderAsync(ct).ConfigureAwait(false);
                    var parts = block.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); int status;
                    if (parts.Length < 2 || (parts[0] != "HTTP/1.1" && parts[0] != "HTTP/1.0") || !int.TryParse(parts[1], out status) || status < 100 || status > 599 || status == 101) throw new IOException("Invalid HTTP response");
                    if (status >= 200) return status;
                }
                throw new IOException("Too many interim responses");
            }
            var header = new StringBuilder();
            int interimResponses = 0;
            var one = new byte[1];
            while (header.Length < 16384)
            {
                if (await s.ReadAsync(one, 0, 1, ct).ConfigureAwait(false) == 0) throw new IOException("Connection closed before response");
                header.Append((char)one[0]);
                int n = header.Length;
                if (n >= 4 && header[n - 4] == '\r' && header[n - 3] == '\n' && header[n - 2] == '\r' && header[n - 1] == '\n')
                {
                    var parts = header.ToString().Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    int code;
                    if (parts.Length < 2 || (parts[0] != "HTTP/1.1" && parts[0] != "HTTP/1.0") || !int.TryParse(parts[1], out code)) throw new IOException("Invalid HTTP response");
                    if (code < 100 || code > 599) throw new IOException("Invalid HTTP status");
                    if (code >= 100 && code < 200 && code != 101) { if (++interimResponses > 8) throw new IOException("Too many interim responses"); header.Clear(); continue; }
                    if (code == 101) throw new IOException("Unexpected protocol upgrade");
                    return code;
                }
            }
            throw new IOException("Response headers exceed 16 KB");
        }
        private static async Task Socks5(Stream s, Uri target, CancellationToken ct)
        {
            await s.WriteAsync(new byte[] { 5, 1, 0 }, 0, 3, ct).ConfigureAwait(false);
            var hello = await Read(s, 2, ct).ConfigureAwait(false);
            if (hello[0] != 5 || hello[1] != 0) throw new IOException("SOCKS5 authentication unsupported");
            var host = Encoding.ASCII.GetBytes(target.IdnHost);
            if (host.Length > 255) throw new IOException("Host too long");
            IPAddress literal;
            bool numeric = IPAddress.TryParse(target.IdnHost.Trim('[', ']'), out literal);
            byte[] address = numeric ? literal.GetAddressBytes() : host;
            int offset = numeric ? 4 : 5;
            var packet = new byte[offset + address.Length + 2];
            packet[0] = 5; packet[1] = 1; packet[3] = numeric ? (byte)(address.Length == 4 ? 1 : 4) : (byte)3;
            if (!numeric) packet[4] = (byte)host.Length;
            Buffer.BlockCopy(address, 0, packet, offset, address.Length);
            packet[packet.Length - 2] = (byte)(target.Port >> 8); packet[packet.Length - 1] = (byte)target.Port;
            await s.WriteAsync(packet, 0, packet.Length, ct).ConfigureAwait(false);
            var reply = await Read(s, 4, ct).ConfigureAwait(false);
            if (reply[0] != 5 || reply[1] != 0 || reply[2] != 0) throw new IOException("SOCKS5 connection rejected");
            int length;
            if (reply[3] == 1) length = 4;
            else if (reply[3] == 4) length = 16;
            else if (reply[3] == 3) length = (await Read(s, 1, ct).ConfigureAwait(false))[0];
            else throw new IOException("Invalid SOCKS5 address type");
            await Read(s, length + 2, ct).ConfigureAwait(false);
        }
        private async Task Socks4(Stream s, Uri target, CancellationToken ct)
        {
            // Prefer standard SOCKS4 with an IPv4 address; SOCKS4a is the DNS-failure fallback.
            var host = Encoding.ASCII.GetBytes(target.IdnHost);
            IPAddress ip;
            var literal = IPAddress.TryParse(target.Host, out ip) && ip.AddressFamily == AddressFamily.InterNetwork;
            if (!literal)
            {
                try
                {
                    if (targetAddresses == null || resolvedHost != target.IdnHost)
                    {
                        resolvedHost = target.IdnHost;
                        targetAddresses = Dns.GetHostAddressesAsync(target.IdnHost);
                    }
                    var canceled = new TaskCompletionSource<bool>();
                    using (ct.Register(() => canceled.TrySetResult(true)))
                    {
                        if (await Task.WhenAny(targetAddresses, canceled.Task).ConfigureAwait(false) != targetAddresses) ct.ThrowIfCancellationRequested();
                        foreach (var address in await targetAddresses.ConfigureAwait(false))
                            if (address.AddressFamily == AddressFamily.InterNetwork) { ip = address; literal = true; break; }
                    }
                }
                catch (SocketException) { /* SOCKS4a can resolve names that local DNS cannot. */ }
            }

            var packet = new byte[literal ? 9 : 10 + host.Length];
            packet[0] = 4; packet[1] = 1; packet[2] = (byte)(target.Port >> 8); packet[3] = (byte)target.Port;
            if (literal) Buffer.BlockCopy(ip.GetAddressBytes(), 0, packet, 4, 4);
            else { packet[7] = 1; Buffer.BlockCopy(host, 0, packet, 9, host.Length); }
            await s.WriteAsync(packet, 0, packet.Length, ct).ConfigureAwait(false);
            var reply = await Read(s, 8, ct).ConfigureAwait(false);
            if (reply[0] != 0 || reply[1] != 90) throw new IOException("SOCKS4 connection rejected");
        }
    }
}
