using Newtonsoft.Json;

namespace PipelineProcessor2.JsonTypes
{
    public class GraphNode
    {
        public int id, mode;
        public string title, type;
        private int[] pos; 

        public GraphLinkReference[] inputs;
        public GraphLinkReference[] outputs;

        public GraphNode(int id, int mode, string title, string type, int[] pos, GraphLinkReference[] inputs, GraphLinkReference[] outputs)
        {
            this.id = id;
            this.mode = mode;
            this.title = title;
            this.type = type;
            this.pos = pos;
            this.inputs = inputs;
            this.outputs = outputs;
        }

        public GraphNode()
        {
            id = 0;
            mode = 0;
            title = "";
            type = "";
            pos = new []{0,0};
            inputs = new GraphLinkReference[0];
            outputs = new GraphLinkReference[0];
        }
    }

    public struct GraphLinkReference
    {
        [JsonProperty("links")]
        public string Name;
        [JsonProperty("links")]
        public string Type;
        [JsonProperty("links")]
        public int[] LinkIds;
    }
}
