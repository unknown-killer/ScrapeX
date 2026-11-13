using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ScrapeX
{
    public partial class ServerControl : UserControl
    {
        public event EventHandler StartRequested, StopRequested, RefreshRequested, CancelRequested, PublishRequested;
        public ServerControl()
        {
            InitializeComponent();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime) return;
            feedType.SelectedIndex = 0; feedFormat.SelectedIndex = 0; feedHttps.SelectedIndex = 0;
            UpdateLink(null, EventArgs.Empty);
        }
        public void LoadSettings(ServerSettings value)
        {
            port.Value = value.Port; network.Checked = value.ListenOnNetwork; anonymous.Checked = value.AllowAnonymous;
            schedule.Checked = value.ScheduleEnabled; interval.Value = (decimal)value.IntervalHours; maxAge.Value = (decimal)value.MaxAgeHours;
            publicUrl.Text = value.PublicBaseUrl; apiKey.Text = value.ApiKey; UpdateLink(null, EventArgs.Empty);
        }
        public ServerSettings ReadSettings()
        {
            var value = new ServerSettings { Port = (int)port.Value, ListenOnNetwork = network.Checked, AllowAnonymous = anonymous.Checked,
                ScheduleEnabled = schedule.Checked, IntervalHours = (double)interval.Value, MaxAgeHours = (double)maxAge.Value,
                PublicBaseUrl = publicUrl.Text.Trim().TrimEnd('/'), ApiKey = apiKey.Text.Trim() };
            value.Validate(); return value;
        }
        public void SetState(bool running, bool refreshing, bool stopping, string status)
        {
            foreach (Control card in new Control[] { connectionCard, automationCard })
                foreach (Control control in card.Controls)
                {
                    if (control is Label) continue;
                    var box = control as TextBox;
                    if (box != null) box.ReadOnly = running || stopping;
                    else control.Enabled = !running && !stopping;
                    if (control is CheckBox || control is NumericUpDown)
                        control.BackColor = control.Enabled ? Color.FromArgb(20, 29, 44) : Color.FromArgb(190, 201, 215);
                }
            copyKey.Enabled = true;
            start.Enabled = !running && !stopping; stop.Enabled = running && !stopping;
            runNow.Enabled = running && !refreshing && !stopping; cancelRun.Enabled = refreshing && !stopping;
            publish.Enabled = !refreshing && !stopping;
            serverState.Text = status; serverState.ForeColor = running ? Color.FromArgb(71, 227, 192) : Color.FromArgb(163, 181, 204);
        }
        private void UpdateLink(object sender, EventArgs e)
        {
            if (feedType.SelectedIndex < 0 || feedFormat.SelectedIndex < 0 || feedHttps.SelectedIndex < 0) return;
            string country = string.IsNullOrWhiteSpace(feedCountry.Text) ? "all" : feedCountry.Text.Trim().ToUpperInvariant();
            string secure = new[] { "true", "false", "all" }[feedHttps.SelectedIndex];
            feedLink.Text = publicUrl.Text.Trim().TrimEnd('/') + "/api/v1/proxies?format=" + feedFormat.SelectedItem + "&type=" + feedType.SelectedItem + "&country=" + Uri.EscapeDataString(country) + "&https=" + secure + "&limit=" + feedLimit.Value;
            authHint.Text = anonymous.Checked ? "Public mode: this URL opens directly in a browser." : "Protected mode: add X-Api-Key in your client. The link contains no secret.";
        }
        private void StartClick(object s, EventArgs e) { StartRequested?.Invoke(this, e); }
        private void StopClick(object s, EventArgs e) { StopRequested?.Invoke(this, e); }
        private void RunClick(object s, EventArgs e) { RefreshRequested?.Invoke(this, e); }
        private void CancelClick(object s, EventArgs e) { CancelRequested?.Invoke(this, e); }
        private void PublishClick(object s, EventArgs e) { PublishRequested?.Invoke(this, e); }
        private void CopyLink(object s, EventArgs e) { Copy(feedLink.Text); }
        private void CopyKey(object s, EventArgs e) { Copy(apiKey.Text); }
        private void Copy(string value) { try { Clipboard.SetText(value); } catch (System.Runtime.InteropServices.ExternalException) { MessageBox.Show(this, "Clipboard is busy. Try again.", "ScrapeX"); } }
        private void NewKey(object s, EventArgs e) { apiKey.Text = ServerSettings.NewKey(); }
        private void MutedButton(object sender, PaintEventArgs e)
        {
            var button = (Button)sender; if (button.Enabled) return;
            e.Graphics.Clear(Color.FromArgb(42, 53, 70));
            TextRenderer.DrawText(e.Graphics, button.Text, button.Font, button.ClientRectangle, Color.FromArgb(180, 192, 210), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
