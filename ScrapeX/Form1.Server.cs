using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScrapeX
{
    public partial class Form1
    {
        private ProxyPublication publication;
        private ServerSettings serverSettings;
        private ProxyApiServer apiServer;
        private RefreshCoordinator refreshCoordinator;
        private ServerRefreshPipeline serverPipeline;
        private bool serverStopping, serverClosePending;
        private DateTime nextMonitorUpdate;
        private bool serviceActionBusy, servicePollBusy;
        private string serviceState = "Checking…", serviceDetail;
        private DateTime nextServicePoll;
        private readonly System.Threading.CancellationTokenSource serviceMonitorStop = new System.Threading.CancellationTokenSource();
        private void InitializeServer()
        {
            try { serverSettings = ServerSettings.Load(Path.Combine(UserFolder, "server-settings.json")); }
            catch (Exception ex) { serverSettings = new ServerSettings(); Log("Server settings could not be restored: " + ex.Message); }
            publication = new ProxyPublication(Path.Combine(UserFolder, "published-proxies.json"));
            try { publication.Load(); } catch (Exception ex) { Log("Publication could not be restored: " + ex.Message); }
            serverControl.LoadSettings(serverSettings);
            serverControl.StartRequested += StartServer;
            serverControl.StopRequested += async (s, e) => await StopServerAsync();
            serverControl.RefreshRequested += (s, e) => { if (refreshCoordinator?.RequestNow() == true) Log("Server refresh requested."); };
            serverControl.CancelRequested += (s, e) => refreshCoordinator?.CancelCurrent();
            serverControl.PublishRequested += PublishDesktopResults;
            serverControl.ExportServiceRequested += ExportService;
            serverControl.ServiceActionRequested += ManageService;
            monitorControl.TargetChanged += (s, e) => { nextMonitorUpdate = nextServicePoll = DateTime.MinValue; UpdateServerState(); };
            FormClosed += (s, e) => { serviceMonitorStop.Cancel(); };
        }
        private ServiceProfile CurrentServiceProfile()
        {
            var settings = serverControl.ReadSettings(); settings.ScheduleEnabled = true;
            var profile = new ServiceProfile { Server = settings, Sources = sourcesBox.Text,
                HttpTarget = targetBox.Text.Trim(), HttpsTarget = httpsTargetBox.Text.Trim(),
                Parallelism = (int)workers.Value, TimeoutMs = (int)timeoutBox.Value, RetryAlternate = retryAlternateBox.Checked,
                MonitorUserSid = System.Security.Principal.WindowsIdentity.GetCurrent().User.Value };
            profile.Validate(); return profile;
        }
        private async void ManageService(string action)
        {
            if (serviceActionBusy) return;
            serviceActionBusy = true; serviceDetail = "Waiting for Windows administrator approval…";
            serverControl.SetServiceState(serviceState, true, serviceDetail);
            try
            {
                var profile = action == "Install" ? CurrentServiceProfile() : null;
                if (profile != null && apiServer != null && serverSettings.Port == profile.Server.Port) await StopServerAsync();
                bool success = await ServiceManager.ElevateAsync(action, profile);
                if (IsDisposed || serviceMonitorStop.IsCancellationRequested) return;
                if (success && profile != null) { profile.Server.Save(Path.Combine(UserFolder, "server-settings.json")); serverSettings = profile.Server; }
                serviceDetail = success ? action + " completed; checking live status…" : "Action failed. See the setup error; previous data is retained.";
                Log("Windows service: " + serviceDetail);
                if (success && action != "Stop") monitorControl.SelectService();
            }
            catch (Exception ex) { if (!IsDisposed) { serviceDetail = ex.Message; Notice(ex.Message); } }
            finally { serviceActionBusy = false; nextServicePoll = nextMonitorUpdate = DateTime.MinValue; if (!IsDisposed) UpdateServerState(); }
        }
        private async void PollService()
        {
            if (servicePollBusy || serviceActionBusy || serviceMonitorStop.IsCancellationRequested || DateTime.UtcNow < nextServicePoll) return;
            servicePollBusy = true; nextServicePoll = DateTime.UtcNow.AddSeconds(3);
            try
            {
                string state = await Task.Run(() => ServiceManager.Status());
                Newtonsoft.Json.Linq.JObject snapshot = null; string detail = null;
                if (state == "Running")
                {
                    try { snapshot = await ServiceMonitor.ReadAsync(serviceMonitorStop.Token); }
                    catch (Exception ex) { detail = "Statistics unavailable. Install / update service from this Windows account. " + ex.Message; }
                }
                if (IsDisposed || serviceMonitorStop.IsCancellationRequested || serviceActionBusy) return;
                serviceState = state; serviceDetail = snapshot != null ? "Live service telemetry connected" : detail;
                serverControl.SetServiceState(serviceState, false, serviceDetail);
                if (monitorPage.Visible && monitorControl.ServiceSelected)
                {
                    if (snapshot != null) monitorControl.UpdateService(snapshot, SmallFlag);
                    else monitorControl.ServiceUnavailable(state, detail);
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !serviceMonitorStop.IsCancellationRequested)
                { serviceState = "Unavailable"; serverControl.SetServiceState(serviceState, serviceActionBusy, ex.Message); if (monitorControl.ServiceSelected) monitorControl.ServiceUnavailable(serviceState, ex.Message); }
            }
            finally { servicePollBusy = false; }
        }
        private void ExportService(object sender, EventArgs e)
        {
            try
            {
                var profile = CurrentServiceProfile();
                using (var dialog = new SaveFileDialog { Filter = "Service profile (*.json)|*.json", FileName = "scrapex-service-profile.json" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    profile.Save(dialog.FileName, false);
                    Notice("Advanced service profile exported. For normal setup, use Install / update service directly in this tab; no terminal is needed.");
                }
            }
            catch (Exception ex) { Notice("Service export failed: " + ex.Message); }
        }
        private async void PublishDesktopResults(object sender, EventArgs e)
        {
            if (operation != null || refreshCoordinator?.IsRunning == true || serverStopping) { Notice("Wait for the current check to finish before publishing desktop results."); return; }
            var rows = results.Select(PublishedProxy.FromResult).Where(p => p != null).ToArray();
            if (rows.Length == 0) { Notice("Check a proxy list first. Only successful results can be published."); return; }
            await Run(token => Task.Run(() => publication.Publish(rows), token), "Publishing working results…");
        }
        private async void StartServer(object sender, EventArgs e)
        {
            if (apiServer != null || serverStopping) return;
            ProxyApiServer attemptedServer = null;
            try
            {
                var settings = serverControl.ReadSettings();
                Uri http, https;
                if (!Uri.TryCreate(targetBox.Text.Trim(), UriKind.Absolute, out http) || http.Scheme != "http" || !string.IsNullOrEmpty(http.UserInfo) || !string.IsNullOrEmpty(http.Fragment) ||
                    !Uri.TryCreate(httpsTargetBox.Text.Trim(), UriKind.Absolute, out https) || https.Scheme != "https" || !string.IsNullOrEmpty(https.UserInfo) || !string.IsNullOrEmpty(https.Fragment)) throw new ArgumentException("Set valid HTTP and HTTPS test URLs in Proxy results first.");
                string[] urls = SourceCatalog.Urls(sourcesBox.Text);
                if (urls.Length < 1 || urls.Length > SourceCatalog.MaxSources) throw new ArgumentException("Configure 1–" + SourceCatalog.MaxSources + " sources first.");
                settings.Save(Path.Combine(UserFolder, "server-settings.json")); serverSettings = settings;
                serverPipeline = new ServerRefreshPipeline(publication, UserFolder, urls, http, https, (int)workers.Value, (int)timeoutBox.Value, retryAlternateBox.Checked);
                serverPipeline.Diagnostic += message => messages.Enqueue("Server: " + message);
                refreshCoordinator = new RefreshCoordinator(serverPipeline.RunAsync);
                refreshCoordinator.Diagnostic += message => messages.Enqueue(message);
                var server = new ProxyApiServer(publication, settings, () => new { phase = serverPipeline.Phase, collected = serverPipeline.Collected, checkedCount = serverPipeline.Checked, working = serverPipeline.Working, nextUtc = refreshCoordinator.NextUtc, history = refreshCoordinator.History });
                attemptedServer = server;
                server.Diagnostic += message => messages.Enqueue(message);
                server.Start(); apiServer = server;
                refreshCoordinator.Start(settings.ScheduleEnabled, TimeSpan.FromHours(settings.IntervalHours));
                Log("API started on " + settings.Prefix + ". " + (settings.AllowAnonymous ? "Public read access enabled." : "API key required for proxy data."));
            }
            catch (Exception ex)
            {
                try { if (attemptedServer != null) await attemptedServer.StopAsync(); }
                catch (Exception cleanup) { Log("Server cleanup: " + cleanup.Message); }
                try { if (refreshCoordinator != null) await refreshCoordinator.StopAsync(); }
                catch (Exception cleanup) { Log("Schedule cleanup: " + cleanup.Message); }
                apiServer = null; refreshCoordinator = null;
                Notice(ex.Message); Log("Server start failed: " + ex.Message);
            }
            UpdateServerState();
        }
        private async Task StopServerAsync()
        {
            if (serverStopping) return; serverStopping = true; UpdateServerState();
            try
            {
                var coordinator = refreshCoordinator; var api = apiServer;
                await Task.WhenAll(coordinator != null ? coordinator.StopAsync() : Task.CompletedTask, api != null ? api.StopAsync() : Task.CompletedTask);
                Log("Server stopped. Published snapshot retained on disk.");
            }
            catch (Exception ex) { Log("Server shutdown: " + ex.Message); }
            finally
            {
                apiServer = null; refreshCoordinator = null; serverStopping = false; UpdateServerState();
                if (serverClosePending && !IsDisposed) BeginInvoke(new Action(Close));
            }
        }
        private void UpdateServerState()
        {
            if (publication == null || DateTime.UtcNow < nextMonitorUpdate) return;
            nextMonitorUpdate = DateTime.UtcNow.AddSeconds(1);
            bool running = apiServer?.IsRunning == true;
            serverControl.SetState(running, refreshCoordinator?.IsRunning == true, serverStopping,
                serverStopping ? "STOPPING…" : running ? "ONLINE  ·  " + serverSettings.Prefix : "OFFLINE  ·  Start the API to serve your published list");
            if (monitorPage.Visible && !monitorControl.ServiceSelected) monitorControl.UpdateState(publication, serverSettings, apiServer, refreshCoordinator, serverPipeline, SmallFlag);
            if (serverPage.Visible || monitorPage.Visible) PollService();
        }
        private void ServerTabClick(object sender, EventArgs e) { ShowPage(serverPage); }
        private void MonitorTabClick(object sender, EventArgs e) { ShowPage(monitorPage); nextMonitorUpdate = DateTime.MinValue; UpdateServerState(); }
        private async void ServerClosing(object sender, FormClosingEventArgs e)
        {
            if (serviceActionBusy) { e.Cancel = true; Notice("Wait for the Windows service action to finish before closing ScrapeX."); return; }
            if (serverClosePending) { if (apiServer != null || serverStopping) e.Cancel = true; return; }
            if (apiServer == null && !serverStopping) return;
            e.Cancel = true; serverClosePending = true;
            await StopServerAsync();
            if (!IsDisposed) Close();
        }
    }
}
