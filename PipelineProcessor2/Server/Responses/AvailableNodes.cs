using System.Collections.Generic;
using Newtonsoft.Json;

namespace PipelineProcessor2.Server.Responses
{
    internal class AvailableNodes : IResponse
    {
        public AvailableNodes()
        {

        }

        public string Response()
        {
            List<Node> nodes = new List<Node>();

            {
                Node tempNode = new Node("Node from C#", "TestNode1", "C#");
                tempNode.menuName = "Test";
                nodes.Add(tempNode);
            }

            {
                Node tempNode = new Node("Other C# option", "TestNode2", "C#");
                nodes.Add(tempNode);
            }

            return JsonConvert.SerializeObject(nodes);
        }

        public string EndpointLocation()
        {
            return "/nodes";
        }
    }
}
