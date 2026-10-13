using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScrapeX
{
    public static class SourceCatalog
    {
        public static string Defaults
        {
            get
            {
                using (var stream = typeof(SourceCatalog).Assembly.GetManifestResourceStream("ScrapeX.DefaultSources.txt"))
                using (var reader = new StreamReader(stream)) return reader.ReadToEnd();
            }
        }
        public static string[] Urls(string text)
        {
            return (text ?? "").Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim()).Where(s => s.Length > 0 && !s.StartsWith("#")).Distinct().ToArray();
        }
        private static string Key(string url) { return url.Trim().Replace("/refs/heads/", "/"); }
        // Add newly shipped feeds once. Remember the previous catalog so later user deletions stay deleted.
        public static string Merge(string saved, string previousDefaults, string defaults)
        {
            if (Urls(saved).Length == 0) return defaults;
            var known = new HashSet<string>(Urls(previousDefaults).Select(Key), StringComparer.Ordinal);
            foreach (string line in saved.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                known.Add(Key(line.Trim().TrimStart('#').Trim()));
            var added = Urls(defaults).Where(u => known.Add(Key(u))).ToArray();
            return saved.TrimEnd() + (added.Length == 0 ? "" : Environment.NewLine + "# New bundled sources" + Environment.NewLine + string.Join(Environment.NewLine, added)) + Environment.NewLine;
        }
        public static string Load(string userFolder, string appFolder, Action<string> diagnostic)
        {
            string defaults = Defaults;
            try
            {
                string saved = Path.Combine(userFolder, "sources.txt");
                string sidecar = Path.Combine(appFolder, "sources.txt");
                string previous = Path.Combine(userFolder, "sources.catalog.txt");
                string text = File.Exists(saved) ? File.ReadAllText(saved) : File.Exists(sidecar) ? File.ReadAllText(sidecar) : defaults;
                string merged = Merge(text, File.Exists(previous) ? File.ReadAllText(previous) : "", defaults);
                Save(userFolder, merged);
                return merged;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            { diagnostic?.Invoke("Source preferences unavailable; using embedded defaults: " + ex.Message); return defaults; }
        }
        public static void Save(string userFolder, string text)
        {
            Directory.CreateDirectory(userFolder);
            File.WriteAllText(Path.Combine(userFolder, "sources.txt"), text);
            File.WriteAllText(Path.Combine(userFolder, "sources.catalog.txt"), Defaults);
        }
    }
}
