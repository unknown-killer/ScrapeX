using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScrapeX;

internal static class ServiceUiTests
{
    static int count;
    static void Check(bool ok, string message) { if(!ok)throw new Exception(message);Console.WriteLine("PASS "+message);count++; }
    static T Field<T>(object target,string field) { return (T)target.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target); }
    static void Render(Control control, string path)
    {
        using(var form=new Form { ShowInTaskbar=false, StartPosition=FormStartPosition.Manual, Location=new Point(-10000,-10000), ClientSize=control.Size })
        {
            form.Controls.Add(control);form.Show();Application.DoEvents();control.PerformLayout();
            using(var bitmap=new Bitmap(control.Width,control.Height)){control.DrawToBitmap(bitmap,new Rectangle(Point.Empty,control.Size));bitmap.Save(path);}
            form.Controls.Remove(control);form.Close();
        }
    }
    static JObject Fixture() { return JObject.Parse(@"{'revision':'fixture','publishedUtc':'2026-11-13T00:00:00Z','available':8,'https':6,'expired':2,'httpOnly':2,'requests':15,'rejected':1,'errors':0,'active':0,'bytesSent':8192,'countries':[{'code':'DE','name':'Germany','count':8,'https':6,'http':3,'socks4':1,'socks5':4}],'protocols':[{'type':'http','count':3},{'type':'socks4','count':1},{'type':'socks5','count':4}],'refresh':{'mode':'windows-service','phase':'Checking HTTP + TLS','collected':100,'checkedCount':45,'working':8,'nextUtc':null,'history':[]}}"); }
    static async Task Pipes()
    {
        string pipeName="ScrapeX.Tests."+Guid.NewGuid().ToString("N"),sid=WindowsIdentity.GetCurrent().User.Value;
        var security=ServiceMonitor.Security(sid);
        var rules=security.GetAccessRules(true,true,typeof(SecurityIdentifier)).Cast<PipeAccessRule>().ToArray();
        Check(rules.Any(r=>r.IdentityReference.Value=="S-1-5-2"&&r.AccessControlType==AccessControlType.Deny),"Service telemetry denies network logons");
        Check(!rules.Any(r=>r.IdentityReference.Value=="S-1-1-0"&&r.AccessControlType==AccessControlType.Allow),"Telemetry is not granted to Everyone");
        using(var monitor=new ServiceMonitor(()=>Fixture(),sid,pipeName)) {
            monitor.Start();
            var result=await ServiceMonitor.ReadAsync(CancellationToken.None,pipeName);
            Check((int)result["available"]==8 && (string)result["refresh"]["mode"]=="windows-service","Named pipe returns statistics from the service host");
            Check(!result.ToString().Contains("apiKey"),"Local telemetry contains no API credential");
            Check((int)result["countries"][0]["socks5"]==4,"Country protocol breakdown travels through local telemetry");
            using(var client=new NamedPipeClientStream(".",pipeName,PipeDirection.In,PipeOptions.Asynchronous)) { await client.ConnectAsync(2000); }
            result=await ServiceMonitor.ReadAsync(CancellationToken.None,pipeName);
            Check((int)result["available"]==8&&monitor.IsHealthy,"Abandoned monitoring client does not stop the service endpoint");
            var watch=Stopwatch.StartNew();await monitor.StopAsync();
            Check(watch.ElapsedMilliseconds<1500&&!monitor.IsHealthy,"Stopping an idle telemetry listener cancels its pending connection");
        }
        using(var stop=new CancellationTokenSource()) {
            stop.Cancel();bool cancelled=false;try{await ServiceMonitor.ReadAsync(stop.Token,pipeName);}catch(OperationCanceledException){cancelled=true;}
            Check(cancelled,"Closing the desktop cancels pending telemetry requests");
        }
        using(var wrong=new ServiceMonitor(()=>new {refresh=new{mode="desktop"}},sid,pipeName)) {
            wrong.Start();bool rejected=false;try{await ServiceMonitor.ReadAsync(CancellationToken.None,pipeName);}catch(IOException){rejected=true;}
            Check(rejected,"Desktop telemetry cannot be mistaken for Windows service data");await wrong.StopAsync();
        }
    }
    static void Ui(string output)
    {
        Directory.CreateDirectory(output);
        using(var monitor=new MonitorControl()) {
            Check(monitor.ServiceSelected,"Monitoring defaults to Windows service");
            using(var flag=Image.FromFile(Path.Combine("ScrapeX","Resources","de.png"))) monitor.UpdateService(Fixture(),_=>new Bitmap(flag,new Size(20,13)));
            Check(Field<Label>(monitor,"availableValue").Text=="8"&&Field<Label>(monitor,"httpsValue").Text=="6","Service inventory populates monitor cards");
            Check(Field<ProgressBar>(monitor,"runProgress").Value==450,"Service progress uses service checked and collected counts");
            Check((long)Field<DataGridView>(monitor,"countryGrid").Rows[0].Cells[7].Value==4,"Country grid displays service SOCKS5 totals");
            Check(Field<Label>(monitor,"runState").Text.Contains("WINDOWS SERVICE"),"Live service status is explicitly labelled");
            Field<Label>(monitor,"intro").Text="Illustrative test fixture · values below demonstrate the Windows service view.";
            monitor.Size=new Size(1200,800);monitor.CreateControl();
            Render(monitor,Path.Combine(output,"service-monitor-v1.3.png"));
            monitor.ServiceUnavailable("Stopped",null);
            Check(Field<Label>(monitor,"availableValue").Text=="—"&&Field<DataGridView>(monitor,"countryGrid").Rows.Count==0,"Stopped service clears stale inventory and country rows");
            Check(Field<ProgressBar>(monitor,"runProgress").Value==0,"Offline service clears progress");
            Field<ComboBox>(monitor,"target").SelectedIndex=1;
            Check(!monitor.ServiceSelected,"Desktop monitoring is an explicit separate selection");
            monitor.SelectService();Check(monitor.ServiceSelected,"Successful service actions can select the service view");
        }
        using(var server=new ServerControl()) {
            string action=null;server.ServiceActionRequested+=value=>action=value;
            server.SetServiceState("Not installed",false,null);
            Check(Field<Button>(server,"serviceInstall").Enabled&&!Field<Button>(server,"serviceStart").Enabled&&!Field<Button>(server,"serviceStop").Enabled,"Not-installed state offers installation only");
            Field<Button>(server,"serviceInstall").PerformClick();Check(action=="Install","Install button dispatches the service action without a shell prompt");
            server.SetServiceState("Stopped",false,null);Field<Button>(server,"serviceStart").PerformClick();Check(action=="Start","Stopped state enables the Start service action");
            server.SetServiceState("Running",false,"Live service telemetry connected");Field<Button>(server,"serviceStop").PerformClick();Check(action=="Stop","Running state enables the Stop service action");
            Field<Button>(server,"serviceRestart").PerformClick();Check(action=="Restart","Restart button dispatches a distinct service action");
            server.SetServiceState("Running",true,null);
            Check(new[]{"serviceInstall","serviceStart","serviceStop","serviceRestart"}.All(n=>!Field<Button>(server,n).Enabled),"All service buttons lock while an action is in progress");
            server.SetServiceState("Not installed",false,"Install once to run after sign-out and reboot.");server.LoadSettings(new ServerSettings());server.Size=new Size(1200,865);server.CreateControl();
            server.SetState(false,false,false,"DESKTOP API · OFFLINE");Render(server,Path.Combine(output,"service-controls-v1.3.png"));
        }
    }
    [STAThread] static int Main(string[] args)
    {
        if(args.Length>0&&args[0]=="--quote"){Console.Write(JsonConvert.SerializeObject(args.Skip(1).ToArray()));return 0;}
        try {
            string[] values={"",@"C:\Program Files\ScrapeX\ScrapeX.exe", "\"C:\\Program Files\\ScrapeX\\ScrapeX.exe\" --service",@"C:\tail\", "space \" quote", "$literal; & not-shell"};
            using(var child=Process.Start(new ProcessStartInfo(typeof(ServiceUiTests).Assembly.Location,"--quote "+string.Join(" ",values.Select(ServiceManager.Quote))){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true})) {
                string text=child.StandardOutput.ReadToEnd();child.WaitForExit();Check(JsonConvert.DeserializeObject<string[]>(text).SequenceEqual(values),"Elevated command arguments preserve spaces, quotes and literal shell characters");
            }
            bool rejected=false;try{ServiceManager.Execute("Invalid",null);}catch(ArgumentException){rejected=true;}
            Check(rejected,"Invalid service actions are rejected before any system change");
            Pipes().GetAwaiter().GetResult();
            Application.EnableVisualStyles();Ui(args.Length>0?args[0]:"reports");
            Console.WriteLine(count+" service UI and telemetry assertions passed.");return 0;
        }catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
    }
}
