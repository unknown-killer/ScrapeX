using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace ScrapeX
{
    public partial class ExportForm : Form
    {
        private sealed class CountryChoice
        {
            public string Code; public string Label; public ProxyTypeDetector.ProxyTestResult[] Results;
            public override string ToString() { return Code + "  ·  " + Label + " (" + Results.Length + ")"; }
        }
        private readonly bool secure;
        public ExportForm() { InitializeComponent(); }
        public ExportForm(ProxyTypeDetector.ProxyTestResult[] results, bool secure) : this()
        {
            this.secure = secure;
            titleLabel.Text = secure ? "Export · HTTPS verified" : "Export · HTTP only (TLS failed)";
            foreach (var group in results.GroupBy(r => CountryKey(r.CountryCode)).OrderBy(g => g.Key))
                countries.Items.Add(new CountryChoice { Code = group.Key, Label = group.Key == "UNKNOWN" ? "Country not resolved" : group.First().Country, Results = group.ToArray() }, true);
            protocol.SelectedIndex = 0; UpdateSummary();
        }
        public static string CountryKey(string code)
        { return code != null && System.Text.RegularExpressions.Regex.IsMatch(code, "^[A-Za-z]{2}$") ? code.ToUpperInvariant() : "UNKNOWN"; }
        private IEnumerable<ProxyTypeDetector.ProxyTestResult> Filter(CountryChoice choice)
        { return choice.Results.Where(r => protocol.SelectedIndex <= 0 || (secure ? r.HttpsType : r.HttpType).ToString().Equals((string)protocol.SelectedItem, StringComparison.OrdinalIgnoreCase)); }
        private void UpdateSummary()
        {
            int count = countries.CheckedItems.Cast<CountryChoice>().Sum(c => Filter(c).Count());
            summary.Text = count.ToString("N0") + " proxies · " + countries.CheckedItems.Count + " countries\nUnknown countries remain in an explicit UNKNOWN group.";
            saveButton.Enabled = count > 0;
        }
        private void CountryChanged(object sender, ItemCheckEventArgs e) { if (IsHandleCreated) BeginInvoke(new Action(UpdateSummary)); }
        private void SelectionChanged(object sender, EventArgs e) { UpdateSummary(); }
        private void SelectAll(object sender, EventArgs e) { for (int i = 0; i < countries.Items.Count; i++) countries.SetItemChecked(i, true); UpdateSummary(); }
        private void SelectNone(object sender, EventArgs e) { for (int i = 0; i < countries.Items.Count; i++) countries.SetItemChecked(i, false); UpdateSummary(); }
        private string[] Lines(IEnumerable<ProxyTypeDetector.ProxyTestResult> results)
        { return fullDetail.Checked ? FullDetail.Document(results) : results.Select(r => (includeScheme.Checked ? (secure ? r.HttpsType : r.HttpType).ToString().ToLowerInvariant() + "://" : "") + r.Endpoint).Distinct().OrderBy(s => s).ToArray(); }
        private void DetailFormatChanged(object sender, EventArgs e) { includeScheme.Visible = !fullDetail.Checked; }
        private void DisabledButtonPaint(object sender, PaintEventArgs e)
        {
            var button = (Button)sender;
            if (button.Enabled) return;
            e.Graphics.Clear(System.Drawing.Color.FromArgb(42, 53, 70));
            TextRenderer.DrawText(e.Graphics, button.Text, button.Font, button.ClientRectangle, System.Drawing.Color.FromArgb(180, 192, 210), TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
        private void SaveExport(object sender, EventArgs e)
        {
            var choices = countries.CheckedItems.Cast<CountryChoice>().Where(c => Filter(c).Any()).ToArray();
            if (choices.Length == 0) return;
            string category = secure ? "HTTPS" : "HTTP_ONLY_TLS_FAILED";
            try
            {
                if (splitCountries.Checked)
                {
                    using (var dialog = new FolderBrowserDialog { Description = "Choose parent folder for the country export" })
                    {
                        if (dialog.ShowDialog(this) != DialogResult.OK) return;
                        string path = Path.Combine(dialog.SelectedPath, "ScrapeX-" + category + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
                        Directory.CreateDirectory(path);
                        foreach (var country in choices) File.WriteAllLines(Path.Combine(path, country.Code + ".txt"), Lines(Filter(country)), new UTF8Encoding(false));
                        summary.Text = "Saved " + choices.Length + " files to:\n" + path;
                    }
                }
                else using (var dialog = new SaveFileDialog { Filter = "Proxy list (*.txt)|*.txt", FileName = (fullDetail.Checked ? "Full Detail-" : "") + category + "-selected-countries.txt" })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    File.WriteAllLines(dialog.FileName, Lines(choices.SelectMany(Filter)), new UTF8Encoding(false));
                    summary.Text = "Saved to:\n" + dialog.FileName;
                }
            }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
