using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using AspNetServiceStackRole.TvRageApi;
using Funq;
using Microsoft.WindowsAzure.ServiceRuntime;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using ServiceStack;
using ServiceStack.Mvc;
using XSockets.Client40;

namespace AspNetServiceStackRole {
    public class AppHost : AppHostBase {

        public AppHost() : base("MVC 4", typeof(AppHost).Assembly) { }

        public override void Configure(Container container) {
            container.Register<TvRageApiHandler>(new TvRageApiHandler());
            SetupXSockets(container);
            SetupRaven(container);

            Plugins.Add(new CorsFeature());
            GlobalRequestFilters.Add((req, res, dto) => {
                                         if(req.Verb == "OPTIONS") {
                                             res.AddHeader("Access-Control-Allow-Methods", "GET, OPTIONS");
                                             res.AddHeader("Access-Control-Allow-Headers", "X-Requested-With, Content-Type");
                                             res.EndRequest();
                                         }
                                     });
            
            ControllerBuilder.Current.SetControllerFactory(new FunqControllerFactory(container));
        }

        private void SetupRaven(Container container) {
            container.Register(new RavenWrapper());
            container.Register<IDocumentStore>(container.Resolve<RavenWrapper>().DocStore);
            IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), container.Resolve<IDocumentStore>());
        }

        private void SetupXSockets(Container container) {
            container.Register(new XSocketsWrapper());
        }
    }

    public class RavenWrapper {
        public RavenWrapper() {
            TryConnect();
        }

        private void TryConnect() {
            try {
                DocStore = new DocumentStore {ConnectionStringName = "ravendb"};
                DocStore.Initialize();
                DocStore.DatabaseCommands.GetBuildNumber();
            } catch {
                DocStore = null;
                // Eat exceptions
            }
        }

        public DocumentStore DocStore { get; private set; }
    }

    public class XSocketsWrapper {
        private const string XSocketsRoleName = "XSocketsRole";
        private const string XSocketsEndpointName = "MyEndpoint";
        private XSocketClient client = null;
        private Task retryTask = null;
        private readonly List<Tuple<object, string>> queuedEvents = new List<Tuple<object, string>>();

        public int NumberOfFooMessages { get; private set; }
        public XSocketsWrapper() {
            TryConnect();
        }

        public bool IsConnected {
            get { return client != null && client.IsConnected && client.IsHandshakeDone; }
        }

        private void TryConnect() {
            if(IsConnected || !RoleEnvironment.Roles.ContainsKey(XSocketsRoleName)) {
                client = null;
                return;
            }
            
            var xsocketsRole = RoleEnvironment.Roles[XSocketsRoleName];
            lock(this) {
                if(xsocketsRole.Instances.Count == 0 && retryTask == null) {
                    retryTask = Task.Run(() => {
                                             Thread.Sleep(2000);
                                             retryTask = null;
                                             TryConnect();
                                         });
                    return;
                }
                
                if(client == null) {
                    var roleInstance = xsocketsRole.Instances.First();
                    client = new XSocketClient(
                        string.Format("ws://{0}:{1}/Generic", 
                            roleInstance.InstanceEndpoints[XSocketsEndpointName].IPEndpoint.Address,
                            roleInstance.InstanceEndpoints[XSocketsEndpointName].IPEndpoint.Port), "*");
                    client.Open();
                    client.OnOpen += (sender, eventArgs) => {
                                         Trace.TraceInformation("XSockets connection OPEN");
                                         queuedEvents.ForEach(t => client.Send(t.Item1, t.Item2));
                                         queuedEvents.Clear();
                                     };
                    client.Bind("foo",
                        message => {
                            ++NumberOfFooMessages;
                            Trace.TraceInformation("XSockets data: " + message.data);
                        });
                }
            }
        }

        public void Send(object o, string evt) {
            if(IsConnected) {
                client.Send(o, evt);
            } else {
                queuedEvents.Add(new Tuple<object, string>(o, evt));
                TryConnect();
            }
        }
    }
}