using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ScrapeX
{
    public static class ServiceManager
    {
        public const string Name = "ScrapeXServer";
        private const string InstallerKey = @"SOFTWARE\ScrapeXServiceInstaller";
        public static string InstallFolder { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "ScrapeX", "Service"); } }
        public static string BinaryCommand { get { return "\"" + Path.Combine(InstallFolder, "ScrapeX.exe") + "\" --service"; } }
        public static string Status()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + Name))
            {
                if (key == null) return "Not installed";
                if (!string.Equals(key.GetValue("ImagePath") as string, BinaryCommand, StringComparison.OrdinalIgnoreCase)) return "Different installation";
            }
            using (var service = new ServiceController(Name)) { service.Refresh(); return service.Status.ToString(); }
        }
        public static void RestrictFolder(string folder, bool serviceAccess)
        {
            Directory.CreateDirectory(folder);
            var security = new DirectorySecurity(); security.SetAccessRuleProtection(true, false);
            foreach (var sid in new[] { WindowsIdentity.GetCurrent().User, new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null) })
                security.AddAccessRule(new FileSystemAccessRule(sid, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            if (serviceAccess) security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null), FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            Directory.SetAccessControl(folder, security);
        }
        public static string Quote(string value)
        {
            var output = new StringBuilder("\""); int slashes = 0;
            foreach (char c in value) { if (c == '\\') { slashes++; continue; } output.Append('\\', c == '"' ? slashes * 2 + 1 : slashes); output.Append(c); slashes = 0; }
            return output.Append('\\', slashes * 2).Append('"').ToString();
        }
        private static string Native(string file, bool requireSuccess, params string[] arguments)
        {
            using (var process = Process.Start(new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, file), string.Join(" ", arguments.Select(Quote)))
            { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true }))
            {
                var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(30000)) { process.Kill(); throw new System.TimeoutException(file + " did not finish in time."); }
                string text = stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult();
                if (requireSuccess && process.ExitCode != 0) throw new IOException(file + ": " + text.Trim());
                return text;
            }
        }
        private static void PlainPath(string path)
        {
            for (var dir = new DirectoryInfo(path); dir != null; dir = dir.Parent)
                if (dir.Exists && (dir.Attributes & FileAttributes.ReparsePoint) != 0) throw new IOException("Installation path contains a redirected folder.");
        }
        private static void Stop()
        {
            if (Status() == "Not installed") return;
            using (var service = new ServiceController(Name))
            {
                if (service.Status == ServiceControllerStatus.StartPending) service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                if (service.Status != ServiceControllerStatus.Stopped) { if (service.Status != ServiceControllerStatus.StopPending) service.Stop(); service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(40)); }
            }
        }
        public static void Execute(string action, string profilePath)
        {
            if (!new[] { "Install", "Start", "Stop", "Restart" }.Contains(action)) throw new ArgumentException("Unknown service action.");
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) throw new UnauthorizedAccessException("Windows administrator approval is required.");
            string state = Status();
            if (state == "Different installation") throw new IOException("ScrapeXServer belongs to a different installation.");
            using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\" + Name))
                if (key != null && !string.Equals(key.GetValue("ObjectName") as string, @"NT AUTHORITY\LocalService", StringComparison.OrdinalIgnoreCase)) throw new IOException("The existing service uses a different account.");
            if (action == "Install")
            {
                var profile = ServiceProfile.Load(profilePath, true); profile.Validate();
                if (string.IsNullOrEmpty(profile.MonitorUserSid)) throw new IOException("Export a current service profile from the desktop first.");
                string source = AppDomain.CurrentDomain.BaseDirectory;
                if (Path.GetFullPath(source).TrimEnd('\\').Equals(InstallFolder.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase)) throw new IOException("Run Install / update from your extracted release, not the service installation folder.");
                string[] files = { "ScrapeX.exe", "ScrapeX.exe.config", "Newtonsoft.Json.dll", "sources.txt" };
                foreach (var file in files) if (!File.Exists(Path.Combine(source, file))) throw new FileNotFoundException("Keep the complete extracted release together.", file);
                PlainPath(InstallFolder); PlainPath(ServiceRuntime.DefaultFolder);
                string prefix = profile.Server.Prefix;
                string reservation = Native("netsh.exe", false, "http", "show", "urlacl", "url=" + prefix);
                bool exists = reservation.Contains(prefix);
                if (exists && !reservation.Contains("(A;;GX;;;LS)") && !reservation.Contains("(A;;GX;;;S-1-5-19)")) throw new IOException("The selected listener URL is reserved for another account. Choose another port.");
                Stop(); Directory.CreateDirectory(InstallFolder); RestrictFolder(ServiceRuntime.DefaultFolder, true);
                foreach (var file in files) File.Copy(Path.Combine(source, file), Path.Combine(InstallFolder, file), true);
                profile.Save(Path.Combine(ServiceRuntime.DefaultFolder, "service-profile.json"), true);
                if (!exists)
                {
                    Native("netsh.exe", true, "http", "add", "urlacl", "url=" + prefix, "sddl=D:(A;;GX;;;LS)");
                    using (var key = Registry.LocalMachine.CreateSubKey(InstallerKey))
                        key.SetValue("OwnedUrls", ((key.GetValue("OwnedUrls") as string[]) ?? new string[0]).Concat(new[] { prefix }).Distinct().ToArray(), RegistryValueKind.MultiString);
                }
                if (state == "Not installed") Native("sc.exe", true, "create", Name, "binPath=", BinaryCommand, "start=", "delayed-auto", "obj=", @"NT AUTHORITY\LocalService", "DisplayName=", "ScrapeX Proxy Feed");
                Native("sc.exe", true, "description", Name, "Scheduled proxy discovery, HTTP/TLS verification and filtered API feeds.");
                Native("sc.exe", true, "config", Name, "start=", "delayed-auto");
                Native("sc.exe", true, "failure", Name, "reset=", "86400", "actions=", "restart/5000/restart/15000/restart/60000");
                Native("sc.exe", true, "failureflag", Name, "1");
            }
            if (action == "Stop" || action == "Restart") Stop();
            if (action != "Stop")
            {
                using (var service = new ServiceController(Name))
                {
                    if (service.Status == ServiceControllerStatus.StopPending) service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(40));
                    if (service.Status != ServiceControllerStatus.Running && service.Status != ServiceControllerStatus.StartPending) service.Start();
                    service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                }
                using (var deadline = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(25)))
                {
                    while (true)
                    {
                        try { ServiceMonitor.ReadAsync(deadline.Token).GetAwaiter().GetResult(); break; }
                        catch (Exception) when (!deadline.IsCancellationRequested) { System.Threading.Thread.Sleep(400); }
                        catch (Exception ex) { throw new IOException("Windows started the service, but live readiness could not be confirmed. Use Install / update service for older versions and inspect ProgramData\\ScrapeX\\Service\\service.log.", ex); }
                    }
                }
            }
        }
        public static async Task<bool> ElevateAsync(string action, ServiceProfile profile)
        {
            string staging = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ScrapeX", "Service setup", Guid.NewGuid().ToString("N"));
            string path = "";
            try
            {
                if (profile != null) { RestrictFolder(staging, false); path = Path.Combine(staging, "profile.json"); profile.Save(path, true); }
                using (var process = Process.Start(new ProcessStartInfo(typeof(ServiceManager).Assembly.Location,
                    "--manage-service " + Quote(action) + " " + Quote(path)) { UseShellExecute = true, Verb = "runas", WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory }))
                { await Task.Run(() => process.WaitForExit()); return process.ExitCode == 0; }
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223) { throw new OperationCanceledException("Windows administrator approval was cancelled. No service action was started."); }
            finally { if (File.Exists(path)) File.Delete(path); if (Directory.Exists(staging)) Directory.Delete(staging); }
        }
    }
}

