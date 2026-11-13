using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ScrapeX
{
    public sealed class ServiceMonitor : IDisposable
    {
        public const string PipeName = "ScrapeX.Service.Status.v1";
        private readonly string name, userSid;
        private readonly Func<object> statistics;
        private readonly CancellationTokenSource stop = new CancellationTokenSource();
        private Task loop;
        public bool IsHealthy { get { return loop != null && !loop.IsCompleted; } }
        public ServiceMonitor(Func<object> statistics, string userSid, string name = PipeName)
        { this.statistics = statistics; this.userSid = userSid; this.name = name; }
        public static PipeSecurity Security(string userSid)
        {
            var security = new PipeSecurity(); security.SetAccessRuleProtection(true, false);
            security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkSid, null), PipeAccessRights.FullControl, AccessControlType.Deny));
            foreach (var sid in new[] { new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null), new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), WindowsIdentity.GetCurrent().User })
                security.AddAccessRule(new PipeAccessRule(sid, PipeAccessRights.FullControl, AccessControlType.Allow));
            if (!string.IsNullOrEmpty(userSid)) security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(userSid), PipeAccessRights.Read, AccessControlType.Allow));
            return security;
        }
        public void Start() { if (loop != null) throw new InvalidOperationException("Monitor already started."); loop = Task.Run(ServeAsync); }
        private async Task ServeAsync()
        {
            while (!stop.IsCancellationRequested)
            {
                using (var pipe = new NamedPipeServerStream(name, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 4096, 65536, Security(userSid)))
                using (stop.Token.Register(() => pipe.Dispose()))
                {
                    try
                    {
                        await pipe.WaitForConnectionAsync(stop.Token).ConfigureAwait(false);
                        var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(statistics(), new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }));
                        if (data.Length > 1024 * 1024) throw new IOException("Monitor snapshot too large.");
                        using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(stop.Token))
                        using (deadline.Token.Register(() => pipe.Dispose()))
                        {
                            deadline.CancelAfter(3000);
                            await pipe.WriteAsync(data, 0, data.Length, deadline.Token).ConfigureAwait(false);
                            await pipe.FlushAsync(deadline.Token).ConfigureAwait(false);
                        }
                    }
                    catch (IOException) { }
                    catch (ObjectDisposedException) { }
                    catch (OperationCanceledException) { }
                }
            }
        }
        public static async Task<JObject> ReadAsync(CancellationToken token, string name = PipeName)
        {
            using (var pipe = new NamedPipeClientStream(".", name, PipeDirection.In, PipeOptions.Asynchronous))
            using (var deadline = CancellationTokenSource.CreateLinkedTokenSource(token))
            using (deadline.Token.Register(() => pipe.Dispose()))
            {
                deadline.CancelAfter(3000);
                try
                {
                    await pipe.ConnectAsync(2000, deadline.Token).ConfigureAwait(false);
                    using (var memory = new MemoryStream())
                    {
                        var buffer = new byte[8192]; int count;
                        while ((count = await pipe.ReadAsync(buffer, 0, buffer.Length, deadline.Token).ConfigureAwait(false)) != 0)
                        { if (memory.Length + count > 1024 * 1024) throw new IOException("Monitor response too large."); memory.Write(buffer, 0, count); }
                        var snapshot = JObject.Parse(Encoding.UTF8.GetString(memory.ToArray()));
                        if ((string)snapshot["refresh"]?["mode"] != "windows-service") throw new IOException("The status endpoint is not a ScrapeX Windows service.");
                        return snapshot;
                    }
                }
                catch (Exception) when (deadline.IsCancellationRequested) { throw new OperationCanceledException("Service monitoring timed out or was cancelled.", deadline.Token); }
            }
        }
        public async Task StopAsync() { stop.Cancel(); if (loop != null) await loop.ConfigureAwait(false); }
        public void Dispose() { stop.Dispose(); }
    }
}
