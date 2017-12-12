using Newtonsoft.Json;

namespace PipelineProcessor2.JsonTypes
{
    public class GraphNode
    {
        public int id;
        public string title, type;

        public GraphLinkReference[] inputs;
        public GraphLinkReference[] outputs;

        public GraphNode(int id, string title, string type, GraphLinkReference[] inputs, GraphLinkReference[] outputs)
        {
            this.id = id;
            this.title = title;
            this.type = type;
            this.inputs = inputs;
            this.outputs = outputs;
        }

        public GraphNode()
        {
            id = 0;
            title = "";
            type = "";
            inputs = new GraphLinkReference[0];
            outputs = new GraphLinkReference[0];
        }
    }

    public struct GraphLinkReference
    {
        [JsonProperty("name")]
        public string Name;
        [JsonProperty("type")]
        public string Type;
        [JsonProperty("links")]
        public int[] LinkIds;
    }
}
