using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace ScrapeX
{
    public partial class MonitorControl : UserControl
    {
        private string lastRevision;
        private int lastCount = -1;
        private DateTime lastRun;
        public event EventHandler TargetChanged;
        public bool ServiceSelected { get { return target.SelectedIndex == 0; } }
        public MonitorControl() { InitializeComponent(); target.SelectedIndex = 0; }
        public void SelectService() { target.SelectedIndex = 0; }
        private void TargetSelectionChanged(object sender, EventArgs e)
        {
            lastRevision = null; lastCount = -1; lastRun = default(DateTime);
            ServiceUnavailable(ServiceSelected ? "Connecting…" : "Desktop", "Loading selected instance…");
            TargetChanged?.Invoke(this, EventArgs.Empty);
        }
        public void ServiceUnavailable(string state, string detail)
        {
            availableValue.Text = httpsValue.Text = countriesValue.Text = requestsValue.Text = "—";
            publishedValue.Text = nextValue.Text = "—"; runProgress.Value = 0;
            runState.Text = (ServiceSelected ? "WINDOWS SERVICE" : "DESKTOP INSTANCE") + "   /   " + state;
            runDetail.Text = detail ?? (state == "Not installed" ? "Use Install / update service in Server API to enable unattended hosting." : "Waiting for the Windows service. Desktop figures are not shown here.");
            traffic.Text = "No live data from the selected instance."; countryGrid.Rows.Clear(); historyGrid.Rows.Clear();
            lastRevision = null; lastCount = -1; lastRun = default(DateTime);
        }
        private static long Number(JToken value) { return Math.Max(0, (long?)value ?? 0); }
        private static string Time(JToken value) { return value == null || value.Type == JTokenType.Null ? "—" : ((DateTime)value).ToLocalTime().ToString("MMM dd · HH:mm:ss"); }
        public void UpdateService(JObject snapshot, Func<string, Image> flag)
        {
            var refresh = snapshot["refresh"];
            if ((string)refresh?["mode"] != "windows-service") throw new ArgumentException("Expected service telemetry.");
            var countries = (snapshot["countries"] as JArray) ?? new JArray();
            var protocols = (snapshot["protocols"] as JArray) ?? new JArray();
            availableValue.Text = Number(snapshot["available"]).ToString("N0"); httpsValue.Text = Number(snapshot["https"]).ToString("N0");
            countriesValue.Text = countries.Count(c => (string)c["code"] != "ZZ").ToString("N0"); requestsValue.Text = Number(snapshot["requests"]).ToString("N0");
            publishedValue.Text = Time(snapshot["publishedUtc"]); nextValue.Text = Time(refresh?["nextUtc"]);
            runState.Text = "WINDOWS SERVICE · RUNNING   /   " + (string)refresh?["phase"];
            long done = Number(refresh?["checkedCount"]), collected = Number(refresh?["collected"]);
            runDetail.Text = done.ToString("N0") + " / " + collected.ToString("N0") + " checked   ·   " + Number(refresh?["working"]).ToString("N0") + " working this run   ·   " + Number(snapshot["expired"]).ToString("N0") + " expired";
            runProgress.Value = collected == 0 ? 0 : (int)Math.Min(1000, 1000.0 * done / collected);
            traffic.Text = string.Join("    ", new[] { "http", "socks4", "socks5" }.Select(p => p.ToUpperInvariant() + " " + Number(protocols.FirstOrDefault(t => (string)t["type"] == p)?["count"]).ToString("N0"))) + "    /    HTTP-only " + Number(snapshot["httpOnly"]).ToString("N0") + "\r\nRejected requests " + Number(snapshot["rejected"]) + "    Errors " + Number(snapshot["errors"]) + "    Active " + Number(snapshot["active"]) + "    Sent " + (Number(snapshot["bytesSent"]) / 1024).ToString("N0") + " KB    ·    Updated " + DateTime.Now.ToString("HH:mm:ss");
            string revision = (string)snapshot["revision"]; int count = (int)Number(snapshot["available"]);
            if (lastRevision != revision || lastCount != count)
            {
                countryGrid.Rows.Clear(); lastRevision = revision; lastCount = count;
                foreach (var country in countries) countryGrid.Rows.Add(flag((string)country["code"]), (string)country["code"], (string)country["name"], Number(country["count"]), Number(country["https"]), Number(country["http"]), Number(country["socks4"]), Number(country["socks5"]));
            }
            var history = refresh?["history"]?.ToObject<RefreshRun[]>() ?? new RefreshRun[0];
            DateTime latest = history.Length == 0 ? default(DateTime) : history[0].FinishedUtc;
            if (latest != lastRun)
            {
                historyGrid.Rows.Clear(); lastRun = latest;
                foreach (var item in history) historyGrid.Rows.Add(item.StartedUtc.ToLocalTime().ToString("MM-dd HH:mm:ss"), item.Outcome, item.Working, (item.FinishedUtc - item.StartedUtc).ToString(@"hh\:mm\:ss"), item.Detail);
            }
        }
        public void UpdateState(ProxyPublication publication, ServerSettings settings, ProxyApiServer api, RefreshCoordinator coordinator, ServerRefreshPipeline pipeline, Func<string, Image> flag)
        {
            var snapshot = publication.Current;
            var rows = new ProxyQuery { Https = "all", MaxAgeHours = settings.MaxAgeHours }.Filter(snapshot, DateTime.UtcNow);
            availableValue.Text = rows.Length.ToString("N0"); httpsValue.Text = rows.Count(p => p.Https).ToString("N0");
            countriesValue.Text = rows.Select(p => p.CountryCode).Where(c => c != "ZZ" && !string.IsNullOrEmpty(c)).Distinct().Count().ToString("N0");
            requestsValue.Text = (api?.RequestCount ?? 0).ToString("N0");
            publishedValue.Text = snapshot.PublishedUtc.HasValue ? snapshot.PublishedUtc.Value.ToLocalTime().ToString("MMM dd · HH:mm:ss") : "No publication yet";
            nextValue.Text = coordinator?.NextUtc?.ToLocalTime().ToString("MMM dd · HH:mm:ss") ?? (coordinator?.IsRunning == true ? "Refresh in progress" : "Schedule off");
            runState.Text = "DESKTOP INSTANCE · " + (api?.IsRunning == true ? "API ONLINE" : "API OFFLINE") + "   /   " + (pipeline?.Phase ?? "Idle");
            runDetail.Text = (pipeline?.Checked ?? 0).ToString("N0") + " / " + (pipeline?.Collected ?? 0).ToString("N0") + " checked   ·   " + (pipeline?.Working ?? 0).ToString("N0") + " working this run   ·   " + (snapshot.Proxies.Length - rows.Length).ToString("N0") + " expired";
            runProgress.Value = pipeline == null || pipeline.Collected == 0 ? 0 : Math.Min(1000, (int)(1000L * pipeline.Checked / pipeline.Collected));
            traffic.Text = "HTTP " + rows.Count(p => p.Protocol == "http").ToString("N0") + "    SOCKS4 " + rows.Count(p => p.Protocol == "socks4").ToString("N0") + "    SOCKS5 " + rows.Count(p => p.Protocol == "socks5").ToString("N0") + "    /    HTTP-only " + rows.Count(p => !p.Https).ToString("N0") + "\r\n" +
                "Rejected requests " + (api?.DeniedCount ?? 0) + "    Errors " + (api?.ErrorCount ?? 0) + "    Active " + (api?.ActiveRequests ?? 0) + "    Sent " + ((api?.BytesSent ?? 0) / 1024).ToString("N0") + " KB";
            if (lastRevision != snapshot.Revision || lastCount != rows.Length)
            {
                lastRevision = snapshot.Revision; lastCount = rows.Length; countryGrid.Rows.Clear();
                foreach (var group in rows.GroupBy(p => p.CountryCode ?? "ZZ").OrderByDescending(g => g.Count()))
                    countryGrid.Rows.Add(flag(group.Key), group.Key, group.First().Country, group.Count(), group.Count(p => p.Https), group.Count(p => p.Protocol == "http"), group.Count(p => p.Protocol == "socks4"), group.Count(p => p.Protocol == "socks5"));
            }
            var history = coordinator?.History ?? new RefreshRun[0];
            if (history.Length == 0 && lastRun != default(DateTime)) { lastRun = default(DateTime); historyGrid.Rows.Clear(); }
            if (history.Length > 0 && history[0].FinishedUtc != lastRun)
            {
                lastRun = history[0].FinishedUtc; historyGrid.Rows.Clear();
                foreach (var item in history) historyGrid.Rows.Add(item.StartedUtc.ToLocalTime().ToString("MM-dd HH:mm:ss"), item.Outcome, item.Working, (item.FinishedUtc - item.StartedUtc).ToString(@"hh\:mm\:ss"), item.Detail);
            }
        }
    }
}
