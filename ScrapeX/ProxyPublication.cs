using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace ScrapeX
{
    public sealed class PublishedProxy
    {
        public string Endpoint { get; set; }
        public string Protocol { get; set; }
        public string CountryCode { get; set; }
        public string Country { get; set; }
        public bool Https { get; set; }
        public bool Http { get; set; }
        public long PingMs { get; set; }
        public long HttpMs { get; set; }
        public long HttpsMs { get; set; }
        public DateTime CheckedUtc { get; set; }
        public static PublishedProxy FromResult(ProxyTypeDetector.ProxyTestResult result)
        {
            ProxyEndpoint endpoint;
            if ((!result.HttpWorking && !result.HttpsVerified) || result.Type == ProxyTypeDetector.ProxyType.Unknown || !ProxyEndpoint.TryParse(result.Endpoint, out endpoint)) return null;
            string code = result.CountryCode;
            bool known = code != null && code.Length == 2 && code.All(c => c >= 'A' && c <= 'Z' || c >= 'a' && c <= 'z');
            return new PublishedProxy { Endpoint = endpoint.Key, Protocol = result.Type.ToString().ToLowerInvariant(),
                CountryCode = known ? code.ToUpperInvariant() : "ZZ", Country = known ? result.Country : "Unknown",
                Https = result.HttpsVerified, Http = result.HttpWorking, PingMs = result.TcpPingMs,
                HttpMs = result.HttpLatencyMs, HttpsMs = result.HttpsLatencyMs,
                CheckedUtc = result.CheckedAt.Kind == DateTimeKind.Utc ? result.CheckedAt : result.CheckedAt.ToUniversalTime() };
        }
    }
    public sealed class ProxySnapshot
    {
        public string Revision { get; set; } = Guid.NewGuid().ToString("N");
        public DateTime? PublishedUtc { get; set; }
        public PublishedProxy[] Proxies { get; set; } = new PublishedProxy[0];
    }
    public sealed class ProxyPublication
    {
        private readonly object sync = new object();
        private readonly string path;
        private ProxySnapshot current = new ProxySnapshot();
        public ProxySnapshot Current { get { return Volatile.Read(ref current); } }
        public ProxyPublication(string path) { this.path = path; }
        public void Load()
        {
            if (!File.Exists(path)) return;
            if (new FileInfo(path).Length > 128 * 1024 * 1024) throw new IOException("Published snapshot exceeds size limit.");
            var loaded = JsonConvert.DeserializeObject<ProxySnapshot>(File.ReadAllText(path));
            if (loaded == null || loaded.Proxies == null) throw new IOException("Published snapshot is invalid.");
            loaded.Proxies = Sanitize(loaded.Proxies);
            Interlocked.Exchange(ref current, loaded);
        }
        private static PublishedProxy[] Sanitize(IEnumerable<PublishedProxy> proxies)
        {
            return proxies.Where(p => {
                ProxyEndpoint endpoint;
                return p != null && (p.Https || p.Http) && ProxyEndpoint.TryParse(p.Endpoint, out endpoint) &&
                    new[] { "http", "socks4", "socks5" }.Contains(p.Protocol) && p.CheckedUtc.Kind == DateTimeKind.Utc && p.CheckedUtc <= DateTime.UtcNow.AddMinutes(1);
            }).GroupBy(p => p.Endpoint).Select(g => g.OrderByDescending(p => p.CheckedUtc).First()).OrderBy(p => p.Endpoint, StringComparer.Ordinal).Take(ProxyScraperManager.MaxCandidates).ToArray();
        }
        public void Publish(IEnumerable<PublishedProxy> proxies)
        {
            // Durable write first: a disk error must not discard the last complete publication.
            lock (sync)
            {
                var snapshot = new ProxySnapshot { PublishedUtc = DateTime.UtcNow, Proxies = Sanitize(proxies) };
                AtomicFile.Write(path, JsonConvert.SerializeObject(snapshot));
                Interlocked.Exchange(ref current, snapshot);
            }
        }
    }
    public sealed class ProxyQuery
    {
        public string Format = "json", Https = "true", Sort = "endpoint";
        public int Limit = 1000, Offset, MaxPing = int.MaxValue;
        public double MaxAgeHours;
        public HashSet<string> Types, Countries;
        public static ProxyQuery Parse(NameValueCollection query, double maxAgeHours, bool textRoute = false)
        {
            var allowed = new[] { "format", "type", "country", "https", "limit", "offset", "max_ping", "max_age_hours", "sort" };
            foreach (string key in query.AllKeys)
                if (key == null || !allowed.Contains(key) || query.GetValues(key).Length != 1) throw new ArgumentException("Unknown or repeated query parameter: " + key);
            var q = new ProxyQuery { MaxAgeHours = maxAgeHours, Format = textRoute ? "txt" : "json" };
            if (query["format"] != null) q.Format = query["format"].ToLowerInvariant();
            if (q.Format != "json" && q.Format != "txt") throw new ArgumentException("format must be json or txt.");
            if (query["https"] != null) q.Https = query["https"].ToLowerInvariant();
            if (!new[] { "true", "false", "all" }.Contains(q.Https)) throw new ArgumentException("https must be true, false (HTTP-only), or all.");
            if (query["sort"] != null) q.Sort = query["sort"].ToLowerInvariant();
            if (q.Sort != "endpoint" && q.Sort != "ping") throw new ArgumentException("sort must be endpoint or ping.");
            q.Limit = Number(query, "limit", 1000, 1, 10000); q.Offset = Number(query, "offset", 0, 0, 500000);
            q.MaxPing = Number(query, "max_ping", int.MaxValue, 0, 3600000);
            if (query["max_age_hours"] != null)
            {
                double age;
                if (!double.TryParse(query["max_age_hours"], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out age) || age <= 0 || age > maxAgeHours) throw new ArgumentException("max_age_hours must be positive and no greater than the server retention setting.");
                q.MaxAgeHours = age;
            }
            q.Types = Csv(query["type"], false); q.Countries = Csv(query["country"], true); return q;
        }
        private static HashSet<string> Csv(string value, bool country)
        {
            if (value == null || value.Equals("all", StringComparison.OrdinalIgnoreCase)) return null;
            var list = value.Split(',').Select(s => s.Trim().ToLowerInvariant()).ToArray();
            if (list.Length == 0 || list.Length > 250 || list.Any(s => country ? s.Length != 2 || !s.All(c => c >= 'a' && c <= 'z') : !new[] { "http", "socks4", "socks5" }.Contains(s)))
                throw new ArgumentException(country ? "country requires ISO codes such as US,DE; use ZZ for unknown." : "type must contain http, socks4, or socks5.");
            return new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
        }
        private static int Number(NameValueCollection query, string key, int fallback, int min, int max)
        {
            if (query[key] == null) return fallback;
            int n; if (!int.TryParse(query[key], NumberStyles.None, CultureInfo.InvariantCulture, out n) || n < min || n > max) throw new ArgumentException(key + " is out of range."); return n;
        }
        public PublishedProxy[] Filter(ProxySnapshot snapshot, DateTime utcNow)
        {
            var rows = snapshot.Proxies.Where(p => p.CheckedUtc >= utcNow.AddHours(-MaxAgeHours) && p.CheckedUtc <= utcNow.AddMinutes(1) &&
                (Https == "all" || (Https == "true" ? p.Https : p.Http && !p.Https)) && (Types == null || Types.Contains(p.Protocol)) &&
                (Countries == null || Countries.Contains(p.CountryCode ?? "ZZ")) && (MaxPing == int.MaxValue || p.PingMs >= 0 && p.PingMs <= MaxPing));
            return (Sort == "ping" ? rows.OrderBy(p => p.PingMs < 0 ? long.MaxValue : p.PingMs).ThenBy(p => p.Endpoint, StringComparer.Ordinal) : rows.OrderBy(p => p.Endpoint, StringComparer.Ordinal)).ToArray();
        }
    }
}
