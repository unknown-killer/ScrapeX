using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ScrapeX
{
    public static class SourceParser
    {
        private static readonly Regex Rows = Pattern(@"<tr\b[^>]*>(.*?)</tr\s*>");
        private static readonly Regex Cells = Pattern(@"<td\b[^>]*>(.*?)</td\s*>");
        private static readonly Regex TextAreas = Pattern(@"<(?:textarea|pre)\b[^>]*>(.*?)</(?:textarea|pre)\s*>");
        private static readonly Regex Tags = Pattern(@"<[^>]*>");
        private static readonly Regex Scripts = Pattern(@"<(script|style)\b[^>]*>.*?</\1\s*>");
        private static Regex Pattern(string value) { return new Regex(value, RegexOptions.IgnoreCase | RegexOptions.Singleline, TimeSpan.FromSeconds(2)); }
        public static List<ProxyEndpoint> Parse(string body, out int rawCount, string sourceUrl = null)
        {
            var result = new List<ProxyEndpoint>();
            body = (body ?? "").TrimStart('\uFEFF', ' ', '\r', '\n', '\t');
            if (body.StartsWith("{") || body.StartsWith("["))
            {
                var root = JToken.Parse(body);
                var obj = root as JObject;
                var array = root as JArray ?? obj?["data"] as JArray ?? obj?["proxies"] as JArray ?? obj?["results"] as JArray;
                if (array == null) throw new IOException("API schema changed: missing proxy array");
                rawCount = array.Count;
                foreach (var item in array)
                {
                    if (item.Type == JTokenType.String) { Add(result, (string)item, null); continue; }
                    if (!(item is JObject)) continue;
                    string username = Scalar(item["username"]), password = Scalar(item["password"]);
                    if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password)) continue;
                    string address = Scalar(item["proxy"]) ?? Scalar(item["ipPort"]);
                    if (address == null) address = (Scalar(item["ip"]) ?? Scalar(item["ip_address"]) ?? Scalar(item["addr"]) ?? Scalar(item["host"])) + ":" + Scalar(item["port"]);
                    string protocol = Scalar(item["protocol"]) ?? Scalar(item["type"]) ?? Scalar((item["protocols"] as JArray)?.FirstOrDefault());
                    Add(result, address, protocol);
                }
                return result;
            }
            if (body.StartsWith("<"))
            {
                Uri source; string host = Uri.TryCreate(sourceUrl, UriKind.Absolute, out source) ? source.Host : "";
                // Decode only documented data literals. No JavaScript is ever executed.
                if (host == "proxy-list.org")
                    foreach (Match m in Pattern(@"\bProxy\(['"" ]*([A-Za-z0-9+/=]+)['"" ]*\)").Matches(body)) Add(result, DecodeBase64(m.Groups[1].Value), null);
                // Only table cells and explicit list blocks; never evaluate scripts or scrape incidental page IPs.
                string html = Scripts.Replace(body, "");
                html = Pattern(@"<(?:div|span)\b[^>]*style=['""][^'""]*display\s*:\s*none[^'""]*['""][^>]*>.*?</(?:div|span)>").Replace(html, "");
                foreach (Match row in Rows.Matches(html))
                {
                    if (host == "advanced.name")
                    {
                        var ip = Pattern(@"data-ip=['""]([^'""]+)['""]").Match(row.Value);
                        var port = Pattern(@"data-port=['""]([^'""]+)['""]").Match(row.Value);
                        if (ip.Success && port.Success) Add(result, DecodeBase64(ip.Groups[1].Value) + ":" + DecodeBase64(port.Groups[1].Value), null);
                    }
                    var cells = Cells.Matches(row.Groups[1].Value).Cast<Match>().Select(c => WebUtility.HtmlDecode(Tags.Replace(c.Groups[1].Value, "")).Trim()).ToArray();
                    for (int i = 0; i + 1 < cells.Length; i++) Add(result, cells[i] + ":" + cells[i + 1], null);
                    foreach (string cell in cells) Add(result, Regex.Replace(cell, @"\s*:\s*", ":"), null);
                }
                if (host == "fineproxy.org")
                    foreach (Match m in Pattern(@"<div\b[^>]*class=['""]fpb-ip['""][^>]*>(.*?)</div>").Matches(html)) Add(result, Tags.Replace(m.Groups[1].Value, "").Trim(), null);
                if (host == "proxy.whatis-my-ip.com")
                    foreach (Match m in Pattern(@"data-addr=['""]([^'""]+)['""]").Matches(html)) Add(result, m.Groups[1].Value, null);
                if (host == "www.my-proxy.com")
                    foreach (Match block in Pattern(@"<div\b[^>]*class=['""]list['""][^>]*>(.*?)</div>").Matches(html))
                        ParseText(Regex.Replace(Tags.Replace(block.Groups[1].Value, "\n"), @"#[A-Za-z]{2}(?=\s|$)", ""), result);
                foreach (Match block in TextAreas.Matches(html)) ParseText(WebUtility.HtmlDecode(Tags.Replace(block.Groups[1].Value, "\n")), result);
            }
            else ParseText(body, result, sourceUrl != null && sourceUrl.IndexOf("/zloi-user/hideip.me/", StringComparison.OrdinalIgnoreCase) >= 0);
            rawCount = result.Count;
            return result;
        }
        private static string DecodeBase64(string value)
        {
            if (value.Length > 256) return "";
            try { return Encoding.ASCII.GetString(Convert.FromBase64String(value)); } catch (FormatException) { return ""; }
        }
        private static string Scalar(JToken token) { return token is JValue ? (string)token : null; }
        private static void ParseText(string body, List<ProxyEndpoint> result, bool countrySuffix = false)
        {
            using (var reader = new StringReader(body))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    ProxyEndpoint direct;
                    if (ProxyEndpoint.TryParse(line, out direct)) { result.Add(direct); continue; }
                    if (countrySuffix)
                    {
                        var suffix = Regex.Match(line, @"^((?:\d{1,3}\.){3}\d{1,3}:\d{1,5}):[^:]+$");
                        if (suffix.Success) { Add(result, suffix.Groups[1].Value, null); continue; }
                    }
                    var columns = Regex.Match(line, @"^((?:\d{1,3}\.){3}\d{1,3})\s+(\d{1,5})(?:\s|$)");
                    if (columns.Success) { Add(result, columns.Groups[1].Value + ":" + columns.Groups[2].Value, null); continue; }
                    // SPYS text lists have an endpoint followed by space-separated metadata.
                    string value = line.Split(new[] { ' ', '\t' }, 2)[0];
                    var match = Regex.Match(value, @"^(http|https|socks4|socks5):(?!//)(.+)$", RegexOptions.IgnoreCase);
                    if (match.Success) Add(result, match.Groups[2].Value, match.Groups[1].Value);
                    else Add(result, value, null);
                }
            }
        }
        private static void Add(List<ProxyEndpoint> result, string value, string protocol)
        {
            ProxyEndpoint endpoint;
            if (!ProxyEndpoint.TryParse(value, out endpoint)) return;
            if (endpoint.PreferredType == ProxyTypeDetector.ProxyType.Unknown)
            {
                if (string.Equals(protocol, "socks4", StringComparison.OrdinalIgnoreCase)) endpoint.PreferredType = ProxyTypeDetector.ProxyType.Socks4;
                else if (string.Equals(protocol, "socks5", StringComparison.OrdinalIgnoreCase)) endpoint.PreferredType = ProxyTypeDetector.ProxyType.Socks5;
                else if (string.Equals(protocol, "http", StringComparison.OrdinalIgnoreCase) || string.Equals(protocol, "https", StringComparison.OrdinalIgnoreCase)) endpoint.PreferredType = ProxyTypeDetector.ProxyType.Http;
            }
            result.Add(endpoint);
        }
    }
}
