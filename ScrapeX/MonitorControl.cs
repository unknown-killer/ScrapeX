using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ScrapeX
{
    public partial class MonitorControl : UserControl
    {
        private string lastRevision;
        private int lastCount = -1;
        private DateTime lastRun;
        public MonitorControl() { InitializeComponent(); }
        public void UpdateState(ProxyPublication publication, ServerSettings settings, ProxyApiServer api, RefreshCoordinator coordinator, ServerRefreshPipeline pipeline, Func<string, Image> flag)
        {
            var snapshot = publication.Current;
            var rows = new ProxyQuery { Https = "all", MaxAgeHours = settings.MaxAgeHours }.Filter(snapshot, DateTime.UtcNow);
            availableValue.Text = rows.Length.ToString("N0"); httpsValue.Text = rows.Count(p => p.Https).ToString("N0");
            countriesValue.Text = rows.Select(p => p.CountryCode).Where(c => c != "ZZ" && !string.IsNullOrEmpty(c)).Distinct().Count().ToString("N0");
            requestsValue.Text = (api?.RequestCount ?? 0).ToString("N0");
            publishedValue.Text = snapshot.PublishedUtc.HasValue ? snapshot.PublishedUtc.Value.ToLocalTime().ToString("MMM dd · HH:mm:ss") : "No publication yet";
            nextValue.Text = coordinator?.NextUtc?.ToLocalTime().ToString("MMM dd · HH:mm:ss") ?? (coordinator?.IsRunning == true ? "Refresh in progress" : "Schedule off");
            runState.Text = (api?.IsRunning == true ? "API ONLINE" : "API OFFLINE") + "   /   " + (pipeline?.Phase ?? "Idle");
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
