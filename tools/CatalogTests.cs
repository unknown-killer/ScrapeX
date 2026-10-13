using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using Newtonsoft.Json.Linq;
using ScrapeX;

class CatalogTests
{
    static int count;
    static void Check(bool ok, string name) { if (!ok) throw new Exception(name); Console.WriteLine("PASS: " + name); count++; }
    static int Parse(string value) { int raw; return SourceParser.Parse(value, out raw).Count; }
    [STAThread]
    static void Main(string[] args)
    {
        if (args.Length == 1) { Audit(args[0]); return; }
        string temp = Path.Combine(Path.GetTempPath(), "ScrapeX-catalog-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            Check(SourceCatalog.Urls(SourceCatalog.Defaults).Length >= 36, "embedded catalog available with no sidecar");
            var requested=File.ReadAllLines("reports/catalog-audit/requested-sources.txt").Where(s=>s.StartsWith("http")).ToArray();
            Check(requested.Length==36 && requested.All(u=>SourceCatalog.Urls(SourceCatalog.Defaults).Contains(u)), "every one of the 36 requested URLs is embedded");
            string loaded = SourceCatalog.Load(Path.Combine(temp,"settings"), Path.Combine(temp,"missing-app-folder"), Console.WriteLine);
            Check(SourceCatalog.Urls(loaded).Length >= 36, "clean server installation uses embedded catalog");
            Check(SourceCatalog.Merge("# empty", "", "https://a\n") == "https://a\n", "empty preferences restore defaults");
            Check(SourceCatalog.Urls(SourceCatalog.Merge("https://custom\n", "", "https://a\nhttps://b\n")).Length == 3, "upgrade preserves custom sources and adds new defaults");
            Check(!SourceCatalog.Merge("https://b\n", "https://a\nhttps://b\n", "https://a\nhttps://b\nhttps://c\n").Contains("https://a"), "subsequent upgrades respect deleted defaults");
            Check(!SourceCatalog.Urls(SourceCatalog.Merge("# https://a\nhttps://custom\n", "", "https://a\n")).Contains("https://a"), "commented feeds remain disabled");
            Check(Parse("# header\n8.8.8.8:80 US-A +\nsocks5:1.1.1.1:1080\n127.0.0.1:80\nno endpoint") == 2, "SPYS metadata and protocol colon format; reject private and junk");
            Check(Parse("8.8.8.8 443\n1.1.1.1\t80") == 2, "space/tab-separated IP and port lists");
            int raw;
            Check(SourceParser.Parse("8.8.8.8:80:United States",out raw,"https://raw.githubusercontent.com/zloi-user/hideip.me/main/http.txt").Count==1 && Parse("8.8.8.8:80:password")==0, "country suffix allowed only for its documented feed format");
            var parsed = SourceParser.Parse("{\"data\":[{\"ip\":\"8.8.8.8\",\"port\":1080,\"protocols\":[\"socks5\"]},{\"ip\":\"127.0.0.1\",\"port\":80}]}", out raw);
            Check(raw == 2 && parsed.Count == 1 && parsed[0].PreferredType == ProxyTypeDetector.ProxyType.Socks5, "JSON pagination count and protocol hint preserved");
            Check(Parse("[{\"addr\":\"1.1.1.1\",\"port\":80},{\"ip\":\"8.8.8.8\",\"port\":80,\"username\":\"private\"}]") == 1, "JSON rejects authenticated routes");
            bool schema = false; try { Parse("{\"error\":\"rate limited\"}"); } catch (IOException) { schema = true; }
            Check(schema, "API errors are visible instead of successful empty lists");
            Check(Parse("<!doctype html><script>8.8.8.8:80</script><p>1.1.1.1:80</p><table><tr><td><a>9.9.9.9</a></td><td>3128</td></tr></table><textarea>8.8.4.4:8080&#10;bad</textarea>") == 2, "HTML uses table cells and list blocks, ignores script/incidental IPs");
            var secure = new ProxyTypeDetector.ProxyTestResult { Endpoint="8.8.8.8:80",Type=ProxyTypeDetector.ProxyType.Http,HttpsType=ProxyTypeDetector.ProxyType.Http,HttpsVerified=true,TcpPingMs=12,HttpsLatencyMs=120,Country="Queued",CheckedAt=DateTime.UtcNow,TlsProtocol="Tls12",Detail="test\nresponse\twith|separator" };
            var plain = new ProxyTypeDetector.ProxyTestResult { Endpoint="1.1.1.1:1080",Type=ProxyTypeDetector.ProxyType.Socks5,HttpType=ProxyTypeDetector.ProxyType.Socks5,HttpWorking=true,Country="Unknown" };
            var failed = new ProxyTypeDetector.ProxyTestResult { Endpoint="9.9.9.9:80",Type=ProxyTypeDetector.ProxyType.Unknown };
            string folder=Path.Combine(temp,"session");
            var writer=new ResultWriter(folder);
            writer.Write(secure); writer.Write(plain); writer.Write(failed); writer.FlushDetails();
            string path=Path.Combine(folder,"Full Detail.txt");
            string text=File.ReadAllText(path);
            Check(text.Contains("HTTPS VERIFIED") && text.Contains("HTTP ONLY / TLS FAILED") && !text.Contains(failed.Endpoint), "Full Detail contains working routes with explicit security classification");
            Check(text.Contains("12 ms") && text.Contains("120 ms") && text.Contains("test response with/separator"), "measured latency and safe single-line details");
            writer.Dispose(); secure.CountryCode="DE";secure.Country="Germany";writer.UpdateDetails(secure);
            text=File.ReadAllText(path);
            Check(text.Contains("Germany") && text.Contains("DE") && !text.Contains("Queued"), "late country result updates file after check writer disposal");
            Check(File.ReadAllLines(path).Count(l=>l.StartsWith("8.8.8.8:"))==1, "country updates replace row without duplicates");
            Check(File.ReadAllText(Path.Combine(folder,"HTTPS","HTTP.txt")).Trim()==secure.Endpoint && File.ReadAllText(Path.Combine(folder,"HTTP_ONLY","SOCKS5.txt")).Trim()==plain.Endpoint, "plain endpoint files remain separated and importable");
            Parallel.For(0,30,i=>writer.UpdateDetails(i%2==0?secure:plain));
            Check(File.ReadAllLines(path).Count(l=>l.StartsWith("8.8.8.8:"))==1 && !File.Exists(path+".tmp"), "concurrent detail updates use complete atomic snapshots");
            Application.EnableVisualStyles();
            using(var form=new ExportForm(new[]{secure},true))
            {
                var checkbox=(CheckBox)typeof(ExportForm).GetField("fullDetail",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(form);
                checkbox.Checked=true;
                var lines=(string[])typeof(ExportForm).GetMethod("Lines",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(form,new object[]{new[]{secure}});
                Check(lines.Any(l=>l.Contains("Germany")), "country export supports Full Detail format");
                form.Show(); Application.DoEvents();
                using(var bitmap=new Bitmap(form.Width,form.Height)) { form.DrawToBitmap(bitmap,form.ClientRectangle);bitmap.Save("reports/export-full-detail.png"); }
                form.Close();
            }
            Console.WriteLine("Passed " + count + " catalog/export assertions.");
        }
        finally { Directory.Delete(temp,true); }
    }
    static void Audit(string path)
    {
        var rows=JArray.Parse(File.ReadAllText(path));
        foreach(var row in rows)
        {
            if((string)row["status"]!="downloaded") continue;
            try
            {
                int raw;var endpoints=SourceParser.Parse(File.ReadAllText(Path.Combine(Path.GetDirectoryName(path),(string)row["sample"])),out raw,(string)row["url"]);
                row["valid_endpoints"]=endpoints.Select(p=>p.Key).Distinct().Count();row["raw_records"]=raw;
                row["parser_status"]=endpoints.Count>0?"usable":"empty / unsupported";
            }
            catch(Exception ex) { row["parser_status"]="error";row["parser_error"]=ex.Message; }
            Console.WriteLine(row["parser_status"]+" "+row["valid_endpoints"]+" "+row["url"]);
        }
        File.WriteAllText(path,rows.ToString());
    }
}
