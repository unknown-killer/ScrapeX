using System;
using System.Linq;
using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ScrapeX;
class LiveSmoke
{
 static void Main(string[] args){Run(args).GetAwaiter().GetResult();}
 static async Task Run(string[] args)
 {
  var candidates=new ConcurrentDictionary<string,ProxyEndpoint>();var scraper=new ProxyScraperManager(candidates);
  scraper.OnSourceCompleted+=r=>Console.WriteLine(r.ToString());
  using(var timeout=new CancellationTokenSource(TimeSpan.FromSeconds(90)))
  {try{await scraper.StartScrapingAsync(File.ReadAllLines(args[0]),timeout.Token);}catch(OperationCanceledException){Console.WriteLine("Source smoke budget reached; partial results retained.");}}
  Console.WriteLine("Unique endpoints: "+candidates.Count);
  // Real-network smoke only; no claim that every scraped endpoint works.
  var checker=new ProxyTypeDetector();int passed=0,tested=0;
  using(var gate=new SemaphoreSlim(10))
  {
   await Task.WhenAll(candidates.Values.OrderBy(x=>x.Key).Take(30).Select(async p=>
   {
    await gate.WaitAsync();try{var r=await checker.DetectAsync(p,new Uri("https://www.google.com/generate_204"),1200,CancellationToken.None);Interlocked.Increment(ref tested);if(r.Type!=ProxyTypeDetector.ProxyType.Unknown){Interlocked.Increment(ref passed);Console.WriteLine(r.Endpoint+" "+r.Type+" "+r.SpeedMs+"ms");}}finally{gate.Release();}
   }));
  }
  Console.WriteLine("HTTPS sample: "+passed+" / "+tested+" passed (1200ms per protocol).");
 }
}
