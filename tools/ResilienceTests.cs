using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ScrapeX;
class ResilienceTests
{
    sealed class RedirectHandler : HttpMessageHandler
    {
        public bool Loop; public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;
            if(Loop||Calls==1) { var response=new HttpResponseMessage((HttpStatusCode)308);response.Headers.Location=new Uri("/actual",UriKind.Relative);return Task.FromResult(response); }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new StringContent("8.8.8.8:80")});
        }
    }
    static int count;
    static void Check(bool ok,string text) { if(!ok) throw new Exception(text); Console.WriteLine("PASS: "+text); count++; }
    static ProxyEndpoint Endpoint(int i) { ProxyEndpoint p; ProxyEndpoint.TryParse("8.8."+(i/250)+"."+(i%250+1)+":80",out p); return p; }
    static ProxyTypeDetector.ProxyTestResult Good(ProxyEndpoint p) { return new ProxyTypeDetector.ProxyTestResult { Endpoint=p.Key,Type=ProxyTypeDetector.ProxyType.Http,HttpWorking=true }; }
    static readonly Uri Http=new Uri("http://1.1.1.1/"), Https=new Uri("https://1.1.1.1/");
    static async Task Run()
    {
        int raw;
        var decoded=SourceParser.Parse("<table><tr><td data-ip='OC44LjguOA=='></td><td data-port='ODA='></td></tr></table>",out raw,"https://advanced.name/freeproxy");
        Check(decoded.Count==1&&decoded[0].Key=="8.8.8.8:80","Encoded IP and port decode as data without executing scripts");
        var hidden=SourceParser.Parse("<table><tr><td>8.8.8.8</td><td><div style='display:none'>12</div><a>80</a></td></tr></table>",out raw);
        Check(hidden.Count==1&&hidden[0].Port==80,"Hidden decoy digits never corrupt table port");
        var spaced=SourceParser.Parse("<table><tr><td><span>8.8.8.8</span> <span>:</span> <span>8080</span></td></tr></table>",out raw);
        Check(spaced.Count==1&&spaced[0].Port==8080,"Split IP/colon/port elements retain the endpoint");
        var script="<script>Proxy('OC44LjguODo4MA==')</script><script>Proxy('invalid!'); arbitraryCode()</script>";
        Check(SourceParser.Parse(script,out raw,"https://proxy-list.org/english/index.php").Single().Key=="8.8.8.8:80"&&SourceParser.Parse(script,out raw,"https://example.org").Count==0,"Only the known feed's Base64 literal is decoded; other script data ignored");
        Check(SourceParser.Parse("<div class='list'>8.8.8.8:80#US<br>1.1.1.1:1080#AU</div>",out raw,"https://www.my-proxy.com/free-proxy-list.html").Count==2,"Country suffix and HTML line breaks in My-Proxy list");
        Check(SourceParser.Parse("<div class='fpb-ip'>8.8.8.8<span>:80</span></div>",out raw,"https://fineproxy.org/es/free-proxy/").Single().Key=="8.8.8.8:80","FineProxy nested address block is extracted without metadata");
        Check(ProxyScraperManager.NextWebsitePage(new Uri("https://proxyhub.me/ru/all-free-proxy-list.html"),"<a href='https://evil.example/?page=2'>wrong</a><a href='?page=2&amp;type=http'>next</a>",2)=="https://proxyhub.me/ru/all-free-proxy-list.html?page=2&type=http","Pagination follows same-site links and decodes HTML entities");
        Check(ProxyScraperManager.NextWebsitePage(new Uri("https://advanced.name/freeproxy"),"<a href='/buy?page=2'>buy</a>",2)==null,"Pagination does not wander into unrelated paths");
        Check(ProxyScraperManager.NextWebsitePage(new Uri("https://www.89ip.cn/index_1.html"),"<a href='index_2.html'>2</a>",2)=="https://www.89ip.cn/index_2.html","Chinese feed numbered HTML pages are supported");
        foreach(bool loop in new[]{false,true})
        {
            var handler=new RedirectHandler {Loop=loop}; var candidates=new ConcurrentDictionary<string,ProxyEndpoint>();var scraper=new ProxyScraperManager(candidates);SourceResult report=null;scraper.OnSourceCompleted+=r=>report=r;
            using(var client=new HttpClient(handler)) await (Task)typeof(ProxyScraperManager).GetMethod("Fetch",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(scraper,new object[]{client,"https://example.org/list",CancellationToken.None});
            Check(loop ? handler.Calls==6&&report.Status.Contains("redirect limit") : handler.Calls==2&&candidates.Count==1,loop?"Redirect loops terminate and report a source error":"HTTP 308 follows relative Location on .NET Framework");
        }
        using(var stream=new ProxyReadStream(new MemoryStream(new byte[1],false)))
        {
            bool closed=false; try { await stream.WriteAsync(new byte[1],0,1,CancellationToken.None); } catch(IOException ex) { closed=ex.InnerException is NotSupportedException; }
            Check(closed,"Read-only/closed write is a connection failure, not an unhandled NotSupportedException");
            using(var cancel=new CancellationTokenSource()) { cancel.Cancel(); bool stopped=false;try { await stream.WriteAsync(new byte[1],0,1,cancel.Token); } catch(OperationCanceledException){stopped=true;} Check(stopped,"Write cancellation is preserved"); }
        }
        var tcpAttempts=new ConcurrentDictionary<string,int>(); var protocolAttempts=new ConcurrentDictionary<string,int>();
        var published=new ConcurrentDictionary<string,int>(); var scheduler=new ProxyCheckScheduler();
        int activeWeight=0,peak=0,wrongTimeout=0;
        scheduler.TcpProbe=async(p,t,c)=> {
            if(t!=1234) Interlocked.Increment(ref wrongTimeout);
            if(p.Key==Endpoint(0).Key && tcpAttempts.AddOrUpdate(p.Key,1,(_,n)=>n+1)==1) throw new SocketException(10055);
            int weight=Interlocked.Increment(ref activeWeight); lock(published) peak=Math.Max(peak,weight);
            try { await Task.Delay(1,c); return null; } finally { Interlocked.Decrement(ref activeWeight); }
        };
        scheduler.ProtocolProbe=async(p,h,s,t,c,r)=> {
            if(t!=1234) Interlocked.Increment(ref wrongTimeout);
            if(p.Key==Endpoint(1).Key && protocolAttempts.AddOrUpdate(p.Key,1,(_,n)=>n+1)==1) throw new IOException("TLS wrapper",new SocketException(10024));
            int weight=Interlocked.Add(ref activeWeight,6); lock(published) peak=Math.Max(peak,weight);
            try { await Task.Delay(1,c); return Good(p); } finally { Interlocked.Add(ref activeWeight,-6); }
        };
        await scheduler.RunAsync(Enumerable.Range(0,50000).Select(Endpoint),Http,Https,500,1234,false,r=>published.AddOrUpdate(r.Endpoint,1,(_,n)=>n+1),CancellationToken.None);
        Check(published.Count==50000 && published.Values.All(n=>n==1),"50,000 endpoints complete exactly once despite TCP and protocol socket exhaustion");
        Check(scheduler.CapacityRetries==2 && tcpAttempts[Endpoint(0).Key]==2 && protocolAttempts[Endpoint(1).Key]==2,"Both resource failures retry the same endpoint");
        Check(wrongTimeout==0,"Budget waiting and recovery never reduce an endpoint's configured timeout");
        Check(peak<=1024 && scheduler.SocketBudget<1024 && activeWeight==0,"Shared weighted socket bound shrinks under pressure and drains completely");
        Check(scheduler.Connecting==0&&scheduler.Probing==0,"50,000-item run drains all stage workers");
        scheduler.TcpProbe=(p,t,c)=>throw new SocketException(10055);
        using(var stop=new CancellationTokenSource(150)) {
            bool canceled=false; try { await scheduler.RunAsync(Enumerable.Range(0,500).Select(Endpoint),Http,Https,500,1234,false,r=>throw new Exception("Must not publish a resource failure"),stop.Token); } catch(OperationCanceledException){canceled=true;}
            Check(canceled&&scheduler.Connecting==0&&scheduler.Probing==0,"Persistent local pressure remains cancellable without false failed verdicts");
        }
        Check(!ProxyTypeDetector.IsLocalCapacityFailure(new SocketException(10061)),"Remote connection refusal is not retried as local socket exhaustion");
        Console.WriteLine("TOTAL: "+count+" resilience assertions passed");
    }
    static int Main(){try {Run().GetAwaiter().GetResult();return 0;} catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
