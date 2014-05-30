using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace AspNetServiceStackRole {
    public class MvcApplication : System.Web.HttpApplication {
        protected void Application_Start() {
            EnableDiagnosticTraceLoggingForComputeEmulator();
            new AppHost().Init();
        }

        [Conditional("DEBUG")] // doc on the Conditional attribute: http://msdn.microsoft.com/en-us/library/system.diagnostics.conditionalattribute.aspx
        void EnableDiagnosticTraceLoggingForComputeEmulator() {
            try {
                if (RoleEnvironment.IsAvailable && RoleEnvironment.IsEmulated) {
                    const string className = "Microsoft.ServiceHosting.Tools.DevelopmentFabric.Runtime.DevelopmentFabricTraceListener";

                    if (Trace.Listeners.Cast<TraceListener>().Any(tl => tl.GetType().FullName == className)) {
                        Trace.TraceWarning("Skipping attempt to add second instance of {0}.", className);
                        return;
                    }

                    const string assemblyName = "Microsoft.ServiceHosting.Tools.DevelopmentFabric.Runtime, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
                    //var path = Assembly.ReflectionOnlyLoad(assemblyName).Location;
                    //Assembly assembly = Assembly.LoadFile(path);
                    var assembly = Assembly.LoadFile(assemblyName);
                    var computeEmulatorTraceListenerType = assembly.GetType(className);
                    var computeEmulatorTraceListener = (TraceListener)Activator.CreateInstance(computeEmulatorTraceListenerType);
                    System.Diagnostics.Trace.Listeners.Add(computeEmulatorTraceListener);
                    Trace.TraceInformation("Diagnostic Trace statements will now appear in Compute Emulator: {0} added.", className);
                }
            } catch (Exception) {
                // eat any exceptions since this method offers a No-throw Guarantee
                // http://en.wikipedia.org/wiki/Exception_guarantees
            }
        }
    }
}
