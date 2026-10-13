using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using ScrapeX;
class SchedulerTests
{
 static int count;static void Assert(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS: "+name);count++;}
 static ProxyEndpoint Local(int port){ProxyEndpoint p;ProxyEndpoint.TryParse("8.8.8.8:"+port,out p);typeof(ProxyEndpoint).GetProperty("Host").SetValue(p,"127.0.0.1",null);return p;}
 static async Task Run()
 {
  var listener=new TcpListener(IPAddress.Loopback,0);listener.Start(1000);int port=((IPEndPoint)listener.LocalEndpoint).Port;
  var handlers=new ConcurrentBag<Task>();int active=0;
  var serve=Task.Run(async()=>{try{while(true){var client=await listener.AcceptTcpClientAsync();handlers.Add(Task.Run(async()=>{Interlocked.Increment(ref active);using(client)try{
   var s=client.GetStream();var one=new byte[1];if(await s.ReadAsync(one,0,1)==0)return;char first=(char)one[0];if(first!='G'&&first!='C')return;
   string header="";while(!header.EndsWith("\r\n\r\n")){if(await s.ReadAsync(one,0,1)==0)return;header+=(char)one[0];}
   if(first=='G')await Task.Delay(180);
   byte[] response=Encoding.ASCII.GetBytes(first=='G'?"HTTP/1.1 204 No Content\r\n\r\n":"HTTP/1.1 403 Forbidden\r\n\r\n");await s.WriteAsync(response,0,response.Length);
  }catch(IOException){}catch(ObjectDisposedException){}finally{Interlocked.Decrement(ref active);}}));}}catch(ObjectDisposedException){}catch(SocketException){}});
  try
  {
   var http=new Uri("http://1.1.1.1/generate_204");var https=new Uri("https://1.1.1.1/generate_204");
   var scheduler=new ProxyCheckScheduler();var results=new ConcurrentBag<ProxyTypeDetector.ProxyTestResult>();var clock=Stopwatch.StartNew();
   var job=scheduler.RunAsync(Enumerable.Range(0,32).Select(i=>Local(port)),http,https,4,500,false,r=>results.Add(r),CancellationToken.None);
   int maxTcp=0,maxProbe=0;while(!job.IsCompleted){maxTcp=Math.Max(maxTcp,scheduler.Connecting);maxProbe=Math.Max(maxProbe,scheduler.Probing);await Task.Delay(10);}await job;
   Console.WriteLine("Queued slow healthy proxies: 32; 4 protocol workers; 500ms timeout; total "+clock.Elapsed.TotalSeconds.ToString("F2")+"s; peak TCP="+maxTcp+", probes="+maxProbe);
   Assert(results.Count==32 && results.All(r=>r.HttpWorking&&!r.HttpsVerified),"Every delayed healthy proxy survives queue waits longer than its timeout");
   Assert(clock.ElapsedMilliseconds>500,"Fixture actually exercises queue waiting beyond the timeout");
   Assert(maxTcp<=12&&maxProbe<=4,"Both scheduler stages respect concurrency bounds");
   Assert(scheduler.Connecting==0&&scheduler.Probing==0,"Stage counters drain to zero");
   using(var cancel=new CancellationTokenSource(50))
   {bool canceled=false;try{await scheduler.RunAsync(Enumerable.Range(0,100).Select(i=>Local(port)),http,https,4,500,false,r=>{},cancel.Token);}catch(OperationCanceledException){canceled=true;}Assert(canceled,"Cancel drains producers, consumers and backpressure waits");}
   bool diskFailure=false;try{await scheduler.RunAsync(Enumerable.Range(0,20).Select(i=>Local(port)),http,https,4,500,false,r=>{throw new IOException("fixture disk failure");},CancellationToken.None);}catch(IOException e){diskFailure=e.Message=="fixture disk failure";}Assert(diskFailure,"Output failure is preserved rather than hidden by peer cancellation");
   var closed=new TcpListener(IPAddress.Loopback,0);closed.Start();int closedPort=((IPEndPoint)closed.LocalEndpoint).Port;closed.Stop();
   var dead=Enumerable.Range(0,24).Select(i=>Local(closedPort)).ToArray();
   clock.Restart();using(var gate=new SemaphoreSlim(4))await Task.WhenAll(dead.Select(async p=>{await gate.WaitAsync();try{await new ProxyTypeDetector().CheckAsync(p,http,https,200,CancellationToken.None);}finally{gate.Release();}}));
   double serial=clock.Elapsed.TotalSeconds;clock.Restart();int complete=0;
   await scheduler.RunAsync(dead,http,https,4,200,false,r=>Interlocked.Increment(ref complete),CancellationToken.None);
   Console.WriteLine("TCP-only failure batch, 24 endpoints, base 4, 200ms: previous coupled workers="+serial.ToString("F3")+"s; staged scheduler="+clock.Elapsed.TotalSeconds.ToString("F3")+"s");
   Assert(complete==24,"Staged admission publishes every TCP failure exactly once");
   await scheduler.RunAsync(new ProxyEndpoint[0],http,https,4,500,false,r=>{throw new Exception();},CancellationToken.None);Assert(true,"Empty input finishes without deadlock");
   await Task.Delay(250);Assert(active==0,"Canceled pipeline leaves no connected peers");
   Console.WriteLine("TOTAL: "+count+" scheduler assertions passed");
  }
  finally{listener.Stop();await serve;await Task.WhenAll(handlers.ToArray());}
 }
 static int Main(){try{Run().GetAwaiter().GetResult();return 0;}catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
