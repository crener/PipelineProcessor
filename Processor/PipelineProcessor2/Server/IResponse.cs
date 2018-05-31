using System.Net;

namespace PipelineProcessor2.Server
{
    interface IResponse
    {
        string Response(HttpListenerRequest request);
        string EndpointLocation();
    }
}
