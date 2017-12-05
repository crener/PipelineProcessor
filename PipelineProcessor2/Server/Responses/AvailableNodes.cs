using Newtonsoft.Json;
using PipelineProcessor2.PluginImporter;

namespace PipelineProcessor2.Server.Responses
{
    internal class AvailableNodes : IResponse
    {
        public AvailableNodes() { }

        public string Response()
        {
            return JsonConvert.SerializeObject(PluginStore.AvailableNodes());
        }

        public string EndpointLocation()
        {
            return "/nodes";
        }
    }
}
