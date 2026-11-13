using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace ScrapeX
{
    public sealed class RefreshRun
    {
        public DateTime StartedUtc { get; set; }
        public DateTime FinishedUtc { get; set; }
        public string Outcome { get; set; }
        public int Working { get; set; }
        public string Detail { get; set; }
    }
    public sealed class RefreshCoordinator
    {
        // Manual checks and automatic cycles share admission to the network workload.
        public static readonly SemaphoreSlim NetworkWork = new SemaphoreSlim(1, 1);
        private readonly Func<CancellationToken, Task<int>> refresh;
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private readonly SemaphoreSlim wake = new SemaphoreSlim(0, 1);
        private readonly object sync = new object();
        private readonly List<RefreshRun> history = new List<RefreshRun>();
        private Task loop;
        private CancellationTokenSource cycle;
        private bool enabled, requested, running;
        private TimeSpan interval;
        private DateTime? nextUtc;
        private readonly string statePath;
        private sealed class SavedState { public DateTime? NextUtc; public bool Running; public RefreshRun[] History; }
        public bool IsHealthy { get { lock (sync) return loop != null && !loop.IsCompleted; } }
        public bool IsRunning { get { lock (sync) return running; } }
        public DateTime? NextUtc { get { lock (sync) return nextUtc; } }
        public RefreshRun[] History { get { lock (sync) return history.ToArray(); } }
        public event Action<string> Diagnostic;
        public RefreshCoordinator(Func<CancellationToken, Task<int>> refresh, string statePath = null) { this.refresh = refresh; this.statePath = statePath; }
        public void Start(bool enabled, TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));
            lock (sync)
            {
                if (loop != null) throw new InvalidOperationException("Already started."); this.enabled = enabled; this.interval = interval; nextUtc = enabled ? (DateTime?)DateTime.UtcNow : null;
                if (enabled && statePath != null && File.Exists(statePath))
                {
                    try
                    {
                        if (new FileInfo(statePath).Length > 1024 * 1024) throw new IOException("Schedule state is too large.");
                        var saved = JsonConvert.DeserializeObject<SavedState>(File.ReadAllText(statePath));
                        if (saved != null)
                        {
                            history.AddRange((saved.History ?? new RefreshRun[0]).Where(r => r != null).Take(50));
                            if (!saved.Running && saved.NextUtc.HasValue) nextUtc = saved.NextUtc.Value > DateTime.UtcNow.Add(interval) ? DateTime.UtcNow : saved.NextUtc;
                        }
                    }
                    catch (Exception ex) { Diagnostic?.Invoke("Schedule state unavailable; starting a new cycle: " + ex.Message); }
                }
                loop = Task.Run(LoopAsync);
            }
        }
        private void SaveState(bool interrupted = false)
        {
            if (statePath == null) return;
            try { AtomicFile.Write(statePath, JsonConvert.SerializeObject(new SavedState { Running = running || interrupted, NextUtc = nextUtc, History = history.ToArray() })); }
            catch (Exception ex) { Diagnostic?.Invoke("Could not persist service schedule: " + ex.Message); }
        }
        public bool RequestNow()
        {
            lock (sync) { if (running || requested || lifetime.IsCancellationRequested) return false; requested = true; if (wake.CurrentCount == 0) wake.Release(); return true; }
        }
        public void CancelCurrent() { lock (sync) cycle?.Cancel(); }
        private async Task LoopAsync()
        {
            try
            {
                while (!lifetime.IsCancellationRequested)
                {
                    int wait;
                    lock (sync) wait = requested ? 0 : nextUtc.HasValue ? (int)Math.Min(int.MaxValue, Math.Max(0, (nextUtc.Value - DateTime.UtcNow).TotalMilliseconds)) : Timeout.Infinite;
                    if (wait != 0) { await wake.WaitAsync(wait, lifetime.Token).ConfigureAwait(false); continue; }
                    var run = new RefreshRun { StartedUtc = DateTime.UtcNow };
                    CancellationToken token;
                    lock (sync) { requested = false; running = true; nextUtc = null; cycle = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token); token = cycle.Token; SaveState(); }
                    try { run.Working = await refresh(token).ConfigureAwait(false); run.Outcome = "Published"; }
                    catch (OperationCanceledException) { run.Outcome = "Cancelled"; run.Detail = "Previous publication retained."; }
                    catch (Exception ex) { run.Outcome = "Failed"; run.Detail = ex.Message; Diagnostic?.Invoke("Scheduled refresh failed; previous publication retained: " + ex.Message); }
                    finally
                    {
                        run.FinishedUtc = DateTime.UtcNow;
                        lock (sync)
                        {
                            history.Insert(0, run); if (history.Count > 50) history.RemoveAt(history.Count - 1);
                            cycle.Dispose(); cycle = null; running = false;
                            var delay = run.Outcome == "Failed" && statePath != null && interval > TimeSpan.FromMinutes(5) ? TimeSpan.FromMinutes(5) : interval;
                            nextUtc = enabled ? (DateTime?)DateTime.UtcNow.Add(delay) : null;
                            SaveState(run.Outcome == "Cancelled" && lifetime.IsCancellationRequested);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
        }
        public async Task StopAsync()
        {
            lifetime.Cancel(); if (loop != null) await loop.ConfigureAwait(false);
            lifetime.Dispose(); wake.Dispose();
        }
    }
}
