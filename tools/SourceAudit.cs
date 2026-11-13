using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using ScrapeX;
class SourceAudit
{
    static int Main(string[] args)
    {
        var root=args[0];var records=JArray.Parse(File.ReadAllText(Path.Combine(root,"downloads.json")));
        foreach(var item in records)
        {
            if ((string)item["status"]!="200") continue;
            try
            {
                int raw;var parsed=SourceParser.Parse(File.ReadAllText(Path.Combine(root,(string)item["file"])),out raw,(string)item["url"]);
                item["valid"]=parsed.Select(p=>p.Key).Distinct().Count();item["parse_status"]="ok";
                File.WriteAllLines(Path.Combine(root,(string)item["file"]+".endpoints"),parsed.Select(p=>p.Key).Distinct());
            }
            catch(Exception ex) { item["valid"]=0;item["parse_status"]=ex.GetType().Name+": "+ex.Message; }
        }
        File.WriteAllText(Path.Combine(root,"parsed.json"),records.ToString());
        Console.WriteLine("Parsed sources: "+records.Count+"; usable: "+records.Count(r=>(int?)r["valid"]>0)+"; new usable: "+records.Count(r=>(bool)r["new"] && (int?)r["valid"]>0));return 0;
    }
}
