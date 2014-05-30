using ServiceStack;
using ServiceStackRole.Dto;

namespace ServiceStackRole.Services {
    public class EchoService : Service {
        public EchoResponse Get(EchoRequest request) {
            return request.ConvertTo<EchoResponse>();
        }
    }
}
