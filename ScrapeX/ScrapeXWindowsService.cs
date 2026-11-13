using System;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace ScrapeX
{
    internal sealed class ScrapeXWindowsService : ServiceBase
    {
        public const string InternalName = "ScrapeXServer";
        private readonly ServiceRuntime runtime = new ServiceRuntime(ServiceRuntime.DefaultFolder);
        private CancellationTokenSource stop;
        private Task worker;
        public ScrapeXWindowsService() { ServiceName = InternalName; CanStop = true; CanShutdown = true; AutoLog = false; }
        protected override void OnStart(string[] args)
        {
            stop = new CancellationTokenSource();
            worker = Task.Run(async () =>
            {
                try
                {
                    runtime.Start();
                    while (!stop.IsCancellationRequested)
                    {
                        await Task.Delay(5000, stop.Token).ConfigureAwait(false);
                        if (!runtime.IsHealthy) throw new InvalidOperationException("API or refresh loop stopped unexpectedly.");
                    }
                }
                catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
                catch (Exception ex)
                {
                    runtime.Log("Fatal service error: " + ex);
                    // A non-zero process exit allows SCM recovery to restart this service.
                    Environment.Exit(1);
                }
                finally { await runtime.StopAsync().ConfigureAwait(false); }
            });
        }
        protected override void OnStop() { RequestAdditionalTime(30000); StopWorker(); }
        protected override void OnShutdown() { StopWorker(); }
        private void StopWorker()
        {
            stop?.Cancel();
            if (worker != null && !worker.Wait(TimeSpan.FromSeconds(25)))
            {
                runtime.Log("Shutdown deadline reached; buffered checkpoints will be replayed on restart.");
                Environment.Exit(0);
            }
            stop?.Dispose();
        }
    }
}
