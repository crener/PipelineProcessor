using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
