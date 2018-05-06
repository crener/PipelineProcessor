using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using PipelineProcessor2.Pipeline;

namespace PipelineTests.Pipeline
{
    [TestFixture]
    public class NodeTests
    {
        [TestCase(new[] { 0, 3, 2, 1 })]
        [TestCase(new[] { 0, 4, 3, 2 })]
        [TestCase(new[] { 0, 6, 3, 2 })]
        [TestCase(new[] { 0, 1, 2, 3 })]
        [TestCase(new[] { 0, 1, 7, 6, 5, 2 })]
        [TestCase(new[] { 55, 3, 12, 23 })]

        public void NodeDependencyOrder(int[] slots)
        {
            DependentNode node = new DependentNode(0, "node");

            foreach (int slot in slots)
                node.AddDependency(slot, 0, slot);

            int lastSlot = -1;
            foreach (NodeSlot dependency in node.Dependencies)
            {
                Assert.Greater(dependency.NodeId, lastSlot);
                lastSlot = dependency.SlotPos;
            }
        }
    }
}
