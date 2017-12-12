using System.Collections.Generic;
using System.Linq;
using PipelineProcessor2.JsonTypes;

namespace PipelineProcessor2.Pipeline
{
    public static class PipelineState
    {
        public static GraphNode[] ActiveNodes { get { return nodes; } }
        public static NodeLinkInfo[] ActiveLinks { get { return links; } }
        public static PipelineExecutor PipelineExecutor { get; private set; }

        private static GraphNode[] nodes = new GraphNode[0];
        private static NodeLinkInfo[] links = new NodeLinkInfo[0];

        public static void UpdateActiveGraph(GraphNode[] graphNodes, NodeLinkInfo[] graphLinks)
        {
            links = graphLinks;
            nodes = StripUnusedNodes(graphNodes, graphLinks);

            PipelineExecutor = new PipelineExecutor();
        }

        private static GraphNode[] StripUnusedNodes(GraphNode[] graphNodes, NodeLinkInfo[] graphLinks)
        {
            List<int> usedIds = new List<int>();
            foreach (NodeLinkInfo link in graphLinks)
            {
                if (!usedIds.Contains(link.OriginId)) usedIds.Add(link.OriginId);
                if (!usedIds.Contains(link.TargetId)) usedIds.Add(link.TargetId);
            }

            Dictionary<int, GraphNode> output = new Dictionary<int, GraphNode>((int)(graphNodes.Length * 1.4f));
            foreach (GraphNode node in graphNodes) if (usedIds.Contains(node.id)) output.Add(node.id, node);

            //ensure all links connections have been satisfied
            foreach (int id in usedIds)
                if (!output.ContainsKey(id))
                    throw new MissingNodeException(id + " is used by a link but does not have a node defined!");

            return output.Values.ToArray();
        }

        public static void ClearAll()
        {
            nodes = new GraphNode[0];
            links = new NodeLinkInfo[0];
        }
    }
}
