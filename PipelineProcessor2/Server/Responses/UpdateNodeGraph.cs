using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PipelineProcessor2.JsonTypes;

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

            JObject information = JObject.Parse(data);

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            {
                Dictionary<string, NodeLinkInfo> dic = JsonConvert.DeserializeObject<Dictionary<string, NodeLinkInfo>>(information["links"].ToString());
                links.AddRange(dic.Values);
            }


            return "";
        }

        public string EndpointLocation()
        {
            return "/graph/update";
        }
    }
}
