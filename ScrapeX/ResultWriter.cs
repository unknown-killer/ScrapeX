using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
namespace ScrapeX
{
    public sealed class ResultWriter : IDisposable
    {
        private readonly string folder;
        private readonly object gate = new object();
        private readonly Dictionary<string, StreamWriter> writers = new Dictionary<string, StreamWriter>();
        private readonly Dictionary<string, string> details = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly object flushGate = new object();
        private readonly Timer detailTimer;
        private bool disposed;
        private long version, savedVersion = -1;
        public event Action<string> Diagnostic;
        public ResultWriter(string folder)
        {
            this.folder = folder; Directory.CreateDirectory(Path.Combine(folder, "HTTPS")); Directory.CreateDirectory(Path.Combine(folder, "HTTP_ONLY"));
            FlushDetails();
            detailTimer = new Timer(_ => { try { FlushDetails(); } catch (Exception ex) { Diagnostic?.Invoke("Full Detail save failed: " + ex.Message); } }, null, 2000, 2000);
        }
        public static string Category(ProxyTypeDetector.ProxyTestResult r) { return r.HttpsVerified ? "HTTPS" : r.HttpWorking ? "HTTP_ONLY" : null; }
        public void Write(ProxyTypeDetector.ProxyTestResult result)
        {
            string category = Category(result); if (category == null) return;
            var type = result.HttpsVerified ? result.HttpsType : result.HttpType;
            if (type == ProxyTypeDetector.ProxyType.Unknown) throw new InvalidOperationException("A successful result must include its verified protocol.");
            string relative = Path.Combine(category, type.ToString().ToUpperInvariant() + ".txt");
            lock (gate)
            {
                StreamWriter writer;
                if (!writers.TryGetValue(relative, out writer))
                {
                    writer = new StreamWriter(Path.Combine(folder, relative), false, new UTF8Encoding(false)) { AutoFlush = true };
                    writers.Add(relative, writer);
                }
                writer.WriteLine(result.Endpoint);
                details[result.Endpoint] = FullDetail.Line(result); version++;
                result.DetailWriter = this;
            }
        }
        // Country resolution can finish after checking has ended. Keep its originating session, never the current run's folder.
        public void UpdateDetails(ProxyTypeDetector.ProxyTestResult result)
        {
            bool flush;
            lock (gate)
            {
                if (!details.ContainsKey(result.Endpoint)) return;
                details[result.Endpoint] = FullDetail.Line(result); version++; flush = disposed;
            }
            if (flush) FlushDetails();
        }
        public void FlushDetails()
        {
            lock (flushGate)
            {
                string[] lines; long snapshot;
                lock (gate)
                {
                    if (version == savedVersion) return;
                    snapshot = version; lines = FullDetail.DocumentLines(details.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => p.Value));
                }
                string path = Path.Combine(folder, "Full Detail.txt"), temporary = path + ".tmp";
                File.WriteAllLines(temporary, lines, new UTF8Encoding(true));
                if (File.Exists(path)) File.Replace(temporary, path, null); else File.Move(temporary, path);
                lock (gate) savedVersion = snapshot;
            }
        }
        public void Dispose()
        {
            detailTimer.Dispose();
            lock (gate) { disposed = true; foreach (var writer in writers.Values) writer.Dispose(); writers.Clear(); }
            FlushDetails();
        }
    }
}
