using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Reflection;
using ScrapeX;

class RegressionTests
{
    static int count;
    static void Assert(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); Console.WriteLine("PASS: " + name); count++; }
    static ProxyEndpoint Local(int port)
    {
        ProxyEndpoint p; ProxyEndpoint.TryParse("8.8.8.8:" + port, out p);
        typeof(ProxyEndpoint).GetProperty("Host").SetValue(p, "127.0.0.1", null); return p;
    }
    static async Task<byte[]> Read(Stream s, int n)
    {
        var b = new byte[n]; int o=0;
        while(o<n) { int x=await s.ReadAsync(b,o,n-o); if(x==0)throw new IOException();o+=x; } return b;
    }
    static async Task Header(Stream s)
    {
        var text="";while(!text.EndsWith("\r\n\r\n")) { text+=(char)(await Read(s,1))[0];if(text.Length>20000)throw new Exception(); }
    }
    static async Task ProxyTest(int protocol, bool domain = false)
    {
        var listener = new TcpListener(IPAddress.Loopback,0);listener.Start();int port=((IPEndPoint)listener.LocalEndpoint).Port;
        var server=Task.Run(async()=>
        {
            for(int attempt=0;attempt<=protocol;attempt++)
            using(var c=await listener.AcceptTcpClientAsync())
            using(var s=c.GetStream())
            {
                if(attempt<protocol) continue;
                if(protocol==1)
                {
                    var hello=await Read(s,3);if(hello[0]!=5)throw new Exception("SOCKS5 hello");
                    await s.WriteAsync(new byte[]{5,0},0,2);
                    var h=await Read(s,4);int addressSize=h[3]==1?4:h[3]==4?16:(await Read(s,1))[0];await Read(s,addressSize+2);
                    Assert(h[3]==(domain ? 3 : 1),domain ? "SOCKS5 delegates hostname DNS to proxy" : "SOCKS5 uses IPv4 address type for numeric targets");
                    var reply=new byte[]{5,0,0,1,127,0,0,1,0,80};
                    // Fragmented responses exercise exact-length reads.
                    foreach(byte b in reply)await s.WriteAsync(new[]{b},0,1);
                }
                if(protocol==2) { var h=await Read(s,9); if(h[0]!=4)throw new Exception("SOCKS4 hello");await s.WriteAsync(new byte[]{0,90,0,80,127,0,0,1},0,8); }
                await Header(s);await Task.Delay(70);var ok=Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\nHTTP/1.1 204 No Content\r\nConnection: close\r\n\r\n");await s.WriteAsync(ok,0,ok.Length);
            }
        });
        try
        {
            var result=await new ProxyTypeDetector().DetectAsync(Local(port),new Uri(domain ? "http://example.org/generate_204" : "http://1.1.1.1/generate_204"),1000,CancellationToken.None);
            Assert(result.Type==(ProxyTypeDetector.ProxyType)protocol,"Protocol detection " + protocol);Assert(result.SpeedMs>=0,"Latency " + protocol);Assert(result.TcpPingMs >= 0 && result.SpeedMs - result.TcpPingMs >= 50, "TCP ping excludes response wait; interim 100 accepted " + protocol);await server;
        }
        finally { listener.Stop(); }
    }
    static async Task TimeoutTest(bool cancel)
    {
        var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();var clients=new System.Collections.Generic.List<TcpClient>();
        var accept=Task.Run(async()=> { try { while(true) { var c=await listener.AcceptTcpClientAsync();lock(clients)clients.Add(c); } } catch(ObjectDisposedException){} catch(SocketException){} });
        var timer=System.Diagnostics.Stopwatch.StartNew();
        using(var ct=new CancellationTokenSource())
        {
            if(cancel)ct.CancelAfter(100);
            try
            {
                var r=await new ProxyTypeDetector().DetectAsync(Local(((IPEndPoint)listener.LocalEndpoint).Port),new Uri("http://1.1.1.1/"),200,ct.Token);
                Assert(!cancel&&r.Type==ProxyTypeDetector.ProxyType.Unknown,"All protocol timeouts return failure");
            }
            catch(OperationCanceledException) { Assert(cancel,"Cancellation propagated"); }
            finally {listener.Stop();await accept;lock(clients)foreach(var c in clients)c.Dispose();}
        }
        Assert(timer.ElapsedMilliseconds<2500,cancel?"Prompt cancellation":"Bounded timeout");
    }
    static async Task InvalidResponseTest()
    {
        var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();
        var server=Task.Run(async()=> {for(int i=0;i<3;i++)using(var c=await listener.AcceptTcpClientAsync()){var bytes=Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: 0\r\n\r\n");await c.GetStream().WriteAsync(bytes,0,bytes.Length);}});
        try {var r=await new ProxyTypeDetector().DetectAsync(Local(((IPEndPoint)listener.LocalEndpoint).Port),new Uri("http://1.1.1.1/generate_204"),500,CancellationToken.None);Assert(r.Type==ProxyTypeDetector.ProxyType.Unknown,"Captive 200 rejected for 204 target");await server;}finally{listener.Stop();}
    }
    static async Task SourceTest()
    {
        var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();int port=((IPEndPoint)listener.LocalEndpoint).Port;
        var server=Task.Run(async()=> {using(var c=await listener.AcceptTcpClientAsync()){await Header(c.GetStream());string body="8.8.8.8:80\nsocks5://8.8.8.8:80\n1.1.1.1:8080\n127.0.0.1:80\n<html>bad</html>\n";var bytes=Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Length: "+body.Length+"\r\nConnection: close\r\n\r\n"+body);await c.GetStream().WriteAsync(bytes,0,bytes.Length);}});
        try
        {
            var dict=new ConcurrentDictionary<string,ProxyEndpoint>();var scraper=new ProxyScraperManager(dict);SourceResult report=null;scraper.OnSourceCompleted+=r=>report=r;
            await scraper.StartScrapingAsync(new[]{"http://127.0.0.1:"+port+"/list"},CancellationToken.None);await server;
            Assert(dict.Count==2,"Source parsing and cross-protocol deduplication");Assert(report.Valid==2&&report.Added==2&&report.Status=="OK","Source report counts");
        }
        finally{listener.Stop();}
    }
    sealed class FakeHandler : System.Net.Http.HttpMessageHandler
    {
        public string Body; public int Calls;
        protected override Task<System.Net.Http.HttpResponseMessage> SendAsync(System.Net.Http.HttpRequestMessage request, CancellationToken token)
        {
            Calls++;
            return Task.FromResult(new System.Net.Http.HttpResponseMessage(HttpStatusCode.OK) { Content = new System.Net.Http.StringContent(Body) });
        }
    }
    static async Task ApiTests()
    {
        foreach(var mode in new[]{0,1,2,3})
        {
            var dict=new ConcurrentDictionary<string,ProxyEndpoint>(); var manager=new ProxyScraperManager(dict); SourceResult report=null;manager.OnSourceCompleted+=r=>report=r;
            var handler=new FakeHandler {Body=mode==0?"{\"data\":[{\"ip\":\"8.8.8.8\",\"port\":80}]}":mode==1?"{\"wrong\":[]}":mode==2?"<html>Error</html>":new string('x',17*1024*1024)};
            using(var client=new System.Net.Http.HttpClient(handler))
            {
                var task=(Task)typeof(ProxyScraperManager).GetMethod("Fetch",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(manager,new object[]{client,"https://proxylist.geonode.com/api/proxy-list?page=1",CancellationToken.None});
                await task;
                if(mode==0)Assert(handler.Calls==2 && dict.Count==1,"Repeated API pages terminate and deduplicate");
                else Assert(report.Status.StartsWith("Failed:"), mode==1?"API schema change reported":mode==2?"Invalid JSON isolated":"Oversized source rejected");
            }
        }
    }
    static void Pump(Task task)
    {
        var timer=System.Diagnostics.Stopwatch.StartNew();
        while(!task.IsCompleted && timer.ElapsedMilliseconds<4000) {System.Windows.Forms.Application.DoEvents();Thread.Sleep(5);}
        Assert(task.IsCompleted,"UI operation completed without deadlock");task.GetAwaiter().GetResult();
    }
    static void UiTests(Form1 form)
    {
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var run=typeof(Form1).GetMethod("Run",flags);int started=0;
        Func<CancellationToken,Task> work=async token=> {started++;await Task.Delay(10000,token);};
        var first=(Task)run.Invoke(form,new object[]{work,"Test operation"});
        var second=(Task)run.Invoke(form,new object[]{work,"Duplicate operation"});
        Assert(started==1 && second.IsCompleted,"Repeated starts do not create extra workers");
        typeof(Form1).GetMethod("StopClick",flags).Invoke(form,new object[]{null,EventArgs.Empty});Pump(first);
        Assert(typeof(Form1).GetField("operation",flags).GetValue(form)==null,"Operation and cancellation state released");
        var next=(Task)run.Invoke(form,new object[]{new Func<CancellationToken,Task>(token=>Task.CompletedTask),"Restart"});Pump(next);
        Assert(!form.IsDisposed,"Restart after cancellation");
        var closing=(Task)run.Invoke(form,new object[]{work,"Close during operation"});form.Close();Pump(closing);
        Assert(form.IsDisposed,"Close waits for cancellation, then disposes UI");
    }
    static async Task HttpOnlySeparationTest()
    {
        var listener=new TcpListener(IPAddress.Loopback,0); listener.Start();
        int port=((IPEndPoint)listener.LocalEndpoint).Port;
        var serving = new CancellationTokenSource();
        var clients = new System.Collections.Concurrent.ConcurrentBag<Task>();
        var server=Task.Run(async()=>
        {
            try { while(!serving.IsCancellationRequested)
            {
                var c=await listener.AcceptTcpClientAsync();
                clients.Add(Task.Run(async()=> { using(c) try
                {
                    var stream=c.GetStream(); var first=await Read(stream,1);
                    if(first[0] != 'C' && first[0] != 'G') return;
                    await Header(stream);
                    var bytes=Encoding.ASCII.GetBytes(first[0]=='C'?"HTTP/1.1 403 Forbidden\r\nContent-Length: 0\r\n\r\n":"HTTP/1.1 204 No Content\r\n\r\n");
                    await stream.WriteAsync(bytes,0,bytes.Length);
                } catch(IOException) {} }));
            } } catch(ObjectDisposedException) {} catch(SocketException) {}
        });
        try
        {
            var r=await new ProxyTypeDetector().CheckAsync(Local(port),new Uri("http://1.1.1.1/generate_204"),new Uri("https://1.1.1.1/generate_204"),500,CancellationToken.None);
            serving.Cancel();listener.Stop();await server;await Task.WhenAll(clients.ToArray());
            Assert(r.HttpWorking&&!r.HttpsVerified,"HTTP success cannot imply HTTPS success");
            Assert(r.HttpType==ProxyTypeDetector.ProxyType.Http&&r.HttpsType==ProxyTypeDetector.ProxyType.Unknown,"Separate protocol verdicts");
            var folder=Path.Combine(Path.GetTempPath(),"ScrapeX-regression-"+Guid.NewGuid().ToString("N"));
            using(var writer=new ResultWriter(folder))
            {
                writer.Write(r);
                writer.Write(new ProxyTypeDetector.ProxyTestResult{Endpoint="8.8.8.8:8080",HttpsVerified=true,HttpsType=ProxyTypeDetector.ProxyType.Socks5,Type=ProxyTypeDetector.ProxyType.Socks5});
                writer.Write(new ProxyTypeDetector.ProxyTestResult{Endpoint="9.9.9.9:80",Type=ProxyTypeDetector.ProxyType.Unknown});
            }
            Assert(File.ReadAllLines(Path.Combine(folder,"HTTP_ONLY","HTTP.txt")).Single()==r.Endpoint,"HTTP-only output saved separately");
            Assert(File.ReadAllLines(Path.Combine(folder,"HTTPS","SOCKS5.txt")).Single()=="8.8.8.8:8080","Verified HTTPS output excludes HTTP-only results");
            Assert(Directory.GetFiles(folder,"*.txt",SearchOption.AllDirectories).All(file=>!File.ReadAllText(file).Contains("9.9.9.9:80")),"Failed endpoints never enter working output");
        }
        finally{serving.Cancel();listener.Stop();serving.Dispose();}
    }
    static async Task InvalidCertificateTest()
    {
        using(var rsa=System.Security.Cryptography.RSA.Create(2048))
        {
            var request=new System.Security.Cryptography.X509Certificates.CertificateRequest("CN=localhost",rsa,System.Security.Cryptography.HashAlgorithmName.SHA256,System.Security.Cryptography.RSASignaturePadding.Pkcs1);
            using(var generated=request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1),DateTimeOffset.UtcNow.AddDays(1)))
            using(var cert=new System.Security.Cryptography.X509Certificates.X509Certificate2(generated.Export(System.Security.Cryptography.X509Certificates.X509ContentType.Pfx), (string)null, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.UserKeySet))
            {
                var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();
                var server=Task.Run(async()=>
                {
                    for(int i=0;i<3;i++)using(var c=await listener.AcceptTcpClientAsync())
                    {
                        if(i>0)continue;
                        await Header(c.GetStream());
                        var bytes=Encoding.ASCII.GetBytes("HTTP/1.1 200 Connection Established\r\n\r\n");await c.GetStream().WriteAsync(bytes,0,bytes.Length);
                        using(var tls=new System.Net.Security.SslStream(c.GetStream(),false))
                        {try{await tls.AuthenticateAsServerAsync(cert,false,System.Security.Authentication.SslProtocols.Tls12,false);await Header(tls);}catch(System.Security.Authentication.AuthenticationException){}catch(IOException){}}
                    }
                });
                try
                {
                    var r=await new ProxyTypeDetector().DetectAsync(Local(((IPEndPoint)listener.LocalEndpoint).Port),new Uri("https://localhost/generate_204"),1500,CancellationToken.None);
                    Assert(r.Type==ProxyTypeDetector.ProxyType.Unknown,"Untrusted TLS certificate is rejected");await server;
                }
                finally{listener.Stop();}
            }
        }
    }
    static object Field(object target, string name) { return target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target); }
    static void NewUiTests(Form1 form, string imagePath)
    {
        var queue = (ConcurrentQueue<ProxyTypeDetector.ProxyTestResult>)Field(form, "incoming");
        var samples = new[] {
            new ProxyTypeDetector.ProxyTestResult { Endpoint="192.0.2.1:8080", Type=ProxyTypeDetector.ProxyType.Http, HttpsType=ProxyTypeDetector.ProxyType.Http, HttpsVerified=true, Country="Germany", CountryCode="DE", TcpPingMs=42, HttpsLatencyMs=120, Detail="Fixture: verified HTTPS" },
            new ProxyTypeDetector.ProxyTestResult { Endpoint="192.0.2.2:1080", Type=ProxyTypeDetector.ProxyType.Socks5, HttpsType=ProxyTypeDetector.ProxyType.Socks5, HttpsVerified=true, Country="United States", CountryCode="US", TcpPingMs=76, HttpsLatencyMs=190, Detail="Fixture: TLS verified, destination HTTP 403" },
            new ProxyTypeDetector.ProxyTestResult { Endpoint="192.0.2.3:1080", Type=ProxyTypeDetector.ProxyType.Socks4, HttpType=ProxyTypeDetector.ProxyType.Socks4, HttpWorking=true, Country="Unknown", TcpPingMs=35, Detail="Fixture: HTTP only" },
            new ProxyTypeDetector.ProxyTestResult { Endpoint="192.0.2.4:80", Type=ProxyTypeDetector.ProxyType.Unknown, Detail="Fixture: failed" }
        };
        foreach(var r in samples) queue.Enqueue(r);
        typeof(Form1).GetMethod("RefreshState",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,null);
        var grid=(System.Windows.Forms.DataGridView)Field(form,"grid");
        Assert(grid.RowCount==3,"Working Proxy default includes HTTPS and HTTP-only, excludes failed");
        Assert(((string)grid.Rows[0].Cells[2].Value).Contains("DE") && ((string)grid.Rows[1].Cells[2].Value).Contains("US"),"Country codes displayed with country names");
        Assert(((Image)grid.Rows[0].Cells[0].Value).Size==new Size(20,13),"Flags have compact fixed dimensions");
        Assert((string)grid.Rows[0].Cells[5].Value=="42 ms","HTTPS-only result displays measured TCP ping");
        Assert(((System.Windows.Forms.Label)Field(form,"httpCountLabel")).Text.Contains("1 working") && ((System.Windows.Forms.Label)Field(form,"socks4CountLabel")).Text.Contains("1 working") && ((System.Windows.Forms.Label)Field(form,"socks5CountLabel")).Text.Contains("1 working"),"Independent protocol counters");
        using(var b=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(b,new Rectangle(Point.Empty,form.Size));b.Save(imagePath.Replace(".png","-results.png"));}
        typeof(Form1).GetMethod("SetBusy",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,new object[]{true});
        using(var b=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(b,new Rectangle(Point.Empty,form.Size));b.Save(imagePath.Replace(".png","-busy.png"));}
        typeof(Form1).GetMethod("SetBusy",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,new object[]{false});
        using(var export=new ExportForm(samples.Take(2).ToArray(),true))
        {
            export.ShowInTaskbar=false;export.StartPosition=System.Windows.Forms.FormStartPosition.Manual;export.Location=new Point(-10000,-10000);export.Show();System.Windows.Forms.Application.DoEvents();
            var countries=(System.Windows.Forms.CheckedListBox)Field(export,"countries");
            Assert(countries.Items.Count==2 && countries.CheckedItems.Count==2,"Export groups and selects countries");
            countries.SetItemChecked(1,false);System.Windows.Forms.Application.DoEvents();
            Assert(((System.Windows.Forms.Label)Field(export,"summary")).Text.StartsWith("1 proxies"),"Country selection updates export count");
            ((System.Windows.Forms.ComboBox)Field(export,"protocol")).SelectedIndex=3;
            Assert(!((System.Windows.Forms.Button)Field(export,"saveButton")).Enabled,"Empty country/protocol intersection cannot export");
            ((System.Windows.Forms.ComboBox)Field(export,"protocol")).SelectedIndex=0;
            countries.SetItemChecked(1,true);System.Windows.Forms.Application.DoEvents();
            var lines=(string[])typeof(ExportForm).GetMethod("Lines",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(export,new object[]{samples.Take(2)});
            Assert(lines.Length==2 && lines.Any(l=>l.StartsWith("socks5://")),"Export preserves verified protocol prefixes");
            using(var b=new Bitmap(export.Width,export.Height)){export.DrawToBitmap(b,new Rectangle(Point.Empty,export.Size));b.Save(imagePath.Replace(".png","-export.png"));}
            export.Close();
        }
        Assert(ExportForm.CountryKey("../bad")=="UNKNOWN" && ExportForm.CountryKey("de")=="DE","Country filenames reject invalid codes");
    }
    [STAThread] static int Main(string[] args)
    {
        try
        {
            ProxyEndpoint p;
            foreach(var s in new[]{"127.0.0.1:80","10.0.0.1:80","999.2.3.4:80","8.8.8.8:0","8.8.8.8:65536","8.8.8.8:80/path","user:pass@8.8.8.8:80","<html>8.8.8.8:80</html>","169.254.169.254:80","1.2:80"})Assert(!ProxyEndpoint.TryParse(s,out p),"Reject "+s);
            Assert(ProxyEndpoint.TryParse(" socks5://8.8.8.8:0080 ",out p)&&p.Key=="8.8.8.8:80","Canonical endpoint");
            Task.Run(async()=> {await ProxyTest(0);await ProxyTest(1);await ProxyTest(2);await ProxyTest(1,true);await TimeoutTest(false);await TimeoutTest(true);await InvalidResponseTest();await SourceTest();await ApiTests();await HttpOnlySeparationTest();await InvalidCertificateTest();}).GetAwaiter().GetResult();
            System.Windows.Forms.Application.EnableVisualStyles();
            using(var form=new Form1())
            {
                form.ShowInTaskbar=false;form.StartPosition=System.Windows.Forms.FormStartPosition.Manual;form.Location=new Point(-10000,-10000);form.Show();System.Windows.Forms.Application.DoEvents();form.PerformLayout();
                using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));bitmap.Save(args[0]);}
                Assert(form.Controls.Count>0,"UI constructed and rendered");
                form.Size=new Size(1120,780);form.PerformLayout();
                using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,form.Width,form.Height));bitmap.Save(args[0].Replace(".png","-minimum.png"));}
                form.Size=new Size(1440,1000);NewUiTests(form,args[0]);
                UiTests(form);
            }
            Console.WriteLine("TOTAL: "+count+" passed");return 0;
        }
        catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
    }
}


