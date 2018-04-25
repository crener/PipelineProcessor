using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Pipeline;

namespace PipelineProcessor2.Server.Responses
{
    internal class UpdateNodeGraph : IResponse
    {
        public UpdateNodeGraph() { }

        public string Response(HttpListenerRequest request)
        {
            Console.WriteLine(DateTime.UtcNow.ToString("mm:ss:fff") + " Starting");
            JObject information;
            {   //Ensures that "data" goes out of scope to save a potentially large amount of memory for duration of request
                string data = "";
                using(StreamReader stream = new StreamReader(request.InputStream))
                    data = stream.ReadToEnd();
                information = JObject.Parse(data);
            }

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            {
                Dictionary<string, NodeLinkInfo> dic = JsonConvert.DeserializeObject<Dictionary<string, NodeLinkInfo>>(information["links"].ToString());
                links.AddRange(dic.Values);
            }

            string input = information["input"].ToObject<string>();
            PipelineState.InputDirectory = input;
            string output = information["output"].ToObject<string>();
            PipelineState.OutputDirectory = output;

            GraphNode[] nodes =
                JsonConvert.DeserializeObject<GraphNode[]>(information["nodes"].ToString());

            PipelineState.UpdateActiveGraph(nodes, links.ToArray());
            PipelineState.Start();

            return "";
        }

        public string EndpointLocation()
        {
            return "/graph/update";
        }
    }
}
