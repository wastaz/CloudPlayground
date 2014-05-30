using ServiceStack;

namespace ServiceStackRole.Dto {
    [Route("/echo/{EchoString}")]
    public class EchoRequest : IReturn<EchoResponse> {
        public string EchoString { get; set; }
    }

    public class EchoResponse {
        public string EchoString { get; set; }
    }
}
