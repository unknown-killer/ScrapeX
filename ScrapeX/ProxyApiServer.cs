using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ScrapeX
{
    public sealed class ProxyApiServer
    {
        private readonly ProxyPublication publication;
        private readonly ServerSettings settings;
        private readonly Func<object> refreshStatus;
        private readonly HttpListener listener = new HttpListener();
        private readonly ConcurrentDictionary<long, Task> requests = new ConcurrentDictionary<long, Task>();
        private readonly SemaphoreSlim capacity = new SemaphoreSlim(32);
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private readonly Stopwatch uptime = new Stopwatch();
        private Task acceptTask;
        private long sequence, requestCount, deniedCount, errorCount, bytesSent;
        private readonly object rateLock = new object();
        private long rateSecond;
        private int rateCount;
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), DateTimeZoneHandling = DateTimeZoneHandling.Utc };
        public bool IsRunning { get { return listener.IsListening; } }
        public bool IsHealthy { get { return IsRunning && acceptTask != null && !acceptTask.IsCompleted; } }
        public long RequestCount { get { return Interlocked.Read(ref requestCount); } }
        public long DeniedCount { get { return Interlocked.Read(ref deniedCount); } }
        public long ErrorCount { get { return Interlocked.Read(ref errorCount); } }
        public long BytesSent { get { return Interlocked.Read(ref bytesSent); } }
        public int ActiveRequests { get { return requests.Count; } }
        public double UptimeSeconds { get { return uptime.Elapsed.TotalSeconds; } }
        public event Action<string> Diagnostic;
        public ProxyApiServer(ProxyPublication publication, ServerSettings settings, Func<object> refreshStatus)
        { this.publication = publication; this.settings = settings; this.refreshStatus = refreshStatus; }
        public void Start()
        {
            settings.Validate(); listener.Prefixes.Add(settings.Prefix);
            try { listener.Start(); }
            catch (HttpListenerException ex)
            {
                listener.Close();
                throw new IOException("Could not listen on " + settings.Prefix + ". Check the port and Windows URL reservation. In an elevated terminal: netsh http add urlacl url=" + settings.Prefix + " user=\"" + Environment.UserDomainName + "\\" + Environment.UserName + "\". " + ex.Message, ex);
            }
            uptime.Start(); acceptTask = AcceptAsync();
        }
        private async Task AcceptAsync()
        {
            while (!lifetime.IsCancellationRequested)
            {
                HttpListenerContext context;
                try { context = await listener.GetContextAsync().ConfigureAwait(false); }
                catch (Exception ex) when (ex is HttpListenerException || ex is ObjectDisposedException || ex is InvalidOperationException)
                { if (!lifetime.IsCancellationRequested) Diagnostic?.Invoke("API listener stopped: " + ex.Message); break; }
                Interlocked.Increment(ref requestCount);
                bool rateOk;
                lock (rateLock)
                {
                    long second = uptime.ElapsedMilliseconds / 1000;
                    if (second != rateSecond) { rateSecond = second; rateCount = 0; }
                    rateOk = ++rateCount <= 120;
                }
                if (!rateOk || !capacity.Wait(0))
                {
                    Interlocked.Increment(ref deniedCount);
                    try { context.Response.StatusCode = 429; context.Response.Headers["Retry-After"] = "1"; context.Response.ContentLength64 = 0; context.Response.Close(); } catch { }
                    continue;
                }
                long id = Interlocked.Increment(ref sequence);
                var task = Task.Run(() => HandleAsync(context)); requests[id] = task;
                _ = task.ContinueWith(t => { Task removed; requests.TryRemove(id, out removed); capacity.Release(); }, TaskScheduler.Default);
            }
        }
        public static bool KeyMatches(string expected, string actual)
        {
            if (expected == null || actual == null || expected.Length != actual.Length) return false;
            int difference = 0; for (int i = 0; i < expected.Length; i++) difference |= expected[i] ^ actual[i]; return difference == 0;
        }
        private bool Authorized(HttpListenerRequest request)
        {
            if (settings.AllowAnonymous) return true;
            string supplied = request.Headers["X-Api-Key"], authorization = request.Headers["Authorization"];
            if (supplied == null && authorization != null && authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) supplied = authorization.Substring(7);
            return KeyMatches(settings.ApiKey, supplied);
        }
        public object Statistics()
        {
            var snapshot = publication.Current;
            var rows = new ProxyQuery { Https = "all", MaxAgeHours = settings.MaxAgeHours }.Filter(snapshot, DateTime.UtcNow);
            return new { snapshot.Revision, snapshot.PublishedUtc, available = rows.Length, expired = snapshot.Proxies.Length - rows.Length,
                https = rows.Count(p => p.Https), httpOnly = rows.Count(p => !p.Https && p.Http),
                countries = rows.GroupBy(p => p.CountryCode ?? "ZZ").Select(g => new { code = g.Key, name = g.First().Country, count = g.Count() }).OrderByDescending(g => g.count).ToArray(),
                protocols = rows.GroupBy(p => p.Protocol).Select(g => new { type = g.Key, count = g.Count() }).OrderBy(g => g.type).ToArray(),
                requests = RequestCount, rejected = DeniedCount, errors = ErrorCount, bytesSent = BytesSent, uptimeSeconds = (long)UptimeSeconds, refresh = refreshStatus?.Invoke() };
        }
        private async Task HandleAsync(HttpListenerContext context)
        {
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token))
            {
                deadline.CancelAfter(TimeSpan.FromSeconds(15));
                using (deadline.Token.Register(() => { try { context.Response.Abort(); } catch { } }))
                try
                {
                    var req = context.Request; var response = context.Response;
                    response.KeepAlive = false; response.Headers["Cache-Control"] = "no-store";
                    response.Headers["X-Content-Type-Options"] = "nosniff";
                    response.Headers["Content-Security-Policy"] = "default-src 'none'; style-src 'unsafe-inline'; form-action 'self'; frame-ancestors 'none'";
                    if (req.HttpMethod != "GET" && req.HttpMethod != "HEAD") { response.Headers["Allow"] = "GET, HEAD"; await Json(context, 405, new { error = "Only GET and HEAD are supported." }, deadline.Token); return; }
                    string path = req.Url.AbsolutePath.TrimEnd('/');
                    // Public landing page describes the API but never embeds credentials or proxy data.
                    if (path == "")
                    {
                        using (var stream = typeof(ProxyApiServer).Assembly.GetManifestResourceStream("ScrapeX.ApiIndex.html"))
                        using (var reader = new StreamReader(stream)) await Send(context, 200, "text/html; charset=utf-8", reader.ReadToEnd(), deadline.Token);
                        return;
                    }
                    if (path == "/health") { await Json(context, 200, new { status = "ok", service = "ScrapeX" }, deadline.Token); return; }
                    if (!Authorized(req)) { Interlocked.Increment(ref deniedCount); response.AddHeader("WWW-Authenticate", "Bearer"); await Json(context, 401, new { error = "Supply X-Api-Key or Authorization: Bearer. Keys in URLs are not accepted." }, deadline.Token); return; }
                    if (req.RawUrl.Length > 4096) { await Json(context, 414, new { error = "Query is too long." }, deadline.Token); return; }
                    if (path == "/api/v1/stats") { await Json(context, 200, Statistics(), deadline.Token); return; }
                    if (path != "/api/v1/proxies" && path != "/proxies.txt") { await Json(context, 404, new { error = "Use /api/v1/proxies, /proxies.txt or /api/v1/stats." }, deadline.Token); return; }
                    var query = ProxyQuery.Parse(req.QueryString, settings.MaxAgeHours, path == "/proxies.txt");
                    var snapshot = publication.Current;
                    var filtered = query.Filter(snapshot, DateTime.UtcNow);
                    var page = filtered.Skip(query.Offset).Take(query.Limit).ToArray();
                    response.Headers["X-Total-Count"] = filtered.Length.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    response.Headers["X-Snapshot-Revision"] = snapshot.Revision;
                    if (query.Format == "txt") await Send(context, 200, "text/plain; charset=utf-8", string.Join("\n", page.Select(p => p.Endpoint)) + (page.Length > 0 ? "\n" : ""), deadline.Token);
                    else await Json(context, 200, new { snapshot.Revision, snapshot.PublishedUtc, total = filtered.Length, count = page.Length, offset = query.Offset, limit = query.Limit, nextOffset = query.Offset + page.Length < filtered.Length ? (int?)(query.Offset + page.Length) : null, proxies = page }, deadline.Token);
                }
                catch (ArgumentException ex) { try { await Json(context, 400, new { error = ex.Message }, deadline.Token); } catch { } }
                catch (Exception ex)
                {
                    if (!(ex is OperationCanceledException) && !lifetime.IsCancellationRequested) Interlocked.Increment(ref errorCount);
                    try { await Json(context, 500, new { error = "Request could not be completed." }, deadline.Token); } catch { }
                }
                finally { try { context.Response.Close(); } catch { } }
            }
        }
        private Task Json(HttpListenerContext context, int status, object data, CancellationToken token)
        { return Send(context, status, "application/json; charset=utf-8", JsonConvert.SerializeObject(data, JsonSettings), token); }
        private async Task Send(HttpListenerContext context, int status, string type, string body, CancellationToken token)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(body); context.Response.StatusCode = status; context.Response.ContentType = type; context.Response.ContentLength64 = bytes.Length;
            if (context.Request.HttpMethod == "HEAD") return;
            await context.Response.OutputStream.WriteAsync(bytes, 0, bytes.Length, token).ConfigureAwait(false); Interlocked.Add(ref bytesSent, bytes.Length);
        }
        public async Task StopAsync()
        {
            lifetime.Cancel(); listener.Close();
            if (acceptTask != null) await acceptTask.ConfigureAwait(false);
            await Task.WhenAll(requests.Values.ToArray()).ConfigureAwait(false); uptime.Stop();
            // Continuations release permits after handlers settle; don't dispose the semaphore under them.
            lifetime.Dispose();
        }
    }
}
