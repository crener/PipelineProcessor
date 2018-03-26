using System.Collections.Generic;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Pipeline;

namespace PipelineTests.Pipeline
{
    public static class TestHelpers
    {
        public static Dictionary<int, DependentNode> ConvertToDictionary(List<DependentNode> deps)
        {
            Dictionary<int, DependentNode> dependent = new Dictionary<int, DependentNode>();

            foreach (DependentNode dep in deps)
                dependent.Add(dep.Id, dep);

            return dependent;
        }

        public static void MatchSlots(DependentNode a, DependentNode b, int aSlot, int bSlot)
        {
            a.AddDependent(b.Id, bSlot, aSlot);
            b.AddDependency(a.Id, aSlot, bSlot);
        }

        public static GraphNode BuildGraphNode(int id, string title, string type = "")
        {
            return new GraphNode { id = id, title = title, type = type };
        }

        public static NodeLinkInfo MatchSlots(GraphNode a, GraphNode b, int aSlot, int bSlot)
        {
            return new NodeLinkInfo(0, a.id, aSlot, b.id, bSlot);
        }
    }
}
