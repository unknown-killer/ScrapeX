using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace ScrapeX
{
    public sealed class ServerSettings
    {
        public int Port { get; set; } = 8088;
        public bool ListenOnNetwork { get; set; }
        public bool AllowAnonymous { get; set; }
        public bool ScheduleEnabled { get; set; }
        public double IntervalHours { get; set; } = 6;
        public double MaxAgeHours { get; set; } = 24;
        public string PublicBaseUrl { get; set; } = "http://127.0.0.1:8088";
        [JsonIgnore] public string ApiKey { get; set; } = NewKey();
        public string ProtectedApiKey { get; set; }
        [JsonIgnore] public string Prefix { get { return "http://" + (ListenOnNetwork ? "+" : "127.0.0.1") + ":" + Port + "/"; } }
        public static string NewKey()
        {
            var bytes = new byte[32]; using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(bytes);
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
        public void Validate()
        {
            if (Port < 1024 || Port > 65535) throw new ArgumentException("Use a port between 1024 and 65535.");
            if (double.IsNaN(IntervalHours) || IntervalHours < 0.25 || IntervalHours > 168) throw new ArgumentException("Refresh interval must be 0.25–168 hours.");
            if (double.IsNaN(MaxAgeHours) || MaxAgeHours < 0.25 || MaxAgeHours > 720) throw new ArgumentException("Maximum result age must be 0.25–720 hours.");
            if (!AllowAnonymous && (ApiKey == null || ApiKey.Length < 24 || ApiKey.Length > 128)) throw new ArgumentException("Use an API key of 24–128 characters.");
            Uri uri;
            if (!Uri.TryCreate(PublicBaseUrl, UriKind.Absolute, out uri) || (uri.Scheme != "http" && uri.Scheme != "https") || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment) || uri.AbsolutePath != "/")
                throw new ArgumentException("Public address must be an http(s) origin, such as https://proxy.example.com, without a path or credentials.");
        }
        public void Save(string path)
        {
            Validate();
            ProtectedApiKey = Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(ApiKey ?? ""), null, DataProtectionScope.CurrentUser));
            AtomicFile.Write(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
        public static ServerSettings Load(string path)
        {
            if (!File.Exists(path)) return new ServerSettings();
            var settings = JsonConvert.DeserializeObject<ServerSettings>(File.ReadAllText(path));
            if (settings == null) throw new IOException("Server settings are empty.");
            if (!string.IsNullOrEmpty(settings.ProtectedApiKey)) settings.ApiKey = Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(settings.ProtectedApiKey), null, DataProtectionScope.CurrentUser));
            settings.Validate(); return settings;
        }
    }
    internal static class AtomicFile
    {
        public static void Write(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                File.WriteAllText(temp, text, new UTF8Encoding(false));
                if (File.Exists(path)) File.Replace(temp, path, null); else File.Move(temp, path);
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
    }
}
