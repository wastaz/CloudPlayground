using AspNetServiceStackRole.Dto;
using ServiceStack;


namespace AspNetServiceStackRole.Services {
    public class EchoService : Service {
        private readonly XSocketsWrapper socketClient;

        public EchoService(XSocketsWrapper socketClient) {
            this.socketClient = socketClient;
        }

        public EchoResponse Get(EchoRequest request) {
            socketClient.Send("I am an event (" + socketClient.NumberOfFooMessages + "): " + request.EchoString, "foo");
            return request.ConvertTo<EchoResponse>();
        }
    }
}
