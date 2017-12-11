using System.Collections.Generic;
using PipelineProcessor2.JsonTypes;

namespace PipelineProcessor2.Pipeline
{
    public static class PipelineState
    {
        public static GraphNode[] ActiveNodes { get { return nodes; } }
        public static NodeLinkInfo[] ActiveLinks { get { return links; } }

        private static GraphNode[] nodes = new GraphNode[0];
        private static NodeLinkInfo[] links = new NodeLinkInfo[0];

        public static void UpdateActiveGraph(GraphNode[] graphNodes, NodeLinkInfo[] graphLinks)
        {
            links = graphLinks;
            nodes = StripUnusedNodes(graphNodes, graphLinks);
        }

        private static GraphNode[] StripUnusedNodes(GraphNode[] graphNodes, NodeLinkInfo[] graphLinks)
        {
            List<int> usedIds = new List<int>();
            foreach (NodeLinkInfo link in graphLinks)
            {
                if (!usedIds.Contains(link.OriginId)) usedIds.Add(link.OriginId);
                if (!usedIds.Contains(link.TargetId)) usedIds.Add(link.TargetId);
            }

            List<GraphNode> output = new List<GraphNode>((int)(graphNodes.Length * 1.4f));
            foreach (GraphNode node in graphNodes) if (usedIds.Contains(node.id)) output.Add(node);

            return output.ToArray();
        }

        public static void ClearAll()
        {
            nodes = new GraphNode[0];
            links = new NodeLinkInfo[0];
        }
    }
}
