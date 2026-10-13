using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace ScrapeX
{
    // Independent bounded stages: slow TCP admission never occupies a protocol worker.
    public sealed class ProxyCheckScheduler
    {
        public int Connecting { get { return Volatile.Read(ref connecting); } }
        public int Probing { get { return Volatile.Read(ref probing); } }
        public int SocketBudget { get { return budget == null ? 0 : budget.Limit; } }
        public int CapacityRetries { get { return Volatile.Read(ref capacityRetries); } }
        public event Action<string> Diagnostic;
        private AdaptiveBudget budget;
        private int capacityRetries;
        internal Func<ProxyEndpoint, int, CancellationToken, Task<ProxyTypeDetector.ProxyTestResult>> TcpProbe = ProxyTypeDetector.CheckTcpAsync;
        internal Func<ProxyEndpoint, Uri, Uri, int, CancellationToken, bool, Task<ProxyTypeDetector.ProxyTestResult>> ProtocolProbe =
            (p, h, s, t, c, r) => new ProxyTypeDetector().CheckAsync(p, h, s, t, c, r, true);
        private int connecting, probing;
        private sealed class AdaptiveBudget
        {
            private readonly object sync = new object();
            private int limit, used;
            private DateTime pauseUntil;
            private TaskCompletionSource<bool> changed = Signal();
            private static TaskCompletionSource<bool> Signal() { return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously); }
            public AdaptiveBudget(int limit) { this.limit = limit; }
            public int Limit { get { lock (sync) return limit; } }
            private void Pulse() { var old = changed; changed = Signal(); old.TrySetResult(true); }
            public async Task Enter(int weight, CancellationToken token)
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested(); Task wait; int delay;
                    lock (sync)
                    {
                        delay = (int)Math.Max(0, (pauseUntil - DateTime.UtcNow).TotalMilliseconds);
                        if (delay == 0 && used + weight <= limit) { used += weight; return; }
                        wait = changed.Task;
                    }
                    await Task.WhenAny(wait, Task.Delay(delay > 0 ? Math.Min(delay, 250) : 100, token)).ConfigureAwait(false);
                }
            }
            public void Exit(int weight) { lock (sync) { used -= weight; Pulse(); } }
            public bool Reduce()
            {
                lock (sync)
                {
                    if (pauseUntil > DateTime.UtcNow) return false;
                    limit = Math.Max(6, limit / 2); pauseUntil = DateTime.UtcNow.AddSeconds(2); Pulse(); return true;
                }
            }
        }
        private async Task<ProxyTypeDetector.ProxyTestResult> WithCapacityRecovery(int weight, Func<Task<ProxyTypeDetector.ProxyTestResult>> action, CancellationToken token)
        {
            while (true)
            {
                await budget.Enter(weight, token).ConfigureAwait(false);
                try { return await action().ConfigureAwait(false); }
                catch (Exception ex) when (ProxyTypeDetector.IsLocalCapacityFailure(ex))
                {
                    token.ThrowIfCancellationRequested(); Interlocked.Increment(ref capacityRetries);
                    if (budget.Reduce()) Diagnostic?.Invoke("Socket pressure: reduced shared budget to " + budget.Limit + "; cooling down and retrying pending endpoints. No failed verdict recorded.");
                }
                finally { budget.Exit(weight); }
            }
        }
        public async Task RunAsync(IEnumerable<ProxyEndpoint> endpoints, Uri http, Uri https, int parallelism, int timeoutMs, bool retry, Action<ProxyTypeDetector.ProxyTestResult> publish, CancellationToken token)
        {
            if (parallelism < 1 || parallelism > 500) throw new ArgumentOutOfRangeException(nameof(parallelism));
            budget = new AdaptiveBudget(Math.Min(1024, parallelism * 9)); capacityRetries = 0;
            var input = new ConcurrentQueue<ProxyEndpoint>(endpoints);
            var ready = new ConcurrentQueue<ProxyEndpoint>();
            int tcpWorkers = Math.Min(input.Count, Math.Min(1000, parallelism * 3));
            int probeWorkers = Math.Min(input.Count, parallelism);
            if (tcpWorkers == 0) return;
            int producers = tcpWorkers;
            Exception failure = null;
            using (var stop = CancellationTokenSource.CreateLinkedTokenSource(token))
            using (var slots = new SemaphoreSlim(parallelism * 2))
            using (var available = new SemaphoreSlim(0))
            {
                var tasks = new List<Task>();
                for (int i = 0; i < probeWorkers; i++) tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            await available.WaitAsync(stop.Token).ConfigureAwait(false);
                            ProxyEndpoint endpoint;
                            if (!ready.TryDequeue(out endpoint)) { if (Volatile.Read(ref producers) == 0) break; continue; }
                            slots.Release(); Interlocked.Increment(ref probing);
                            try { publish(await WithCapacityRecovery(6, () => ProtocolProbe(endpoint, http, https, timeoutMs, stop.Token, retry), stop.Token).ConfigureAwait(false)); }
                            finally { Interlocked.Decrement(ref probing); }
                        }
                    }
                    catch (Exception ex) { if (!(ex is OperationCanceledException)) Interlocked.CompareExchange(ref failure, ex, null); stop.Cancel(); throw; }
                }, CancellationToken.None));
                for (int i = 0; i < tcpWorkers; i++) tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        ProxyEndpoint endpoint;
                        while (input.TryDequeue(out endpoint))
                        {
                            stop.Token.ThrowIfCancellationRequested(); Interlocked.Increment(ref connecting);
                            ProxyTypeDetector.ProxyTestResult rejected;
                            try { rejected = await WithCapacityRecovery(1, () => TcpProbe(endpoint, timeoutMs, stop.Token), stop.Token).ConfigureAwait(false); }
                            finally { Interlocked.Decrement(ref connecting); }
                            if (rejected != null) { publish(rejected); continue; }
                            // Backpressure is an async wait, never a blocked OS thread or a reduced probe timeout.
                            await slots.WaitAsync(stop.Token).ConfigureAwait(false);
                            ready.Enqueue(endpoint); available.Release();
                        }
                    }
                    catch (Exception ex) { if (!(ex is OperationCanceledException)) Interlocked.CompareExchange(ref failure, ex, null); stop.Cancel(); throw; }
                    finally { if (Interlocked.Decrement(ref producers) == 0) available.Release(probeWorkers); }
                }, CancellationToken.None));
                try { await Task.WhenAll(tasks).ConfigureAwait(false); }
                catch { if (failure != null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw(); throw; }
                token.ThrowIfCancellationRequested();
            }
        }
    }
}
