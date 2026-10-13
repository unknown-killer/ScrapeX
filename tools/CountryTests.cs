using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ScrapeX;
class CountryTests
{
 static int assertions;static void Assert(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS: "+name);assertions++;}
 sealed class Handler:HttpMessageHandler
 {
  public int Mode,Calls;
  protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken ct)
  {
   Interlocked.Increment(ref Calls);string host=request.RequestUri.Host;
   if(Mode==4)throw new Exception("Cache should avoid network");
   var result=new HttpResponseMessage(Mode==0?HttpStatusCode.OK:HttpStatusCode.ServiceUnavailable);
   if(Mode==2&&host=="api.country.is"){result.StatusCode=(HttpStatusCode)429;result.Headers.RetryAfter=new System.Net.Http.Headers.RetryConditionHeaderValue(TimeSpan.FromMinutes(1));}
   result.Content=new StringContent(Mode==0 ? host=="api.country.is"?"{\"ip\":\"1.1.1.1\",\"country\":\"DE\"}":host=="ipwho.is"?"{\"success\":false}":"{\"ip\":\"8.8.8.8\",\"country_code\":\"US\"}" : "{}");
   return Task.FromResult(result);
  }
 }
 static ProxyTypeDetector.ProxyTestResult Route()=>new ProxyTypeDetector.ProxyTestResult{Endpoint="8.8.8.8:1080",Type=ProxyTypeDetector.ProxyType.Socks5,HttpWorking=true};
 static async Task Run()
 {
  Assert(CountryResolver.Parse("country.is","8.8.8.8","{\"ip\":\"1.1.1.1\",\"country\":\"DE\"}")==null,"Reject country for wrong IP / accidental exit-IP response");
  Assert(CountryResolver.Parse("country.is","8.8.8.8","{\"ip\":\"8.8.8.8\",\"country\":\"ZZ\"}")==null,"Reject invalid ISO country code");
  Assert(CountryResolver.Parse("country.is","8.8.8.8","{\"ip\":\"8.8.8.8\",\"country\":\"de\"}").Name=="Germany","Normalize ISO code and local country name");
  var folder=Path.Combine(Path.GetTempPath(),"ScrapeX-country-tests-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
  string cache=Path.Combine(folder,"cache.json");
  var first=new Handler();using(var resolver=new CountryResolver(cache,first))
  {
   var found=await resolver.ResolveAsync(Route(),CancellationToken.None);Assert(found.Code=="US"&&found.Source.StartsWith("ipapi.co"),"Third provider succeeds after missing and mismatched responses");
   int calls=first.Calls;await resolver.ResolveAsync(Route(),CancellationToken.None);Assert(calls==first.Calls,"Positive memory cache avoids repeated requests");
  }
  var cached=new Handler{Mode=4};using(var resolver=new CountryResolver(cache,cached))Assert((await resolver.ResolveAsync(Route(),CancellationToken.None)).Code=="US"&&cached.Calls==0,"Persistent cache works across resolver instances");
  foreach(int mode in new[]{1,2})
  {
   int proxyCalls=0,blockedCalls=0;
   using(var resolver=new CountryResolver(Path.Combine(folder,"route"+mode+".json"),new Handler{Mode=mode},(proxy,type,uri,ct)=>
   {
    Interlocked.Increment(ref proxyCalls);if(uri.Host=="api.country.is")Interlocked.Increment(ref blockedCalls);
    if(type!=ProxyTypeDetector.ProxyType.Socks5 || !uri.AbsoluteUri.Contains("8.8.8.8")) throw new Exception("Proxy route/IP mismatch");
    return Task.FromResult(new GeoHttpResponse{Status=200,Body=uri.Host=="api.country.is"?"{\"ip\":\"8.8.8.8\",\"country\":\"US\"}":"{\"ip\":\"8.8.8.8\",\"country_code\":\"US\"}"});
   }))
   {
    var found=await resolver.ResolveAsync(Route(),CancellationToken.None);
    Assert(found.Code=="US"&&proxyCalls>0&&found.Source.Contains("via Socks5"),"Direct failure recovers through SOCKS5 route");
    if(mode==2)Assert(blockedCalls==0,"429 cooldown applies to proxy route too");
   }
  }
  var chunked=ProxyTypeDetector.ParseCountryHttp("HTTP/1.1 200 OK\r\nTransfer-Encoding: chunked\r\n\r\n2\r\n{}\r\n0\r\n\r\n");
  Assert(chunked.Status==200&&chunked.Body=="{}","Proxy country transport decodes chunked JSON");
  bool rejected=false;try{ProxyTypeDetector.ParseCountryHttp("HTTP/1.1 200 OK\r\nTransfer-Encoding: chunked\r\n\r\nFF\r\n{}\r\n0\r\n\r\n");}catch(IOException){rejected=true;}Assert(rejected,"Truncated chunked response is rejected");
  using(var resolver=new CountryResolver(Path.Combine(folder,"cancel.json"),new Handler{Mode=1}))
  using(var cancel=new CancellationTokenSource())
  {cancel.Cancel();bool canceled=false;try{await resolver.ResolveAsync(Route(),cancel.Token);}catch(OperationCanceledException){canceled=true;}Assert(canceled,"Country lookup cancellation propagates");}
  Console.WriteLine("TOTAL: "+assertions+" country assertions passed");
 }
 static int Main(){try{Run().GetAwaiter().GetResult();return 0;}catch(Exception e){Console.Error.WriteLine(e);return 1;}}
}
