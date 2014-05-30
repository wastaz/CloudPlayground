using System.Linq;
using AspNetServiceStackRole.Dto;
using Raven.Client;
using ServiceStack;


namespace AspNetServiceStackRole.Services {
    public class EchoService : Service {
        private readonly XSocketsWrapper socketClient;
        private readonly RavenWrapper raven;

        public EchoService(XSocketsWrapper socketClient, RavenWrapper ravenWrapper) {
            this.socketClient = socketClient;
            this.raven = ravenWrapper;
        }

        public EchoResponse Get(EchoRequest request) {
            int total = -1;
            if (raven.DocStore != null) {
                using(var session = raven.DocStore.OpenSession()) {
                    session.Store(request);
                    session.SaveChanges();

                    RavenQueryStatistics stats;
                    session.Query<EchoRequest>()
                        .Statistics(out stats)
                        .ToArray();
                    total = stats.TotalResults;
                }
            }

            socketClient.Send("I am an event (" + socketClient.NumberOfFooMessages + " total: " + total + "): " + request.EchoString, "foo");
            
            return request.ConvertTo<EchoResponse>();
        }
    }

    
}
