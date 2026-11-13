using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ScrapeX
{
    // Exported profiles belong to the interactive user; installed profiles belong to the machine.
    // The installer restricts the machine profile directory to LocalService, SYSTEM and administrators.
    public sealed class ServiceProfile
    {
        public int Version { get; set; } = 1;
        public ServerSettings Server { get; set; } = new ServerSettings { ScheduleEnabled = true };
        public string Sources { get; set; } = SourceCatalog.Defaults;
        public string HttpTarget { get; set; } = "http://www.google.com/generate_204";
        public string HttpsTarget { get; set; } = "https://www.google.com/generate_204";
        public int Parallelism { get; set; } = 200;
        public int TimeoutMs { get; set; } = 8000;
        public bool RetryAlternate { get; set; }
        public bool MachineProtected { get; set; }
        public void Validate()
        {
            if (Version != 1 || Server == null) throw new ArgumentException("Unsupported service profile.");
            Server.Validate();
            if (!Server.ScheduleEnabled) throw new ArgumentException("Enable automatic refresh for the Windows service.");
            if (Parallelism < 1 || Parallelism > 1000 || TimeoutMs < 100 || TimeoutMs > 120000) throw new ArgumentException("Invalid service check limits.");
            var urls = SourceCatalog.Urls(Sources);
            if (urls.Length == 0 || urls.Length > SourceCatalog.MaxSources) throw new ArgumentException("Service profile needs 1–512 sources.");
            foreach (var value in urls) ValidateUrl(value, null);
            ValidateUrl(HttpTarget, "http"); ValidateUrl(HttpsTarget, "https");
        }
        private static void ValidateUrl(string value, string scheme)
        {
            Uri uri;
            if (!Uri.TryCreate(value, UriKind.Absolute, out uri) || (uri.Scheme != "http" && uri.Scheme != "https") ||
                (scheme != null && uri.Scheme != scheme) || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Fragment)) throw new ArgumentException("Invalid service source or probe URL.");
        }
        public string Fingerprint()
        {
            using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(
                string.Join("\n", SourceCatalog.Urls(Sources)) + "\n" + HttpTarget + "\n" + HttpsTarget + "\n" + TimeoutMs + "\n" + RetryAlternate)));
        }
        public void Save(string path, bool machine)
        {
            Validate(); MachineProtected = machine;
            Server.ProtectedApiKey = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(Server.ApiKey ?? ""), null,
                machine ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser));
            AtomicFile.Write(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static ServiceProfile Load(string path, bool requireMachine)
        {
            if (new FileInfo(path).Length > 4 * 1024 * 1024) throw new IOException("Service profile is too large.");
            var value = JsonConvert.DeserializeObject<ServiceProfile>(File.ReadAllText(path));
            if (value?.Server == null || value.MachineProtected != requireMachine) throw new IOException("Wrong profile protection scope. Export from the UI, then install on this machine.");
            if (string.IsNullOrEmpty(value.Server.ProtectedApiKey)) throw new IOException("Service profile has no protected API key.");
            value.Server.ApiKey = Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(value.Server.ProtectedApiKey), null,
                value.MachineProtected ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser));
            value.Validate(); return value;
        }
    }
}
