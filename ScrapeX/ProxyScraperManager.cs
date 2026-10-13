using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapeX
{
    public sealed class SourceResult
    {
        public string Url { get; set; }
        public string Status { get; set; }
        public int Valid { get; set; }
        public int Added { get; set; }
        public override string ToString() { return Status + " | " + Valid + " valid, " + Added + " new | " + Url; }
    }
    public sealed class ProxyScraperManager
    {
        public const int MaxCandidates = 500000;
        public event Action<SourceResult> OnSourceCompleted;
        private readonly object addLock = new object();
        private readonly ConcurrentDictionary<string, ProxyEndpoint> candidates;
        public ProxyScraperManager(ConcurrentDictionary<string, ProxyEndpoint> candidates) { this.candidates = candidates; }
        public bool Add(ProxyEndpoint endpoint)
        {
            lock (addLock)
            {
                ProxyEndpoint existing;
                if (candidates.TryGetValue(endpoint.Key, out existing))
                {
                    if (existing.PreferredType == ProxyTypeDetector.ProxyType.Unknown) existing.PreferredType = endpoint.PreferredType;
                    return false;
                }
                return candidates.Count < MaxCandidates && candidates.TryAdd(endpoint.Key, endpoint);
            }
        }
        public async Task StartScrapingAsync(IEnumerable<string> urls, CancellationToken token)
        {
            using (var handler = new HttpClientHandler { AllowAutoRedirect = false, SslProtocols = System.Security.Authentication.SslProtocols.Tls12, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate })
            using (var client = new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan })
            using (var gate = new SemaphoreSlim(6))
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("ScrapeX/2.0");
                var jobs = urls.Distinct(StringComparer.Ordinal).Select(async url =>
                {
                    await gate.WaitAsync(token).ConfigureAwait(false);
                    try { await Fetch(client, url, token).ConfigureAwait(false); }
                    finally { gate.Release(); }
                });
                await Task.WhenAll(jobs).ConfigureAwait(false);
            }
        }
        private async Task Fetch(HttpClient client, string url, CancellationToken token)
        {
            var report = new SourceResult { Url = url };
            var sourceSeen = new HashSet<string>(StringComparer.Ordinal);
            try
            {
                Uri uri;
                if (!Uri.TryCreate(url, UriKind.Absolute, out uri) || (uri.Scheme != "https" && uri.Scheme != "http")) throw new IOException("Invalid source URL");
                bool geo = uri.Host == "proxylist.geonode.com";
                bool proxi = uri.Host == "papi.proxiware.com";
                bool paginated = geo || proxi || uri.Host == "www.freeproxy.world" || uri.Host == "roundproxies.com";
                bool websitePages = new[] { "advanced.name", "proxy-list.org", "www.89ip.cn", "www.my-proxy.com", "proxy.scdn.io", "proxyhub.me", "proxy.developer.pm", "kabak.top", "topfreeproxylist.com" }.Contains(uri.Host);
                string pageUrl = url;
                for (int page = 1; page <= 20; page++)
                {
                    token.ThrowIfCancellationRequested();
                    string body = await Download(client, pageUrl, token).ConfigureAwait(false);
                    if ((geo || proxi) && !(JToken.Parse(body) is JObject)) throw new IOException("API schema changed: expected an object");
                    int rawCount;
                    var items = SourceParser.Parse(body, out rawCount, url);
                    int newOnPage = 0;
                    foreach (var item in items)
                    {
                        token.ThrowIfCancellationRequested();
                        if (!sourceSeen.Add(item.Key)) continue;
                        if (item.PreferredType == ProxyTypeDetector.ProxyType.Unknown)
                        {
                            string hint = uri.AbsoluteUri.ToLowerInvariant();
                            if (hint.Contains("socks5")) item.PreferredType = ProxyTypeDetector.ProxyType.Socks5;
                            else if (hint.Contains("socks4")) item.PreferredType = ProxyTypeDetector.ProxyType.Socks4;
                            else if (uri.AbsolutePath.ToLowerInvariant().Contains("http") || hint.Contains("protocol=http")) item.PreferredType = ProxyTypeDetector.ProxyType.Http;
                        }
                        newOnPage++; report.Valid++;
                        if (Add(item)) report.Added++;
                    }
                    if (candidates.Count >= MaxCandidates) { report.Status = "Candidate limit reached (500,000)"; break; }
                    if ((!paginated && !websitePages) || rawCount == 0 || newOnPage == 0) break;
                    if (page == 20) report.Status = "OK (20-page safety limit)";
                    if (paginated) pageUrl = System.Text.RegularExpressions.Regex.Replace(url, @"([?&]page=)\d+", "${1}" + (page + 1));
                    else
                    {
                        pageUrl = NextWebsitePage(new Uri(pageUrl), body, page + 1);
                        if (pageUrl == null) break;
                    }
                    await Task.Delay(350, token).ConfigureAwait(false);
                }
                report.Status = report.Status ?? (report.Valid > 0 ? "OK" : "Empty / no valid public IPv4 endpoints");
            }
            catch (OperationCanceledException) { token.ThrowIfCancellationRequested(); report.Status = "Source timeout"; }
            catch (Exception ex) when (ex is HttpRequestException || ex is IOException || ex is Newtonsoft.Json.JsonException || ex is FormatException || ex is System.Text.RegularExpressions.RegexMatchTimeoutException)
            { report.Status = "Failed: " + ex.GetBaseException().Message; }
            OnSourceCompleted?.Invoke(report);
        }
        internal static string NextWebsitePage(Uri current, string body, int page)
        {
            foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(body, @"href\s*=\s*['""]([^'""]+)['""]", System.Text.RegularExpressions.RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2)))
            {
                Uri next;
                if (!Uri.TryCreate(current, WebUtility.HtmlDecode(m.Groups[1].Value), out next) || next.Host != current.Host || next.Scheme != current.Scheme || !string.IsNullOrEmpty(next.UserInfo)) continue;
                if (System.Text.RegularExpressions.Regex.IsMatch(next.Query, @"(?:^|[?&])page=" + page + @"(?:&|$)") && next.AbsolutePath.TrimEnd('/') == current.AbsolutePath.TrimEnd('/')) return next.AbsoluteUri;
                if (current.Host == "www.89ip.cn" && next.AbsolutePath == "/index_" + page + ".html") return next.AbsoluteUri;
                if (current.Host == "www.my-proxy.com" && next.AbsolutePath == "/free-proxy-list-" + page + ".html") return next.AbsoluteUri;
            }
            return null;
        }
        private static async Task<string> Download(HttpClient client, string url, CancellationToken token)
        {
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                deadline.CancelAfter(20000);
                using (var response = await GetResponse(client, new Uri(url), deadline.Token).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (deadline.Token.Register(() => stream.Dispose()))
                    using (var buffer = new MemoryStream())
                    {
                        var bytes = new byte[8192]; int n;
                        try
                        {
                            while ((n = await stream.ReadAsync(bytes, 0, bytes.Length, deadline.Token).ConfigureAwait(false)) > 0)
                            {
                                if (buffer.Length + n > 16 * 1024 * 1024) throw new IOException("Source exceeds 16 MB limit");
                                buffer.Write(bytes, 0, n);
                            }
                        }
                        catch (Exception) when (deadline.IsCancellationRequested) { throw new OperationCanceledException(deadline.Token); }
                        return Encoding.UTF8.GetString(buffer.ToArray()).TrimStart('\uFEFF');
                    }
                }
            }
        }
        private static async Task<HttpResponseMessage> GetResponse(HttpClient client, Uri uri, CancellationToken token)
        {
            // Framework's automatic redirect handler does not understand permanent HTTP 308.
            for (int hop = 0; hop <= 5; hop++)
            {
                var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, token).ConfigureAwait(false);
                int status = (int)response.StatusCode;
                if (status != 301 && status != 302 && status != 303 && status != 307 && status != 308) return response;
                Uri next; var location = response.Headers.Location;
                response.Dispose();
                if (hop == 5) throw new IOException("Source redirect limit reached");
                if (location == null || !Uri.TryCreate(uri, location, out next) || (next.Scheme != "http" && next.Scheme != "https") || !string.IsNullOrEmpty(next.UserInfo)) throw new IOException("Invalid source redirect");
                uri = next;
            }
            throw new IOException("Source redirect limit reached");
        }
    }
}

