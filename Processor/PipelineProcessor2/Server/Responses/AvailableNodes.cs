using System.Net;
using Newtonsoft.Json;
using PipelineProcessor2.Plugin;

namespace PipelineProcessor2.Server.Responses
{
    internal class AvailableNodes : IResponse
    {
        public AvailableNodes() { }

        public string Response(HttpListenerRequest request)
        {
            return JsonConvert.SerializeObject(PluginStore.AvailableNodes());
        }

        public string EndpointLocation()
        {
            return "/nodes";
        }
    }
}
