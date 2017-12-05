using System.IO;
using System.Net;
using Newtonsoft.Json;
using PipelineProcessor2.PluginImporter;

namespace PipelineProcessor2.Server.Responses
{
    internal class UpdateNodeGraph : IResponse
    {
        public UpdateNodeGraph() { }

        public string Response(HttpListenerRequest request)
        {
            string data = "";

            using (StreamReader stream = new StreamReader(request.InputStream))
                data = stream.ReadToEnd();



            return "";
        }

        public string EndpointLocation()
        {
            return "/graph/update";
        }
    }
}
