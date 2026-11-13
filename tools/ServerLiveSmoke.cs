// Optional online integration check. Uses a separate data directory and real public feeds.
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using ScrapeX;
class ServerLiveSmoke
{
    [STAThread] static void Main(string[] args)
    {
        if (args.Length == 0) args = new[] { "ui", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../outputs/server-live") };
        if (args.Length != 2) throw new ArgumentException("Usage: ServerLiveSmoke.exe check|ui DATA_DIRECTORY");
        string folder = Path.GetFullPath(args[1]); Directory.CreateDirectory(folder);
        if (args[0] == "ui")
        {
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(folder)); return;
        }
        var publication = new ProxyPublication(Path.Combine(folder, "published-proxies.json"));
        var sources = new[] {
            "https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/http.txt",
            "https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/socks4.txt",
            "https://raw.githubusercontent.com/monosans/proxy-list/main/proxies/socks5.txt" };
        var pipeline = new ServerRefreshPipeline(publication, folder, sources,
            new Uri("http://www.google.com/generate_204"), new Uri("https://www.google.com/generate_204"), 100, 3500, true);
        pipeline.Diagnostic += Console.WriteLine;
        using (var stop = new CancellationTokenSource(TimeSpan.FromMinutes(10)))
        {
            int working = pipeline.RunAsync(stop.Token).GetAwaiter().GetResult();
            if (pipeline.Checked != pipeline.Collected || working != publication.Current.Proxies.Length)
                throw new Exception("Publication does not match the completed check.");
            Console.WriteLine("PASS live pipeline: " + pipeline.Checked + " checked; " + working + " published. Availability is time/network dependent.");
        }
    }
}
