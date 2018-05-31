using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline;
using NUnit.Framework;

namespace PipelineTests.Pipeline
{
    [TestFixture]
    public class PipelineHelperTests
    {
        [Test]
        public void SimpleSlotDependents()
        {
            List<DependentNode> nodes = new List<DependentNode>();

            DependentNode a = new DependentNode(0, "a"),
                b = new DependentNode(1, "b"),
                c = new DependentNode(2, "c");

            nodes.Add(a);
            nodes.Add(b);
            nodes.Add(c);

            TestHelpers.MatchSlots(a, b, 0, 0);
            TestHelpers.MatchSlots(a, c, 1, 0);

            NodeSlot resultSlot = ExecutionHelper.FindFirstNodeSlotInDependents(a, TestHelpers.ConvertToDictionary(nodes), 1);

            Assert.AreEqual(c.Id, resultSlot.NodeId);
        }

        [Test]
        public void SimpleSlotDependencies()
        {
            List<DependentNode> nodes = new List<DependentNode>();

            DependentNode a = new DependentNode(0, "a"),
                b = new DependentNode(1, "b"),
                c = new DependentNode(2, "c");

            nodes.Add(a);
            nodes.Add(b);
            nodes.Add(c);

            TestHelpers.MatchSlots(b, a, 0, 0);
            TestHelpers.MatchSlots(b, c, 1, 0);

            NodeSlot resultSlot = ExecutionHelper.FindFirstNodeSlotInDependents(b, TestHelpers.ConvertToDictionary(nodes), 0);

            Assert.AreEqual(a.Id, resultSlot.NodeId);
        }

        [Test]
        public void MultiSlotDependents()
        {
            List<DependentNode> nodes = new List<DependentNode>();

            DependentNode a = new DependentNode(0, "a"),
                b = new DependentNode(1, "b"),
                c = new DependentNode(2, "c"),
                d = new DependentNode(3, "d");

            nodes.Add(a);
            nodes.Add(b);
            nodes.Add(c);
            nodes.Add(d);

            TestHelpers.MatchSlots(a, b, 0, 0);
            TestHelpers.MatchSlots(b, c, 1, 0);
            TestHelpers.MatchSlots(b, d, 1, 0);

            NodeSlot[] resultSlots = ExecutionHelper.FindAllNodeSlotsInDependents(b, TestHelpers.ConvertToDictionary(nodes), 1);

            Assert.AreEqual(2, resultSlots.Length);
            Assert.AreEqual(c.Id, resultSlots[0].NodeId);
            Assert.AreEqual(d.Id, resultSlots[1].NodeId);
        }
    }
}
