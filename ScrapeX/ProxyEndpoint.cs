using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace ScrapeX
{
    public sealed class ProxyEndpoint
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public ProxyTypeDetector.ProxyType PreferredType { get; set; } = ProxyTypeDetector.ProxyType.Unknown;
        public string Key { get { return Host + ":" + Port; } }
        public static bool TryParse(string input, out ProxyEndpoint endpoint)
        {
            endpoint = null;
            if (string.IsNullOrWhiteSpace(input)) return false;
            var text = input.Trim();
            var preferred = ProxyTypeDetector.ProxyType.Unknown;
            var scheme = text.IndexOf("://", StringComparison.Ordinal);
            if (scheme >= 0)
            {
                var protocol = text.Substring(0, scheme).ToLowerInvariant();
                if (protocol != "http" && protocol != "https" && protocol != "socks4" && protocol != "socks5") return false;
                preferred = protocol == "socks5" ? ProxyTypeDetector.ProxyType.Socks5 : protocol == "socks4" ? ProxyTypeDetector.ProxyType.Socks4 : ProxyTypeDetector.ProxyType.Http;
                text = text.Substring(scheme + 3);
            }
            var parts = text.Split(':');
            int port;
            IPAddress address;
            if (parts.Length != 2 || !Regex.IsMatch(parts[0], @"^(\d{1,3}\.){3}\d{1,3}$") ||
                !IPAddress.TryParse(parts[0], out address) || address.AddressFamily != AddressFamily.InterNetwork ||
                !int.TryParse(parts[1], out port) || port < 1 || port > 65535) return false;
            var b = address.GetAddressBytes();
            // Public lists must not cause requests to loopback, LAN, multicast or metadata services.
            if (b[0] == 0 || b[0] == 10 || b[0] == 127 || b[0] >= 224 ||
                (b[0] == 169 && b[1] == 254) || (b[0] == 172 && b[1] >= 16 && b[1] <= 31) ||
                (b[0] == 192 && b[1] == 168) || (b[0] == 100 && b[1] >= 64 && b[1] <= 127) ||
                (b[0] == 198 && (b[1] == 18 || b[1] == 19)) ||
                (b[0] == 192 && b[1] == 0) || (b[0] == 198 && b[1] == 51 && b[2] == 100) ||
                (b[0] == 203 && b[1] == 0 && b[2] == 113)) return false;
            endpoint = new ProxyEndpoint { Host = address.ToString(), Port = port, PreferredType = preferred };
            return true;
        }
        public static IEnumerable<ProxyEndpoint> ParseLines(string text)
        {
            using (var reader = new System.IO.StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    ProxyEndpoint p;
                    if (TryParse(line, out p)) yield return p;
                }
            }
        }
    }
}
