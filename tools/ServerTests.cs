using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ScrapeX;

internal static class ServerTests
{
    static int count;
    static void Check(bool value, string message) { if (!value) throw new Exception(message); Console.WriteLine("PASS " + message); count++; }
    static void Reject(Action action, string message) { try { action(); } catch (ArgumentException) { Check(true, message); return; } throw new Exception(message); }
    static PublishedProxy Proxy(string endpoint, string protocol, string country, bool https, int age = 0, int ping = 40)
    { return new PublishedProxy { Endpoint = endpoint, Protocol = protocol, CountryCode = country, Country = country == "US" ? "United States" : country == "DE" ? "Germany" : "Unknown", Https = https, Http = true, CheckedUtc = DateTime.UtcNow.AddHours(-age), PingMs = ping }; }
    static ProxyQuery Query(params string[] pairs) { var q = new NameValueCollection(); for (int i=0;i<pairs.Length;i+=2) q.Add(pairs[i],pairs[i+1]); return ProxyQuery.Parse(q,24); }
    static async Task Run(string folder)
    {
        var settings = new ServerSettings(); settings.Save(Path.Combine(folder,"settings.json"));
        Check(!File.ReadAllText(Path.Combine(folder,"settings.json")).Contains(settings.ApiKey),"API key is encrypted on disk");
        Check(ServerSettings.Load(Path.Combine(folder,"settings.json")).ApiKey==settings.ApiKey,"API key restores for the current Windows account");
        Reject(()=>new ServerSettings { PublicBaseUrl="http://user:pass@example.com/" }.Validate(),"Public origin rejects credentials");
        Reject(()=>new ServerSettings { IntervalHours=double.NaN }.Validate(),"Schedule rejects NaN intervals");
        var publication=new ProxyPublication(Path.Combine(folder,"published.json"));
        publication.Publish(new[] { Proxy("8.8.8.8:8080","http","US",true),Proxy("1.1.1.1:1080","socks5","DE",true,0,20),Proxy("9.9.9.9:1080","socks4","ZZ",false),Proxy("8.8.4.4:80","http","US",true,25),Proxy("127.0.0.1:80","http","US",true) });
        Check(publication.Current.Proxies.Length==4,"Publication excludes non-public endpoints");
        var loaded=new ProxyPublication(Path.Combine(folder,"published.json"));loaded.Load();
        Check(loaded.Current.Revision==publication.Current.Revision && loaded.Current.Proxies.Length==4,"Durable publication round-trips");
        Check(Query().Filter(publication.Current,DateTime.UtcNow).Length==2,"Default query returns fresh HTTPS-verified proxies only");
        Check(Query("https","false").Filter(publication.Current,DateTime.UtcNow).Single().Protocol=="socks4","HTTP-only filter excludes HTTPS entries");
        Check(Query("country","us,de","type","socks5").Filter(publication.Current,DateTime.UtcNow).Single().CountryCode=="DE","Country and protocol filters intersect, case insensitive");
        Check(Query("https","all","country","ZZ").Filter(publication.Current,DateTime.UtcNow).Length==1,"Unknown countries have explicit ZZ filter");
        Check(Query("sort","ping").Filter(publication.Current,DateTime.UtcNow).First().PingMs==20,"Latency ordering is stable");
        Check(Query("max_ping","25").Filter(publication.Current,DateTime.UtcNow).Length==1,"Latency threshold filters correctly");
        foreach(var pair in new[]{new[]{"limit","0"},new[]{"limit","10001"},new[]{"offset","-1"},new[]{"https","yes"},new[]{"format","xml"},new[]{"country","USA"},new[]{"type","https"},new[]{"max_age_hours","25"},new[]{"api_key","secret"},new[]{"country","US","country","DE"}})
            Reject(()=>Query(pair),"Invalid query rejected: "+string.Join(" ",pair));
        Check(ProxyApiServer.KeyMatches("abc","abc")&&!ProxyApiServer.KeyMatches("abc","abd")&&!ProxyApiServer.KeyMatches("abc",null),"API key comparison handles mismatch and missing values");
        var reserve=new TcpListener(IPAddress.Loopback,0);reserve.Start();int port=((IPEndPoint)reserve.LocalEndpoint).Port;reserve.Stop();
        settings.Port=port;settings.PublicBaseUrl="http://127.0.0.1:"+port;
        var api=new ProxyApiServer(publication,settings,()=>new { phase="Idle" });api.Start();
        try
        {
            using(var client=new HttpClient(new HttpClientHandler { UseProxy=false }) { BaseAddress=new Uri(settings.PublicBaseUrl), Timeout=TimeSpan.FromSeconds(10) })
            {
                Check((await client.GetAsync("/health")).StatusCode==HttpStatusCode.OK,"Listener health works without a secret");
                var unauthorized=await client.GetAsync("/api/v1/proxies");
                Check(unauthorized.StatusCode==HttpStatusCode.Unauthorized,"Proxy data requires authorization by default (actual: "+(int)unauthorized.StatusCode+" "+await unauthorized.Content.ReadAsStringAsync()+")");
                client.DefaultRequestHeaders.Add("X-Api-Key",settings.ApiKey);
                var response=await client.GetAsync("/api/v1/proxies?country=DE&format=json");var json=JObject.Parse(await response.Content.ReadAsStringAsync());
                Check(response.IsSuccessStatusCode && (int)json["total"]==1 && (string)json["proxies"][0]["protocol"]=="socks5","Live JSON API applies filters and camel-case schema");
                var text=await client.GetStringAsync("/proxies.txt?https=all&country=US");Check(text=="8.8.8.8:8080\n","Text feed is IP:port only, expired results omitted");
                json=JObject.Parse(await client.GetStringAsync("/api/v1/proxies?limit=1"));Check((int)json["count"]==1 && (int)json["total"]==2 && (int)json["nextOffset"]==1,"Pagination exposes correct next offset");
                Check((await client.GetAsync("/api/v1/proxies?limit=banana")).StatusCode==HttpStatusCode.BadRequest,"Malformed queries return 400");
                Check((await client.PostAsync("/api/v1/proxies",new StringContent(""))).StatusCode==HttpStatusCode.MethodNotAllowed,"API is read-only");
                Check((await client.GetAsync("/not-found")).StatusCode==HttpStatusCode.NotFound,"Unknown routes return 404");
                response=await client.SendAsync(new HttpRequestMessage(HttpMethod.Head,"/proxies.txt"));Check(response.IsSuccessStatusCode && (await response.Content.ReadAsByteArrayAsync()).Length==0,"HEAD sends no response body");
                json=JObject.Parse(await client.GetStringAsync("/api/v1/stats"));Check((int)json["available"]==3 && (int)json["expired"]==1 && (int)json["https"]==2,"Monitoring totals match published fresh records");
                client.DefaultRequestHeaders.Remove("X-Api-Key");client.DefaultRequestHeaders.Authorization=new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer",settings.ApiKey);
                Check((await client.GetAsync("/api/v1/proxies")).IsSuccessStatusCode,"Bearer authorization supported");
                publication.Publish(new PublishedProxy[0]);
                json=JObject.Parse(await client.GetStringAsync("/api/v1/proxies"));Check((int)json["total"]==0,"Successful empty publication clears old proxies atomically");
            }
        }
        finally { await api.StopAsync(); }
        Check(!api.IsRunning,"API stops cleanly with no open listener");
        int concurrent=0,maxConcurrent=0,runs=0;
        var coordinator=new RefreshCoordinator(async token=>{int n=Interlocked.Increment(ref concurrent);maxConcurrent=Math.Max(maxConcurrent,n);try{Interlocked.Increment(ref runs);await Task.Delay(35,token);return 3;}finally{Interlocked.Decrement(ref concurrent);}});
        coordinator.Start(true,TimeSpan.FromMilliseconds(30));await Task.Delay(230);await coordinator.StopAsync();
        Check(runs>=2 && maxConcurrent==1,"Scheduled refreshes repeat without overlap");
        Check(coordinator.History.Any(r=>r.Outcome=="Published"),"Refresh history records completed publications");
        var began=new TaskCompletionSource<bool>();var cancelled=new RefreshCoordinator(async token=>{began.TrySetResult(true);await Task.Delay(10000,token);return 0;});
        cancelled.Start(false,TimeSpan.FromHours(1));Check(cancelled.RequestNow(),"Manual refresh can start with schedule disabled");await began.Task;
        Check(!cancelled.RequestNow(),"Duplicate refresh requests are rejected while active");cancelled.CancelCurrent();await Task.Delay(80);await cancelled.StopAsync();
        Check(cancelled.History.Single().Outcome=="Cancelled","Cancellation is recorded and does not publish partial data");
        var failing=new RefreshCoordinator(token=>{throw new IOException("fixture disk error");});failing.Start(true,TimeSpan.FromHours(1));await Task.Delay(80);await failing.StopAsync();
        Check(failing.History.Single().Outcome=="Failed" && failing.History[0].Detail.Contains("fixture"),"Refresh failures are retained in history");
    }
    static int Main()
    {
        string folder=Path.Combine(Path.GetTempPath(),"ScrapeX-server-tests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        try { Run(folder).GetAwaiter().GetResult();Console.WriteLine("TOTAL: "+count+" server assertions passed");return 0; }
        catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
        finally{Directory.Delete(folder,true);}
    }
}
