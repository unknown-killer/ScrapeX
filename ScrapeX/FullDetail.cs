using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ScrapeX
{
    public static class FullDetail
    {
        public const string Header = "ENDPOINT                | COUNTRY | COUNTRY NAME             | PROTOCOL | HTTPS / SSL                | TCP PING   | HTTP TIME  | TLS TIME   | HTTP TYPE | HTTPS TYPE | TLS VERSION | CHECKED (UTC)        | COUNTRY LOOKUP | PROBE DETAILS";
        private static string Clean(string value) { return (value ?? "--").Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ').Replace('|', '/'); }
        private static string Ms(long value) { return value < 0 ? "--" : value == 0 ? "<1 ms" : value.ToString(CultureInfo.InvariantCulture) + " ms"; }
        private static string Protocol(ProxyTypeDetector.ProxyType type) { return type == ProxyTypeDetector.ProxyType.Unknown ? "--" : type.ToString().ToUpperInvariant(); }
        public static string Line(ProxyTypeDetector.ProxyTestResult r)
        {
            return string.Join(" | ", new[] {
                Clean(r.Endpoint).PadRight(23), ExportForm.CountryKey(r.CountryCode).PadRight(7), Clean(r.Country).PadRight(24),
                Protocol(r.Type).PadRight(8), r.SecurityLabel.PadRight(26), Ms(r.TcpPingMs).PadRight(10),
                Ms(r.HttpLatencyMs).PadRight(10), Ms(r.HttpsLatencyMs).PadRight(10), Protocol(r.HttpType).PadRight(9),
                Protocol(r.HttpsType).PadRight(10), Clean(r.TlsProtocol).PadRight(11),
                (r.CheckedAt == default(DateTime) ? "--" : r.CheckedAt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                Clean(r.CountryDetail), Clean(r.Detail) });
        }
        public static string[] Document(IEnumerable<ProxyTypeDetector.ProxyTestResult> results)
        { return DocumentLines(results.Where(r => r.HttpsVerified || r.HttpWorking).GroupBy(r => r.Endpoint).OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => Line(g.Last()))); }
        internal static string[] DocumentLines(IEnumerable<string> lines)
        {
            return new[] { "SCRAPE X - Full Detail", "Country describes the endpoint IP. TCP ping is connection time, not ICMP. -- means not measured / unavailable.",
                "HTTPS VERIFIED and HTTP ONLY / TLS FAILED are explicitly distinguished. Pending country values update after background lookup.", "", Header, new string('-', 210) }.Concat(lines).ToArray();
        }
    }
}
