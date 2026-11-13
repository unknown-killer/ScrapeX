using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace ScrapeX
{
    public sealed class CheckCheckpoint : IDisposable
    {
        private sealed class Manifest
        {
            public string Id, Fingerprint;
            public DateTime CreatedUtc;
            public string[] Endpoints;
        }
        private sealed class Entry
        {
            public string Id, Endpoint;
            public ProxyTypeDetector.ProxyTestResult Result;
        }
        private readonly string folder;
        private readonly object gate = new object();
        private Manifest manifest;
        private StreamWriter writer;
        private Timer timer;
        private Exception flushError;
        private int buffered;
        public HashSet<string> Completed { get; } = new HashSet<string>(StringComparer.Ordinal);
        public List<ProxyTypeDetector.ProxyTestResult> Working { get; } = new List<ProxyTypeDetector.ProxyTestResult>();
        public ProxyEndpoint[] Endpoints { get; private set; }
        public CheckCheckpoint(string folder) { this.folder = folder; }
        public bool Restore(string fingerprint, double maxAgeHours)
        {
            string path = Path.Combine(folder, "checkpoint.json");
            if (!File.Exists(path)) return false;
            if (new FileInfo(path).Length > 32 * 1024 * 1024) throw new IOException("Oversized checkpoint.");
            manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(path));
            if (manifest == null || manifest.Endpoints == null || manifest.Endpoints.Length > ProxyScraperManager.MaxCandidates ||
                manifest.Fingerprint != fingerprint || manifest.CreatedUtc > DateTime.UtcNow.AddMinutes(1) || manifest.CreatedUtc < DateTime.UtcNow.AddHours(-maxAgeHours)) return false;
            Endpoints = manifest.Endpoints.Select(s => { ProxyEndpoint p; if (!ProxyEndpoint.TryParse(s, out p)) throw new IOException("Invalid checkpoint endpoint."); return p; }).ToArray();
            var valid = new HashSet<string>(Endpoints.Select(p => p.Key));
            string journal = Path.Combine(folder, "check-journal.jsonl");
            if (File.Exists(journal))
            {
                if (new FileInfo(journal).Length > 512L * 1024 * 1024) throw new IOException("Oversized checkpoint journal.");
                foreach (string line in File.ReadLines(journal))
                {
                    Entry entry;
                    try { entry = JsonConvert.DeserializeObject<Entry>(line); } catch (JsonException) { continue; }
                    if (entry == null || entry.Id != manifest.Id || !valid.Contains(entry.Endpoint) || Completed.Contains(entry.Endpoint)) continue;
                    if (entry.Result != null && (entry.Result.Endpoint != entry.Endpoint || PublishedProxy.FromResult(entry.Result) == null)) continue;
                    Completed.Add(entry.Endpoint); if (entry.Result != null) Working.Add(entry.Result);
                }
            }
            // Rewrite only validated entries: a torn final line can never corrupt the next append.
            lock (gate)
            {
                OpenJournal();
                var good = Working.ToDictionary(r => r.Endpoint);
                foreach (string key in Completed) { ProxyTypeDetector.ProxyTestResult result; good.TryGetValue(key, out result); WriteEntry(key, result); }
                writer.Flush();
            }
            return true;
        }
        public void Begin(ProxyEndpoint[] endpoints, string fingerprint)
        {
            Endpoints = endpoints; Completed.Clear(); Working.Clear();
            manifest = new Manifest { Id = Guid.NewGuid().ToString("N"), CreatedUtc = DateTime.UtcNow, Fingerprint = fingerprint,
                Endpoints = endpoints.Select(p => (p.PreferredType == ProxyTypeDetector.ProxyType.Unknown ? "" : p.PreferredType.ToString().ToLowerInvariant() + "://") + p.Key).ToArray() };
            OpenJournal();
            AtomicFile.Write(Path.Combine(folder, "checkpoint.json"), JsonConvert.SerializeObject(manifest));
        }
        private void OpenJournal()
        {
            Directory.CreateDirectory(folder);
            writer = new StreamWriter(Path.Combine(folder, "check-journal.jsonl"), false, new UTF8Encoding(false));
            timer = new Timer(_ => { lock (gate) { try { writer?.Flush(); } catch (Exception ex) { flushError = ex; } } }, null, 1000, 1000);
        }
        private void WriteEntry(string endpoint, ProxyTypeDetector.ProxyTestResult result)
        { writer.WriteLine(JsonConvert.SerializeObject(new Entry { Id = manifest.Id, Endpoint = endpoint, Result = result })); }
        public void Record(ProxyTypeDetector.ProxyTestResult result)
        {
            lock (gate)
            {
                if (flushError != null) throw new IOException("Checkpoint flush failed.", flushError);
                WriteEntry(result.Endpoint, result.HttpsVerified || result.HttpWorking ? result : null);
                if (++buffered >= 64) { writer.Flush(); buffered = 0; }
            }
        }
        public void Complete()
        {
            Dispose(); File.Delete(Path.Combine(folder, "checkpoint.json"));
            File.Delete(Path.Combine(folder, "check-journal.jsonl"));
        }
        public void Dispose()
        {
            lock (gate) { timer?.Dispose(); timer = null; writer?.Dispose(); writer = null; }
        }
    }
}
