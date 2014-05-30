using ServiceStack;

namespace AspNetServiceStackRole.Dto {
    [Route("/echo/{EchoString}", "GET")]
    public class EchoRequest : IReturn<EchoResponse> {
        public string EchoString { get; set; }
    }

    public class EchoResponse {
        public string EchoString { get; set; }
    }
}
