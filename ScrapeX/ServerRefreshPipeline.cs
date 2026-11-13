using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapeX
{
    public sealed class ServerRefreshPipeline
    {
        private readonly ProxyPublication publication;
        private readonly string folder;
        private readonly string[] sources;
        private readonly Uri http, https;
        private readonly int parallelism, timeout;
        private readonly bool retry;
        private readonly string checkpointFingerprint;
        private readonly double checkpointMaxAge;
        private int collected, checkedCount, working;
        private string phase = "Idle";
        public string Phase { get { return Volatile.Read(ref phase); } }
        public int Collected { get { return Volatile.Read(ref collected); } }
        public int Checked { get { return Volatile.Read(ref checkedCount); } }
        public int Working { get { return Volatile.Read(ref working); } }
        public event Action<string> Diagnostic;
        public ServerRefreshPipeline(ProxyPublication publication, string folder, string[] sources, Uri http, Uri https, int parallelism, int timeout, bool retry, string checkpointFingerprint = null, double checkpointMaxAge = 24)
        { this.publication = publication; this.folder = folder; this.sources = sources; this.http = http; this.https = https; this.parallelism = parallelism; this.timeout = timeout; this.retry = retry; this.checkpointFingerprint = checkpointFingerprint; this.checkpointMaxAge = checkpointMaxAge; }
        public async Task<int> RunAsync(CancellationToken token)
        {
            phase = "Waiting for current desktop operation";
            bool admitted = false;
            var checkpoint = checkpointFingerprint == null ? null : new CheckCheckpoint(folder);
            try
            {
                await RefreshCoordinator.NetworkWork.WaitAsync(token).ConfigureAwait(false); admitted = true;
                collected = checkedCount = working = 0;
                bool resumed = false;
                if (checkpoint != null)
                {
                    try { resumed = checkpoint.Restore(checkpointFingerprint, checkpointMaxAge); }
                    catch (Exception ex) when (ex is IOException || ex is Newtonsoft.Json.JsonException || ex is ArgumentException)
                    { checkpoint.Dispose(); checkpoint = new CheckCheckpoint(folder); Diagnostic?.Invoke("Checkpoint unavailable; collecting again: " + ex.Message); }
                }
                ProxyEndpoint[] ordered;
                if (resumed)
                {
                    ordered = checkpoint.Endpoints; collected = ordered.Length;
                    Diagnostic?.Invoke("Resuming checkpoint: " + checkpoint.Completed.Count + " completed, " + (ordered.Length - checkpoint.Completed.Count) + " pending.");
                }
                else
                {
                    phase = "Collecting sources";
                    var candidates = new ConcurrentDictionary<string, ProxyEndpoint>();
                    var scraper = new ProxyScraperManager(candidates);
                    scraper.OnSourceCompleted += r => { Volatile.Write(ref collected, candidates.Count); Diagnostic?.Invoke(r.ToString()); };
                    await scraper.StartScrapingAsync(sources, token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (candidates.IsEmpty) throw new IOException("No candidates returned by the configured sources.");
                    ordered = candidates.Values.ToArray(); var random = new Random();
                    for (int i = ordered.Length - 1; i > 0; i--) { int j = random.Next(i + 1); var value = ordered[i]; ordered[i] = ordered[j]; ordered[j] = value; }
                    checkpoint?.Begin(ordered, checkpointFingerprint);
                }
                string resultsFolder = Path.Combine(folder, "Server results", DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff"));
                var good = new ConcurrentBag<ProxyTypeDetector.ProxyTestResult>();
                var geoQueue = new ConcurrentQueue<ProxyTypeDetector.ProxyTestResult>();
                int finished = 0;
                phase = "Checking HTTP + TLS";
                using (var geoStop = CancellationTokenSource.CreateLinkedTokenSource(token))
                using (var resolver = new CountryResolver(Path.Combine(folder, "server-country-cache.json")))
                using (var writer = new ResultWriter(resultsFolder))
                {
                    resolver.Diagnostic += msg => Diagnostic?.Invoke(msg);
                    writer.Diagnostic += msg => Diagnostic?.Invoke(msg);
                    Func<Task> resolve = async () =>
                    {
                        try
                        {
                            while (Volatile.Read(ref finished) == 0 || !geoQueue.IsEmpty)
                            {
                                ProxyTypeDetector.ProxyTestResult result;
                                if (!geoQueue.TryDequeue(out result)) { await Task.Delay(200, geoStop.Token).ConfigureAwait(false); continue; }
                                var country = await resolver.ResolveAsync(result, geoStop.Token).ConfigureAwait(false);
                                if (country != null) { result.Country = country.Name; result.CountryCode = country.Code; writer.UpdateDetails(result); }
                            }
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex) { Diagnostic?.Invoke("Country enrichment: " + ex.Message); }
                    };
                    if (resumed)
                    {
                        checkedCount = checkpoint.Completed.Count;
                        foreach (var result in checkpoint.Working) { writer.Write(result); good.Add(result); geoQueue.Enqueue(result); working++; }
                        ordered = ordered.Where(p => !checkpoint.Completed.Contains(p.Key)).ToArray();
                    }
                    var geoTasks = new[] { Task.Run(resolve), Task.Run(resolve) };
                    try
                    {
                        var scheduler = new ProxyCheckScheduler(); scheduler.Diagnostic += msg => Diagnostic?.Invoke(msg);
                        await scheduler.RunAsync(ordered, http, https, parallelism, timeout, retry, result =>
                        {
                            writer.Write(result); checkpoint?.Record(result); Interlocked.Increment(ref checkedCount);
                            if (result.HttpsVerified || result.HttpWorking)
                            { good.Add(result); Interlocked.Increment(ref working); geoQueue.Enqueue(result); }
                        }, token).ConfigureAwait(false);
                        phase = "Finishing country lookups";
                        Volatile.Write(ref finished, 1); geoStop.CancelAfter(TimeSpan.FromMinutes(2));
                        await Task.WhenAll(geoTasks).ConfigureAwait(false);
                    }
                    finally { Volatile.Write(ref finished, 1); geoStop.Cancel(); await Task.WhenAll(geoTasks).ConfigureAwait(false); }
                    token.ThrowIfCancellationRequested();
                }
                token.ThrowIfCancellationRequested();
                phase = "Publishing snapshot";
                publication.Publish(good.Select(PublishedProxy.FromResult).Where(p => p != null));
                try { checkpoint?.Complete(); } catch (IOException ex) { Diagnostic?.Invoke("Published, but checkpoint cleanup needs retry: " + ex.Message); }
                Diagnostic?.Invoke("Published " + working + " working endpoints. Results: " + resultsFolder);
                return working;
            }
            finally
            {
                try { checkpoint?.Dispose(); }
                finally { phase = "Idle"; if (admitted) RefreshCoordinator.NetworkWork.Release(); }
            }
        }
    }
}
