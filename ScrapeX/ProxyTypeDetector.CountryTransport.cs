using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace ScrapeX
{
    public sealed class GeoHttpResponse
    {
        public int Status;
        public string Body;
        public int RetryAfterSeconds;
    }
    public sealed partial class ProxyTypeDetector
    {
        public static async Task<GeoHttpResponse> FetchCountryThroughProxyAsync(ProxyEndpoint proxy, ProxyType type, Uri target, CancellationToken token)
        {
            if (target.Scheme != "https") throw new ArgumentException("Country transport requires HTTPS");
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(token))
            using (var client = new TcpClient())
            {
                timeout.CancelAfter(6000);
                using (timeout.Token.Register(() => client.Close()))
                {
                    await Connect(client, proxy).ConfigureAwait(false);
                    var network = client.GetStream();
                    if (type == ProxyType.Socks5) await Socks5(network, target, timeout.Token).ConfigureAwait(false);
                    else if (type == ProxyType.Socks4) await new ProxyTypeDetector().Socks4(network, target, timeout.Token).ConfigureAwait(false);
                    else if (type == ProxyType.Http)
                    {
                        await Send(network, "CONNECT " + target.IdnHost + ":" + target.Port + " HTTP/1.1\r\nHost: " + target.Authority + "\r\n\r\n", timeout.Token).ConfigureAwait(false);
                        if (await ReadStatus(network, timeout.Token).ConfigureAwait(false) != 200) throw new IOException("Country CONNECT rejected");
                    }
                    else throw new IOException("No verified proxy protocol for country request");
                    using (var tls = new SslStream(network, false))
                    {
                        await tls.AuthenticateAsClientAsync(target.IdnHost, null, SslProtocols.Tls12, true).ConfigureAwait(false);
                        await Send(tls, "GET " + target.PathAndQuery + " HTTP/1.1\r\nHost: " + target.Authority + "\r\nAccept: application/json\r\nAccept-Encoding: identity\r\nUser-Agent: ScrapeX/2.0\r\nConnection: close\r\n\r\n", timeout.Token).ConfigureAwait(false);
                        using (var memory = new MemoryStream())
                        {
                            var buffer = new byte[4096]; int n;
                            while ((n = await tls.ReadAsync(buffer, 0, buffer.Length, timeout.Token).ConfigureAwait(false)) != 0)
                            {
                                if (memory.Length + n > 131072) throw new IOException("Country response too large");
                                memory.Write(buffer, 0, n);
                            }
                            return ParseCountryHttp(memory.ToArray());
                        }
                    }
                }
            }
        }
        public static GeoHttpResponse ParseCountryHttp(string text)
        { return ParseCountryHttp(Encoding.UTF8.GetBytes(text)); }
        public static GeoHttpResponse ParseCountryHttp(byte[] wire)
        {
            int split = -1;
            for (int i = 0; i + 3 < wire.Length && i <= 16384; i++)
                if (wire[i] == 13 && wire[i + 1] == 10 && wire[i + 2] == 13 && wire[i + 3] == 10) { split = i; break; }
            string text = split < 0 ? "" : Encoding.ASCII.GetString(wire, 0, split + 4);
            if (split < 0 || split > 16384) throw new IOException("Invalid country HTTP headers");
            var lines = text.Substring(0, split).Split(new[] { "\r\n" }, StringSplitOptions.None);
            var status = lines[0].Split(' '); int code;
            if (status.Length < 2 || !status[0].StartsWith("HTTP/1.") || !int.TryParse(status[1], out code)) throw new IOException("Invalid country HTTP status");
            byte[] payload = new byte[wire.Length - split - 4]; Buffer.BlockCopy(wire, split + 4, payload, 0, payload.Length);
            string body = Encoding.UTF8.GetString(payload); int retry = 0; bool chunked = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("Transfer-Encoding:", StringComparison.OrdinalIgnoreCase)) chunked = line.IndexOf("chunked", StringComparison.OrdinalIgnoreCase) >= 0;
                if (line.StartsWith("Retry-After:", StringComparison.OrdinalIgnoreCase))
                {
                    string value = line.Substring(12).Trim(); DateTimeOffset date;
                    if (!int.TryParse(value, out retry) && DateTimeOffset.TryParse(value, out date)) retry = Math.Max(1, (int)(date - DateTimeOffset.UtcNow).TotalSeconds);
                }
            }
            if (chunked)
            {
                // Work on bytes: chunk lengths are bytes, not Unicode character counts.
                byte[] bytes = payload; int offset = 0;
                using (var output = new MemoryStream())
                {
                    while (true)
                    {
                        int end = offset; while (end + 1 < bytes.Length && !(bytes[end] == 13 && bytes[end + 1] == 10)) end++;
                        int length;
                        if (end + 1 >= bytes.Length || !int.TryParse(Encoding.ASCII.GetString(bytes, offset, end - offset).Split(';')[0], System.Globalization.NumberStyles.HexNumber, null, out length) || length < 0) throw new IOException("Invalid chunk length");
                        offset = end + 2; if (length == 0) break;
                        if (length > bytes.Length - offset - 2 || bytes[offset + length] != 13 || bytes[offset + length + 1] != 10) throw new IOException("Truncated chunk");
                        output.Write(bytes, offset, length); offset += length + 2;
                    }
                    body = Encoding.UTF8.GetString(output.ToArray());
                }
            }
            return new GeoHttpResponse { Status = code, Body = body, RetryAfterSeconds = retry };
        }
    }
}
