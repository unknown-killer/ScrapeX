using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScrapeX
{
    public partial class Form1 : Form
    {
        private readonly ConcurrentDictionary<string, ProxyEndpoint> candidates = new ConcurrentDictionary<string, ProxyEndpoint>();
        private readonly ConcurrentQueue<ProxyTypeDetector.ProxyTestResult> incoming = new ConcurrentQueue<ProxyTypeDetector.ProxyTestResult>();
        private readonly ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
        private readonly List<ProxyTypeDetector.ProxyTestResult> results = new List<ProxyTypeDetector.ProxyTestResult>();
        private readonly List<ProxyTypeDetector.ProxyTestResult> visible = new List<ProxyTypeDetector.ProxyTestResult>();
        private readonly Stopwatch watch = new Stopwatch();
        private readonly System.Windows.Forms.Timer refresh = new System.Windows.Forms.Timer { Interval = 250 };
        private CancellationTokenSource operation;
        private int checkedCount, goodCount, httpOnlyCount, failedCount, totalCount;
        private int httpCount, socks4Count, socks5Count;
        private readonly CancellationTokenSource metadataLifetime = new CancellationTokenSource();
        private readonly ConcurrentQueue<ProxyTypeDetector.ProxyTestResult> countryQueue = new ConcurrentQueue<ProxyTypeDetector.ProxyTestResult>();
        private CountryResolver countryResolver;
        private readonly Dictionary<ProxyTypeDetector.ProxyTestResult, Tuple<int, DateTime>> countryRetries = new Dictionary<ProxyTypeDetector.ProxyTestResult, Tuple<int, DateTime>>();
        private int countryGeneration;
        private ProxyCheckScheduler scheduler;
        private Task countryTask;
        private static readonly System.Drawing.Bitmap emptyFlag = new System.Drawing.Bitmap(20, 13);
        private bool closeAfterStop;
        private bool checking;
        private string outputFolder;
        private static readonly string UserFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ScrapeX");
        public Form1()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime) return;
            filterBox.SelectedIndex = 0;
            countryResolver = new CountryResolver(Path.Combine(UserFolder, "country-cache.json"));
            countryResolver.Diagnostic += message => messages.Enqueue(message);
            countryTask = Task.WhenAll(ProcessCountriesAsync(metadataLifetime.Token), ProcessCountriesAsync(metadataLifetime.Token));
            refresh.Tick += (s, e) => RefreshState();
            refresh.Start();
            FormClosing += HandleClosing;
            Shown += (s, e) =>
            {
                var screen = Screen.FromControl(this).WorkingArea;
                MaximizedBounds = screen;
                if (Width > screen.Width || Height > screen.Height) WindowState = FormWindowState.Maximized;
            };
            FormClosed += (s, e) => { refresh.Dispose(); foreach (var flag in smallFlags.Values) flag.Dispose(); metadataLifetime.Cancel(); countryTask.ContinueWith(t => { countryResolver.Dispose(); metadataLifetime.Dispose(); }); };
            try
            {
                sourcesBox.Text = SourceCatalog.Load(UserFolder, AppDomain.CurrentDomain.BaseDirectory, Log);
            }
            catch (Exception ex) { Log("Could not load sources: " + ex.Message); }
            SetBusy(false);
        }
        private async Task Run(Func<CancellationToken, Task> action, string label)
        {
            if (operation != null) return;
            operation = new CancellationTokenSource();
            SetBusy(true); watch.Restart(); statusLabel.Text = label;
            try { await action(operation.Token); statusLabel.Text = "Completed"; }
            catch (OperationCanceledException) { statusLabel.Text = "Stopped — partial results retained"; }
            catch (Exception ex) { statusLabel.Text = "Operation failed — see activity"; Log(ex.ToString()); }
            finally
            {
                checking = false;
                watch.Stop(); while (!incoming.IsEmpty) RefreshState(); RefreshState(); operation.Dispose(); operation = null; SetBusy(false);
                if (closeAfterStop) Close();
            }
        }
        private async void ScrapeClick(object sender, EventArgs e)
        {
            var urls = sourcesBox.Lines.Select(x => x.Trim()).Where(x => x.Length > 0 && !x.StartsWith("#")).ToArray();
            if (urls.Length == 0 || urls.Length > 200) { Notice("Enter between 1 and 200 source URLs."); return; }
            await Run(async token =>
            {
                var scraper = new ProxyScraperManager(candidates);
                scraper.OnSourceCompleted += r => messages.Enqueue(r.ToString());
                await Task.Run(() => scraper.StartScrapingAsync(urls, token), token);
                Log("Source run finished. " + candidates.Count.ToString("N0") + " unique candidates.");
            }, "Collecting proxies…");
        }
        private async void CheckClick(object sender, EventArgs e)
        {
            Uri target, httpsTarget;
            if (!Uri.TryCreate(targetBox.Text.Trim(), UriKind.Absolute, out target) ||
                target.Scheme != "http" || !string.IsNullOrEmpty(target.UserInfo) || !string.IsNullOrEmpty(target.Fragment))
            { Notice("The HTTP probe must use an http:// URL without credentials or a fragment."); return; }
            if (!Uri.TryCreate(httpsTargetBox.Text.Trim(), UriKind.Absolute, out httpsTarget) || httpsTarget.Scheme != "https" || !string.IsNullOrEmpty(httpsTarget.UserInfo) || !string.IsNullOrEmpty(httpsTarget.Fragment))
            { Notice("The mandatory TLS probe must use an https:// URL without credentials or a fragment."); return; }
            var sourceUrls = SourceCatalog.Urls(sourcesBox.Text);
            if (candidates.IsEmpty && sourceUrls.Length == 0) { sourcesBox.Text = SourceCatalog.Defaults; sourceUrls = SourceCatalog.Urls(sourcesBox.Text); }
            if (candidates.IsEmpty && sourceUrls.Length > 200) { Notice("Enter at most 200 source URLs."); return; }
            int concurrency = (int)workers.Value, timeout = (int)timeoutBox.Value;
            bool lookupCountries = autoCountryBox.Checked;
            bool retryAlternate = retryAlternateBox.Checked;
            await Run(async token =>
            {
                if (candidates.IsEmpty)
                {
                    statusLabel.Text = "Collecting default sources before checking…";
                    var scraper = new ProxyScraperManager(candidates);
                    scraper.OnSourceCompleted += r => messages.Enqueue(r.ToString());
                    await Task.Run(() => scraper.StartScrapingAsync(sourceUrls, token), token);
                    if (candidates.IsEmpty) throw new IOException("No endpoints returned by sources. See Activity for download errors.");
                    statusLabel.Text = "Testing proxies…";
                }
                watch.Restart(); checking = true;
                results.Clear(); visible.Clear(); httpCount = socks4Count = socks5Count = 0; grid.RowCount = 0;
                checkedCount = goodCount = httpOnlyCount = failedCount = 0;
                ProxyTypeDetector.ProxyTestResult oldCountry; while (countryQueue.TryDequeue(out oldCountry)) { }
                countryGeneration++; countryRetries.Clear(); totalCount = candidates.Count;
                var ordered = await Task.Run(() =>
                {
                    var array = candidates.Values.ToArray(); var random = new Random();
                    for (int i = array.Length - 1; i > 0; i--) { int j = random.Next(i + 1); var item = array[i]; array[i] = array[j]; array[j] = item; }
                    return array;
                }, token);
                outputFolder = Path.Combine(UserFolder, "Results", DateTime.Now.ToString("yyyyMMdd-HHmmss-fff"));
                Directory.CreateDirectory(outputFolder);
                File.WriteAllText(Path.Combine(outputFolder, "session.txt"), "HTTP target: " + target + Environment.NewLine + "HTTPS target: " + httpsTarget + Environment.NewLine + "TCP / probe stage timeout: " + timeout + " ms; protocol workers: " + concurrency + "; alternate retries: " + retryAlternate + Environment.NewLine + "Latency measures connection + handshake + HTTP response headers, not ICMP ping.");
                using (var writer = new ResultWriter(outputFolder))
                {
                    writer.Diagnostic += message => messages.Enqueue(message);
                    scheduler = new ProxyCheckScheduler();
                    scheduler.Diagnostic += message => messages.Enqueue(message);
                    await scheduler.RunAsync(ordered, target, httpsTarget, concurrency, timeout, retryAlternate, result =>
                    {
                        result.Country = lookupCountries ? "Queued" : "Lookup disabled";
                        writer.Write(result);
                        if (result.HttpsVerified) Interlocked.Increment(ref goodCount);
                        else if (result.HttpWorking) Interlocked.Increment(ref httpOnlyCount);
                        else Interlocked.Increment(ref failedCount);
                        if (lookupCountries && result.Type != ProxyTypeDetector.ProxyType.Unknown)
                        { result.Country = "Queued"; countryQueue.Enqueue(result); }
                        incoming.Enqueue(result); Interlocked.Increment(ref checkedCount);
                    }, token);
                }

                Log("Results saved to " + outputFolder);
            }, "Testing proxies…");
        }
        private async void ImportClick(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "Proxy lists (*.txt)|*.txt|All files (*.*)|*.*", Title = "Import proxy endpoints" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                string file = dialog.FileName;
                await Run(token => Task.Run(() =>
                {
                    if (new FileInfo(file).Length > 32 * 1024 * 1024) throw new IOException("Import limit is 32 MB.");
                    int added = 0, duplicate = 0, rejected = 0;
                    using (var reader = new StreamReader(file))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            token.ThrowIfCancellationRequested();
                            ProxyEndpoint p;
                            if (!ProxyEndpoint.TryParse(line, out p)) { rejected++; continue; }
                            if (candidates.Count >= ProxyScraperManager.MaxCandidates) { messages.Enqueue("Candidate limit reached (500,000)."); break; }
                            if (candidates.TryAdd(p.Key, p)) added++; else duplicate++;
                        }
                    }
                    messages.Enqueue("Import: " + added + " added, " + duplicate + " duplicates, " + rejected + " invalid / non-public endpoints.");
                }, token), "Importing proxies…");
            }
        }
        private void RefreshState()
        {
            ProxyTypeDetector.ProxyTestResult result; int batch = 0;
            while (batch++ < 5000 && incoming.TryDequeue(out result))
            {
                results.Add(result);
                if (result.HttpsVerified || result.HttpWorking)
                {
                    if (result.Type == ProxyTypeDetector.ProxyType.Http) httpCount++;
                    else if (result.Type == ProxyTypeDetector.ProxyType.Socks4) socks4Count++;
                    else if (result.Type == ProxyTypeDetector.ProxyType.Socks5) socks5Count++;
                }
                if (Matches(result)) visible.Add(result);
            }
            grid.RowCount = visible.Count;
            string message;
            while (messages.TryDequeue(out message)) Log(message);
            collectedLabel.Text = candidates.Count.ToString("N0");
            checkedLabel.Text = Volatile.Read(ref checkedCount).ToString("N0") + " / " + totalCount.ToString("N0");
            goodLabel.Text = Volatile.Read(ref goodCount).ToString("N0");
            httpOnlyLabel.Text = Volatile.Read(ref httpOnlyCount).ToString("N0");
            badLabel.Text = Volatile.Read(ref failedCount).ToString("N0");
            httpCountLabel.Text = "HTTP   " + httpCount.ToString("N0") + " working";
            socks4CountLabel.Text = "SOCKS4   " + socks4Count.ToString("N0") + " working";
            socks5CountLabel.Text = "SOCKS5   " + socks5Count.ToString("N0") + " working";
            if (checking && operation != null && !operation.IsCancellationRequested && totalCount > 0 && watch.Elapsed.TotalSeconds > 0)
                statusLabel.Text = "Checking · " + (Volatile.Read(ref checkedCount) / watch.Elapsed.TotalSeconds).ToString("N1") + " proxies/sec · " + visible.Count.ToString("N0") + " shown · TCP " + (scheduler?.Connecting ?? 0) + " / probes " + (scheduler?.Probing ?? 0);
            elapsedLabel.Text = watch.Elapsed.ToString(@"hh\:mm\:ss");
            progress.Value = totalCount == 0 ? 0 : Math.Min(1000, (int)(1000L * Volatile.Read(ref checkedCount) / totalCount));
        }
        private bool Matches(ProxyTypeDetector.ProxyTestResult r)
        {
            string type = filterBox.SelectedItem as string;
            if (type == "Working Proxy" && !r.HttpsVerified && !r.HttpWorking) return false;
            if (type == "HTTPS verified" && !r.HttpsVerified) return false;
            if (type == "HTTP only" && (r.HttpsVerified || !r.HttpWorking)) return false;
            if (type == "Failed" && r.Type != ProxyTypeDetector.ProxyType.Unknown) return false;
            if ((type == "Http" || type == "Socks4" || type == "Socks5") && r.Type.ToString() != type) return false;
            return r.Endpoint.IndexOf(searchBox.Text.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }
        private void ApplyFilter(object sender, EventArgs e)
        {
            visible.Clear(); visible.AddRange(results.Where(Matches)); grid.RowCount = visible.Count; grid.Invalidate();
        }
        private void GridValue(object sender, DataGridViewCellValueEventArgs e)
        {
            if (e.RowIndex >= visible.Count) return;
            var r = visible[e.RowIndex];
            switch (e.ColumnIndex)
            {
                case 0:
                    e.Value = SmallFlag(r.CountryCode); break;
                case 1: e.Value = r.Endpoint; break;
                case 2: e.Value = (string.IsNullOrEmpty(r.CountryCode) ? "--" : r.CountryCode.ToUpperInvariant()) + "  ·  " + r.Country; break;
                case 3: e.Value = r.Type == ProxyTypeDetector.ProxyType.Unknown ? "—" : r.Type.ToString().ToUpperInvariant(); break;
                case 4: e.Value = r.HttpsVerified ? "HTTPS / TLS 1.2" : r.HttpWorking ? "HTTP ONLY / TLS FAILED" : "FAILED"; break;
                case 5: e.Value = r.TcpPingMs < 0 ? "—" : r.TcpPingMs == 0 ? "<1 ms" : r.TcpPingMs + " ms"; break;
                case 6: e.Value = r.HttpsLatencyMs < 0 ? "—" : r.HttpsLatencyMs + " ms"; break;
                case 7: e.Value = r.Detail; break;
            }
        }
        private void StopClick(object sender, EventArgs e)
        {
            if (operation == null) return;
            stopButton.Enabled = false; statusLabel.Text = "Stopping…"; operation.Cancel();
        }
        private void HandleClosing(object sender, FormClosingEventArgs e)
        {
            if (operation == null) return;
            e.Cancel = true; closeAfterStop = true; StopClick(sender, EventArgs.Empty);
        }
        private void SetBusy(bool busy)
        {
            scrapeButton.Enabled = checkButton.Enabled = importButton.Enabled = clearButton.Enabled = saveSourcesButton.Enabled = restoreSourcesButton.Enabled = !busy;
            workers.Enabled = timeoutBox.Enabled = autoCountryBox.Enabled = retryAlternateBox.Enabled = !busy;
            targetBox.ReadOnly = httpsTargetBox.ReadOnly = sourcesBox.ReadOnly = busy;
            workers.BackColor = timeoutBox.BackColor = autoCountryBox.BackColor = retryAlternateBox.BackColor = busy ? System.Drawing.Color.FromArgb(190, 201, 215) : System.Drawing.Color.FromArgb(20, 29, 44);
            autoCountryBox.ForeColor = busy ? System.Drawing.Color.Black : System.Drawing.Color.FromArgb(236, 244, 255);
            stopButton.Enabled = busy;
        }
        private void ClearClick(object sender, EventArgs e)
        {
            countryGeneration++; countryRetries.Clear();
            candidates.Clear(); results.Clear(); visible.Clear(); httpCount = socks4Count = socks5Count = 0;
            ProxyTypeDetector.ProxyTestResult r; while (incoming.TryDequeue(out r)) { }
            while (countryQueue.TryDequeue(out r)) { }
            checkedCount = goodCount = httpOnlyCount = failedCount = totalCount = 0; watch.Reset(); RefreshState(); statusLabel.Text = "Ready";
        }
        private void SaveSources(object sender, EventArgs e)
        {
            try { SourceCatalog.Save(UserFolder, sourcesBox.Text); Log("Source preferences saved."); }
            catch (Exception ex) { Notice(ex.Message); }
        }
        private void ExportClick(object sender, EventArgs e) { ExportCategory(true); }
        private void RestoreSources(object sender, EventArgs e)
        { sourcesBox.Text = SourceCatalog.Defaults; Log("Embedded source catalog restored. Save sources to keep this selection."); }
        private void ExportHttpClick(object sender, EventArgs e) { ExportCategory(false); }
        private void ExportCategory(bool secure)
        {
            using (var dialog = new ExportForm(results.Where(r => secure ? r.HttpsVerified : r.HttpWorking && !r.HttpsVerified).ToArray(), secure))
                dialog.ShowDialog(this);
        }
        private void OpenOutput(object sender, EventArgs e)
        {
            try { Directory.CreateDirectory(outputFolder ?? UserFolder); Process.Start("explorer.exe", "\"" + (outputFolder ?? UserFolder) + "\""); }
            catch (Exception ex) { Notice(ex.Message); }
        }
        private void LookupCountry(object sender, EventArgs e)
        {
            if (grid.CurrentCell == null || grid.CurrentCell.RowIndex >= visible.Count) return;
            var result = visible[grid.CurrentCell.RowIndex]; countryRetries.Remove(result); result.Country = "Queued"; countryQueue.Enqueue(result);
        }
        private async Task ProcessCountriesAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    ProxyTypeDetector.ProxyTestResult result;
                    if (!countryQueue.TryDequeue(out result)) { await Task.Delay(200, token); continue; }
                    Tuple<int, DateTime> retry;
                    if (!countryRetries.TryGetValue(result, out retry)) retry = Tuple.Create(0, DateTime.MinValue);
                    if (retry.Item2 > DateTime.UtcNow) { countryQueue.Enqueue(result); await Task.Delay(200, token); continue; }
                    int generation = countryGeneration;
                    result.Country = "Resolving…";
                    var country = await countryResolver.ResolveAsync(result, token);
                    if (generation != countryGeneration) continue;
                    if (country != null)
                    {
                        result.Country = country.Name; result.CountryCode = country.Code;
                        result.CountryDetail = "Endpoint IP country · " + country.Source + " · " + country.CheckedUtc.ToString("u");
                        countryRetries.Remove(result);
                    }
                    else
                    {
                        int attempts = retry.Item1 + 1;
                        result.Country = attempts < 3 ? "Retry " + attempts + "/3" : "Unknown (retry available)";
                        result.CountryDetail = "No validated country from three providers using direct and proxy routes. See Activity; right-click to retry.";
                        if (attempts < 3)
                        {
                            countryRetries[result] = Tuple.Create(attempts, DateTime.UtcNow.AddSeconds(attempts * 15));
                            countryQueue.Enqueue(result);
                        }
                        else countryRetries.Remove(result);
                    }
                    if (result.DetailWriter != null)
                    {
                        try { await Task.Run(() => result.DetailWriter.UpdateDetails(result)); }
                        catch (Exception ex) { messages.Enqueue("Full Detail country update failed: " + ex.Message); }
                    }
                    if (!token.IsCancellationRequested && !IsDisposed) grid.Invalidate();
                }
            }
            catch (OperationCanceledException) { }
        }
        private void CountryToolTip(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        { if (e.RowIndex >= 0 && e.RowIndex < visible.Count && e.ColumnIndex <= 2) e.ToolTipText = visible[e.RowIndex].CountryDetail; }
        private void Log(string message)
        {
            if (activityBox.TextLength > 100000) activityBox.Clear();
            activityBox.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + message + Environment.NewLine);
        }
        private void Notice(string text) { MessageBox.Show(this, text, "ScrapeX", MessageBoxButtons.OK, MessageBoxIcon.Information); }
    }
}
