using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace ScrapeX
{
    public sealed class CountryInfo
    {
        public string Ip, Code, Name, Source;
        public DateTime CheckedUtc;
    }
    public sealed class CountryResolver : IDisposable
    {
        private sealed class Provider
        {
            public string Name, Template;
            public readonly SemaphoreSlim Gate = new SemaphoreSlim(1);
            public DateTime NextRequest, BlockedUntil;
        }
        private readonly Provider[] providers = {
            new Provider { Name="country.is", Template="https://api.country.is/{0}" },
            new Provider { Name="ipwho.is", Template="https://ipwho.is/{0}" },
            new Provider { Name="ipapi.co", Template="https://ipapi.co/{0}/json/" }
        };
        private readonly HttpClient direct;
        private readonly Func<ProxyEndpoint, ProxyTypeDetector.ProxyType, Uri, CancellationToken, Task<GeoHttpResponse>> throughProxy;
        private readonly Dictionary<string, CountryInfo> cache = new Dictionary<string, CountryInfo>();
        private readonly SemaphoreSlim cacheGate = new SemaphoreSlim(1);
        private readonly string cachePath;
        public event Action<string> Diagnostic;
        public CountryResolver(string path, HttpMessageHandler handler = null, Func<ProxyEndpoint, ProxyTypeDetector.ProxyType, Uri, CancellationToken, Task<GeoHttpResponse>> proxyTransport = null)
        {
            cachePath = path;
            direct = new HttpClient(handler ?? new HttpClientHandler { UseProxy = false, AllowAutoRedirect = false, SslProtocols = SslProtocols.Tls12, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }) { Timeout = Timeout.InfiniteTimeSpan, MaxResponseContentBufferSize = 65536 };
            direct.DefaultRequestHeaders.UserAgent.ParseAdd("ScrapeX/2.0");
            throughProxy = proxyTransport ?? ProxyTypeDetector.FetchCountryThroughProxyAsync;
            try
            {
                if (File.Exists(cachePath) && new FileInfo(cachePath).Length <= 8 * 1024 * 1024)
                    foreach (var entry in JsonConvert.DeserializeObject<CountryInfo[]>(File.ReadAllText(cachePath)) ?? new CountryInfo[0])
                        if (Valid(entry)) cache[entry.Ip] = entry;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is ArgumentException) { }
        }
        private static bool Valid(CountryInfo entry)
        {
            IPAddress ip; string name;
            return entry != null && IPAddress.TryParse(entry.Ip, out ip) && CountryName(entry.Code, out name) && entry.CheckedUtc <= DateTime.UtcNow.AddMinutes(5) && entry.CheckedUtc > DateTime.UtcNow.AddDays(-7);
        }
        private static bool CountryName(string code, out string name)
        {
            name = null;
            if (code == null || !System.Text.RegularExpressions.Regex.IsMatch(code, "^[A-Za-z]{2}$")) return false;
            code = code.ToUpperInvariant();
            if (code == "XK") { name = "Kosovo"; return true; }
            try { var region = new RegionInfo(code); if (!region.TwoLetterISORegionName.Equals(code, StringComparison.OrdinalIgnoreCase)) return false; name = region.EnglishName; return true; }
            catch (ArgumentException) { return false; }
        }
        public static CountryInfo Parse(string provider, string requestedIp, string json)
        {
            var data = JObject.Parse(json);
            if ((bool?)data["success"] == false || (bool?)data["error"] == true) return null;
            IPAddress actual, requested;
            if (!IPAddress.TryParse((string)data["ip"], out actual) || !IPAddress.TryParse(requestedIp, out requested) || !actual.Equals(requested)) return null;
            string code = provider == "country.is" ? (string)data["country"] : (string)data["country_code"];
            string name;
            if (!CountryName(code, out name)) return null;
            return new CountryInfo { Ip = requested.ToString(), Code = code.ToUpperInvariant(), Name = name, Source = provider, CheckedUtc = DateTime.UtcNow };
        }
        public async Task<CountryInfo> ResolveAsync(ProxyTypeDetector.ProxyTestResult route, CancellationToken token)
        {
            ProxyEndpoint endpoint;
            if (!ProxyEndpoint.TryParse(route.Endpoint, out endpoint)) return null;
            await cacheGate.WaitAsync(token).ConfigureAwait(false);
            try { CountryInfo cached; if (cache.TryGetValue(endpoint.Host, out cached) && Valid(cached)) return cached; }
            finally { cacheGate.Release(); }
            var result = await Race(endpoint, route.Type, false, token).ConfigureAwait(false);
            if (result == null && route.Type != ProxyTypeDetector.ProxyType.Unknown) result = await Race(endpoint, route.Type, true, token).ConfigureAwait(false);
            if (result == null) return null;
            await cacheGate.WaitAsync(token).ConfigureAwait(false);
            try
            {
                cache[endpoint.Host] = result;
                // Keep bounded, validated positive entries. Failures never become permanent cache hits.
                if (cache.Count > 20000) foreach (var key in cache.OrderBy(p => p.Value.CheckedUtc).Take(cache.Count - 20000).Select(p => p.Key).ToArray()) cache.Remove(key);
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(cachePath)));
                    string temporary = cachePath + ".tmp";
                    File.WriteAllText(temporary, JsonConvert.SerializeObject(cache.Values));
                    if (File.Exists(cachePath)) File.Replace(temporary, cachePath, null); else File.Move(temporary, cachePath);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { Diagnostic?.Invoke("Country cache: " + ex.Message); }
            }
            finally { cacheGate.Release(); }
            return result;
        }
        private async Task<CountryInfo> Race(ProxyEndpoint endpoint, ProxyTypeDetector.ProxyType type, bool proxied, CancellationToken token)
        {
            using (var race = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                var pending = providers.Select(p => Query(p, endpoint, type, proxied, race.Token)).ToList(); var all = pending.ToArray();
                try
                {
                    while (pending.Count > 0)
                    {
                        var done = await Task.WhenAny(pending).ConfigureAwait(false); pending.Remove(done);
                        var found = await done.ConfigureAwait(false); if (found != null) return found;
                    }
                    return null;
                }
                finally { race.Cancel(); try { await Task.WhenAll(all).ConfigureAwait(false); } catch (OperationCanceledException) { } token.ThrowIfCancellationRequested(); }
            }
        }
        private async Task<CountryInfo> Query(Provider provider, ProxyEndpoint endpoint, ProxyTypeDetector.ProxyType type, bool proxied, CancellationToken token)
        {
            await provider.Gate.WaitAsync(token).ConfigureAwait(false);
            try
            {
                if (DateTime.UtcNow < provider.BlockedUntil) return null;
                var wait = provider.NextRequest - DateTime.UtcNow; if (wait > TimeSpan.Zero) await Task.Delay(wait, token).ConfigureAwait(false);
                provider.NextRequest = DateTime.UtcNow.AddMilliseconds(1100);
                var uri = new Uri(string.Format(provider.Template, endpoint.Host));
                GeoHttpResponse response;
                if (proxied) response = await throughProxy(endpoint, type, uri, token).ConfigureAwait(false);
                else using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(token))
                {
                    timeout.CancelAfter(4000);
                    using (var raw = await direct.GetAsync(uri, timeout.Token).ConfigureAwait(false))
                    {
                        int retry = raw.Headers.RetryAfter?.Delta != null ? (int)raw.Headers.RetryAfter.Delta.Value.TotalSeconds : raw.Headers.RetryAfter?.Date != null ? Math.Max(1, (int)(raw.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow).TotalSeconds) : 0;
                        response = new GeoHttpResponse { Status = (int)raw.StatusCode, Body = await raw.Content.ReadAsStringAsync().ConfigureAwait(false), RetryAfterSeconds = retry };
                    }
                }
                if (response.Status == 429)
                {
                    // Same provider cooldown applies to both routes; proxy fallback does not evade quota limits.
                    provider.BlockedUntil = DateTime.UtcNow.AddSeconds(Math.Max(provider.Name == "ipwho.is" ? 86400 : 60, Math.Min(86400, response.RetryAfterSeconds)));
                    Diagnostic?.Invoke("Country " + provider.Name + ": rate limited; using other providers"); return null;
                }
                if (response.Status != 200) { Diagnostic?.Invoke("Country " + provider.Name + ": HTTP " + response.Status); return null; }
                var country = Parse(provider.Name, endpoint.Host, response.Body);
                if (country != null) country.Source += proxied ? " via " + type : " direct";
                else Diagnostic?.Invoke("Country " + provider.Name + ": missing/invalid country or mismatched IP");
                return country;
            }
            catch (OperationCanceledException) { token.ThrowIfCancellationRequested(); Diagnostic?.Invoke("Country " + provider.Name + ": request timeout"); return null; }
            catch (Exception ex) when (ex is HttpRequestException || ex is IOException || ex is System.Net.Sockets.SocketException || ex is AuthenticationException || ex is ObjectDisposedException || ex is JsonException || ex is FormatException || ex is InvalidCastException || ex is NotSupportedException || ex is InvalidOperationException)
            { Diagnostic?.Invoke("Country " + provider.Name + (proxied ? " via proxy: " : " direct: ") + ex.Message); return null; }
            finally { provider.Gate.Release(); }
        }
        public void Dispose() { direct.Dispose(); cacheGate.Dispose(); foreach (var provider in providers) provider.Gate.Dispose(); }
    }
}
