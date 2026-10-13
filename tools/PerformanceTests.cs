using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ScrapeX;
class PerformanceTests
{
 static int active,peak; static int assertions;
 static void Assert(bool ok,string message){if(!ok)throw new Exception(message);Console.WriteLine("PASS: "+message);assertions++;}
 static ProxyEndpoint Local(int port){ProxyEndpoint p;ProxyEndpoint.TryParse("8.8.8.8:"+port,out p);typeof(ProxyEndpoint).GetProperty("Host").SetValue(p,"127.0.0.1",null);return p;}
 static readonly Uri plain=new Uri("http://1.1.1.1/generate_204"),secure=new Uri("https://1.1.1.1/generate_204");
 static async Task<double> Batch(ProxyEndpoint p,bool legacy)
 {
  var clock=Stopwatch.StartNew();
  using(var workers=new SemaphoreSlim(20))
   await Task.WhenAll(Enumerable.Range(0,40).Select(async i=>{await workers.WaitAsync();try{
    var detector=new ProxyTypeDetector();
    if(legacy){foreach(var target in new[]{secure,secure,plain,plain})await detector.DetectAsync(p,target,200,CancellationToken.None);}
    else {var r=await detector.CheckAsync(p,plain,secure,200,CancellationToken.None);if(r.Type!=ProxyTypeDetector.ProxyType.Unknown)throw new Exception("Silent peer marked working");}
   }finally{workers.Release();}}));
  return clock.Elapsed.TotalSeconds;
 }
 static async Task Run()
 {
  var listener=new TcpListener(IPAddress.Loopback,0);listener.Start(1000);var p=Local(((IPEndPoint)listener.LocalEndpoint).Port);
  var handlers=new ConcurrentBag<Task>();
  var accept=Task.Run(async()=>{try{while(true){var c=await listener.AcceptTcpClientAsync();handlers.Add(Task.Run(async()=>{int a=Interlocked.Increment(ref active);int old;do{old=peak;if(old>=a)break;}while(Interlocked.CompareExchange(ref peak,a,old)!=old);using(c)try{var buffer=new byte[2048];while(await c.GetStream().ReadAsync(buffer,0,buffer.Length)>0){}}catch(IOException){}finally{Interlocked.Decrement(ref active);}}));}}catch(ObjectDisposedException){}catch(SocketException){}});
  try
  {
   double previous=await Batch(p,true);await Task.Delay(100);peak=0;
   double current=await Batch(p,false);await Task.Delay(100);
   Console.WriteLine("Silent reachable proxy benchmark: 40 endpoints, 20 workers, 200ms timeout.");
   Console.WriteLine("Previous 12-attempt schedule: "+previous.ToString("F3")+"s; new fast pipeline: "+current.ToString("F3")+"s; speedup: "+(previous/current).ToString("F2")+"x; peak accepted sockets: "+peak);
   Assert(current<previous/3,"At least 3x faster on identical silent peers (same timeout)");
   Assert(current<2,"Batch bounded by parallel rounds instead of 12 sequential timeouts");
   Assert(active==0,"All completed and canceled sockets released");
   var sw=Stopwatch.StartNew();using(var ct=new CancellationTokenSource(80))
   {try{await new ProxyTypeDetector().CheckAsync(p,plain,secure,5000,ct.Token);throw new Exception("Cancellation swallowed");}catch(OperationCanceledException){}}
   Assert(sw.ElapsedMilliseconds<1000,"Cancellation promptly closes concurrent probes");
   await Task.Delay(100);Assert(active==0,"Cancellation leaves no live checker sockets");
   sw.Restart();
   var retry=await new ProxyTypeDetector().CheckAsync(p,new Uri("http://www.google.com/generate_204"),new Uri("https://www.google.com/generate_204"),200,CancellationToken.None,true);
   Assert(retry.Type==ProxyTypeDetector.ProxyType.Unknown && retry.Detail.Contains("Alternate microsoft.com") && retry.Detail.Contains("Alternate gstatic.com"),"Optional retry checks both alternate targets without inventing success");
   Assert(sw.ElapsedMilliseconds<1500,"Optional retry is two parallel rounds, not twelve serial waits");
   listener.Stop();await accept;await Task.WhenAll(handlers.ToArray());
   var refused=await new ProxyTypeDetector().CheckAsync(p,plain,secure,500,CancellationToken.None);
   Assert(refused.Type==ProxyTypeDetector.ProxyType.Unknown&&refused.Detail.StartsWith("TCP:"),"Refused endpoint exits at single TCP gate");
   Console.WriteLine("TOTAL: "+assertions+" performance assertions passed");
  }finally{listener.Stop();}
 }
 static int Main(){try{Run().GetAwaiter().GetResult();return 0;}catch(Exception ex){Console.Error.WriteLine(ex);return 1;}}
}
