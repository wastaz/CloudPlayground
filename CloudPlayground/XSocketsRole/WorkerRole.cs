using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using XSockets.Core.Common.Configuration;
using XSockets.Core.Common.Socket;
using XSockets.Core.Configuration;

namespace XSocketsRole {
    public class WorkerRole : RoleEntryPoint {
        public override void Run() {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("XSocketsRole entry point called");

            while (true) {
                Thread.Sleep(10000);
                Trace.TraceInformation("Working");
            }
        }

        public override bool OnStart() {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            var container = XSockets.Plugin.Framework.Composable.GetExport<IXSocketServerContainer>();
            // Create a Custom Configuration based on what we have defined using property pages.
            var myCustomConfig = new List<IConfigurationSetting>();
            
            var internalConfig = new ConfigurationSetting(new Uri(RoleEnvironment.GetConfigurationSettingValue("MyUri"))) {
                Endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints.FirstOrDefault(n => n.Key.Equals("MyEndpoint")).Value.IPEndpoint
            };
            
            var externalConfig = new ConfigurationSetting(new Uri(RoleEnvironment.GetConfigurationSettingValue("MyUri"))) {
                Endpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints.FirstOrDefault(n => n.Key.Equals("MyExternalEndpoint")).Value.IPEndpoint
            };
            myCustomConfig.Add(internalConfig);
            myCustomConfig.Add(externalConfig);

            container.StartServers(false, false, myCustomConfig);

            return base.OnStart();
        }
    }
}
