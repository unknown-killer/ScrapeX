using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ScrapeX;

internal static class ServiceTests
{
    static int count;
    static void Check(bool ok, string detail) { if (!ok) throw new Exception(detail); Console.WriteLine("PASS " + detail); count++; }
    static ProxyEndpoint Endpoint(string text) { ProxyEndpoint p; if (!ProxyEndpoint.TryParse(text, out p)) throw new Exception(text); return p; }
    static async Task Run(string folder)
    {
        var profile = new ServiceProfile(); string key = profile.Server.ApiKey;
        string exported = Path.Combine(folder,"export.json"), installed = Path.Combine(folder,"service-profile.json");
        profile.Save(exported,false);
        Check(!File.ReadAllText(exported).Contains(key),"Exported profile contains no plaintext API key");
        profile = ServiceProfile.Load(exported,false);
        Check(profile.Server.ApiKey==key && profile.Server.ScheduleEnabled,"User profile round-trip preserves key and automatic schedule");
        bool rejected=false;try{ServiceProfile.Load(exported,true);}catch(IOException){rejected=true;}
        Check(rejected,"Service refuses a user-scoped profile");
        string fingerprint=profile.Fingerprint(); profile.TimeoutMs++;
        Check(profile.Fingerprint()!=fingerprint,"Probe configuration changes invalidate old checkpoints");
        var endpoints = new[]{Endpoint("socks5://8.8.8.8:1080"),Endpoint("1.1.1.1:80"),Endpoint("9.9.9.9:80")};
        var result = new ProxyTypeDetector.ProxyTestResult { Endpoint=endpoints[0].Key, Type=ProxyTypeDetector.ProxyType.Socks5, HttpWorking=true, HttpsVerified=true, CheckedAt=DateTime.UtcNow, CountryCode="US", Country="United States", TcpPingMs=12 };
        using(var cp=new CheckCheckpoint(folder)) {
            cp.Begin(endpoints,fingerprint); cp.Record(result);
            cp.Record(new ProxyTypeDetector.ProxyTestResult { Endpoint=endpoints[1].Key, Type=ProxyTypeDetector.ProxyType.Unknown });
        }
        File.AppendAllText(Path.Combine(folder,"check-journal.jsonl"),"{\"torn");
        using(var cp=new CheckCheckpoint(folder)) {
            Check(cp.Restore(fingerprint,24),"Interrupted checkpoint restores despite torn journal tail");
            Check(cp.Completed.Count==2 && cp.Endpoints.Count(p=>!cp.Completed.Contains(p.Key))==1,"Only unfinished endpoints remain in the resumed queue");
            Check(cp.Working.Count==1 && cp.Working[0].TcpPingMs==12 && cp.Working[0].HttpsVerified,"Verified HTTPS result and timings survive restart");
            Check(cp.Endpoints[0].PreferredType==ProxyTypeDetector.ProxyType.Socks5,"Protocol hints survive checkpoint round-trip");
            cp.Record(new ProxyTypeDetector.ProxyTestResult { Endpoint=endpoints[2].Key });
        }
        using(var cp=new CheckCheckpoint(folder)) { Check(cp.Restore(fingerprint,24)&&cp.Completed.Count==3,"Journal can append after recovering a torn line"); }
        using(var cp=new CheckCheckpoint(folder)) { Check(!cp.Restore("changed",24),"Changed probe settings trigger fresh collection"); }
        var manifest=JObject.Parse(File.ReadAllText(Path.Combine(folder,"checkpoint.json")));manifest["CreatedUtc"]=DateTime.UtcNow.AddDays(-2);File.WriteAllText(Path.Combine(folder,"checkpoint.json"),manifest.ToString());
        using(var cp=new CheckCheckpoint(folder)) { Check(!cp.Restore(fingerprint,24),"Expired checkpoint is not reused");cp.Complete(); }
        Check(!File.Exists(Path.Combine(folder,"checkpoint.json")),"Completed checkpoint is removed");

        string schedule=Path.Combine(folder,"schedule-state.json");
        var started=new TaskCompletionSource<bool>();
        var coordinator=new RefreshCoordinator(async token=>{started.TrySetResult(true);await Task.Delay(30000,token);return 0;},schedule);
        coordinator.Start(true,TimeSpan.FromHours(6));await started.Task;await coordinator.StopAsync();
        Check((bool)JObject.Parse(File.ReadAllText(schedule))["Running"],"Interrupted scheduled cycle persists immediate-resume marker");
        var resumed=new TaskCompletionSource<bool>();
        coordinator=new RefreshCoordinator(token=>{resumed.TrySetResult(true);return Task.FromResult(1);},schedule);
        coordinator.Start(true,TimeSpan.FromHours(6));
        Check(await Task.WhenAny(resumed.Task,Task.Delay(3000))==resumed.Task,"Interrupted cycle resumes without waiting for its six-hour interval");
        while(coordinator.IsRunning) await Task.Delay(10);await coordinator.StopAsync();
        int calls=0;coordinator=new RefreshCoordinator(token=>{Interlocked.Increment(ref calls);return Task.FromResult(0);},schedule);
        coordinator.Start(true,TimeSpan.FromHours(6));await Task.Delay(100);
        Check(calls==0 && coordinator.NextUtc>DateTime.UtcNow && coordinator.History.Length>=2,"Idle restart preserves next run and history");await coordinator.StopAsync();

        var reserve=new TcpListener(IPAddress.Loopback,0);reserve.Start();int port=((IPEndPoint)reserve.LocalEndpoint).Port;reserve.Stop();
        profile.Server.Port=port;profile.Server.PublicBaseUrl="http://127.0.0.1:"+port;profile.Save(installed,true);
        Check(ServiceProfile.Load(installed,true).Server.ApiKey==key,"Machine-scoped profile decrypts for independent service host");
        var publication=new ProxyPublication(Path.Combine(folder,"published-proxies.json"));publication.Publish(new[]{PublishedProxy.FromResult(result)});
        string revision=publication.Current.Revision;
        for(int pass=0;pass<2;pass++) {
            var runtime=new ServiceRuntime(folder);runtime.Start();
            try {
                using(var client=new HttpClient(new HttpClientHandler{UseProxy=false}){BaseAddress=new Uri(profile.Server.PublicBaseUrl),Timeout=TimeSpan.FromSeconds(5)}) {
                    client.DefaultRequestHeaders.Add("X-Api-Key",key);
                    var stats=JObject.Parse(await client.GetStringAsync("/api/v1/stats"));
                    Check(runtime.IsHealthy && (string)stats["refresh"]["mode"]=="windows-service","Independent host serves service monitoring, start "+(pass+1));
                    Check(await client.GetStringAsync("/proxies.txt")==result.Endpoint+"\n","Saved publication remains available after host restart "+(pass+1));
                }
                bool locked=false;try{new ServiceRuntime(folder).Start();}catch(IOException){locked=true;}
                Check(locked,"Exclusive data lease prevents two hosts sharing checkpoint state");
            } finally { await Task.WhenAll(runtime.StopAsync(),runtime.StopAsync()); }
            Check(!runtime.IsHealthy,"Service host shutdown closes listener and scheduler");
        }
        publication.Load();Check(publication.Current.Revision==revision,"Stopping and restarting does not replace a valid publication");
        // Exercise an actual process exit, not just two objects in one test process.
        for (int pass=0;pass<2;pass++) {
            string ready=Path.Combine(folder,"host-ready");File.Delete(ready);
            using(var child=Process.Start(new ProcessStartInfo { FileName=typeof(ServiceTests).Assembly.Location,
                Arguments="--host \""+folder+"\"",UseShellExecute=false,CreateNoWindow=true })) {
                try {
                    var deadline=DateTime.UtcNow.AddSeconds(10);
                    while(!File.Exists(ready)&&!child.HasExited&&DateTime.UtcNow<deadline) await Task.Delay(25);
                    Check(File.Exists(ready)&&!child.HasExited,"Headless service runtime starts in its own process "+(pass+1));
                    using(var client=new HttpClient(new HttpClientHandler{UseProxy=false})) {
                        client.DefaultRequestHeaders.Add("X-Api-Key",key);
                        Check(await client.GetStringAsync(profile.Server.PublicBaseUrl+"/proxies.txt")==result.Endpoint+"\n","Publication survives abrupt process termination "+(pass+1));
                    }
                } finally { if(!child.HasExited)child.Kill();child.WaitForExit(); }
            }
        }
    }
    static int Main(string[] args) {
        if(args.Length==2 && args[0]=="--host") {
            var host=new ServiceRuntime(args[1]);host.Start();File.WriteAllText(Path.Combine(args[1],"host-ready"),"ready");Thread.Sleep(Timeout.Infinite);return 0;
        }
        string folder=Path.Combine(Path.GetTempPath(),"ScrapeX-ServiceTests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        try{Run(folder).GetAwaiter().GetResult();Console.WriteLine(count+" service assertions passed.");return 0;}
        catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
        finally{Directory.Delete(folder,true);}
    }
}
