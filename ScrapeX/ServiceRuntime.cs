using System;
using System.IO;
using System.Threading.Tasks;

namespace ScrapeX
{
    // Independent of Form1 and the logged-in desktop. Also used by integration tests.
    public sealed class ServiceRuntime
    {
        private readonly string folder;
        private FileStream lease;
        private ProxyApiServer api;
        private RefreshCoordinator coordinator;
        private ServerRefreshPipeline pipeline;
        private readonly object logGate = new object();
        private Task stopping;
        public static string DefaultFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ScrapeX", "Service"); } }
        public bool IsHealthy { get { return api?.IsHealthy == true && coordinator?.IsHealthy == true; } }
        public ServiceRuntime(string folder) { this.folder = Path.GetFullPath(folder); }
        public void Log(string message)
        {
            lock (logGate)
            {
                try
                {
                    string path = Path.Combine(folder, "service.log");
                    if (File.Exists(path) && new FileInfo(path).Length > 5 * 1024 * 1024)
                    { string old = path + ".1"; if (File.Exists(old)) File.Delete(old); File.Move(path, old); }
                    File.AppendAllText(path, DateTime.UtcNow.ToString("O") + " " + message.Replace('\r', ' ').Replace('\n', ' ') + Environment.NewLine);
                }
                catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }
        public void Start()
        {
            Directory.CreateDirectory(folder);
            lease = new FileStream(Path.Combine(folder, "service.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            try
            {
                var profile = ServiceProfile.Load(Path.Combine(folder, "service-profile.json"), true);
                var publication = new ProxyPublication(Path.Combine(folder, "published-proxies.json"));
                try { publication.Load(); } catch (Exception ex) { Log("Publication restore: " + ex.Message); }
                pipeline = new ServerRefreshPipeline(publication, folder, SourceCatalog.Urls(profile.Sources), new Uri(profile.HttpTarget), new Uri(profile.HttpsTarget),
                    profile.Parallelism, profile.TimeoutMs, profile.RetryAlternate, profile.Fingerprint(), profile.Server.MaxAgeHours);
                pipeline.Diagnostic += Log;
                coordinator = new RefreshCoordinator(pipeline.RunAsync, Path.Combine(folder, "schedule-state.json")); coordinator.Diagnostic += Log;
                api = new ProxyApiServer(publication, profile.Server, () => new { mode = "windows-service", phase = pipeline.Phase, collected = pipeline.Collected,
                    checkedCount = pipeline.Checked, working = pipeline.Working, nextUtc = coordinator.NextUtc, history = coordinator.History });
                api.Diagnostic += Log; api.Start();
                coordinator.Start(true, TimeSpan.FromHours(profile.Server.IntervalHours));
                Log("Service ready on " + profile.Server.Prefix + "; refresh interval " + profile.Server.IntervalHours + " hours.");
            }
            catch { StopAsync().GetAwaiter().GetResult(); throw; }
        }
        public Task StopAsync()
        {
            lock (logGate) return stopping ?? (stopping = StopCoreAsync());
        }
        private async Task StopCoreAsync()
        {
            try { await Task.WhenAll(api != null ? api.StopAsync() : Task.CompletedTask, coordinator != null ? coordinator.StopAsync() : Task.CompletedTask).ConfigureAwait(false); }
            finally { lease?.Dispose(); lease = null; Log("Service stopped; publication, schedule and unfinished check retained."); }
        }
    }
}
