using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline;
using PipelineProcessor2.Pipeline.Detectors;

namespace PipelineTests.Pipeline.Detectors
{
    [TestFixture]
    public class SyncBlockDetectionTest
    {
        [Test]
        public void NoSync()
        {
            // Multi
            // S -> Process -> Process -> End

            List<DependentNode> nodes = new List<DependentNode>();
            DependentNode start = new DependentNode(0, "start"),
                pro1 = new DependentNode(2, "pro"),
                pro2 = new DependentNode(3, "pro"),
                end = new DependentNode(4, "end");

            nodes.Add(start);
            nodes.Add(pro1);
            nodes.Add(pro2);
            nodes.Add(end);

            TestHelpers.MatchSlots(start, pro1, 0, 0);
            TestHelpers.MatchSlots(pro1, pro2, 0, 0);
            TestHelpers.MatchSlots(pro2, end, 0, 0);

            DataStore staticData = new DataStore(true);
            SpecialNodeData data =
                SpecialNodeSearch.CheckForSpecialNodes(TestHelpers.ConvertToDictionary(nodes), staticData);

            Assert.AreEqual(0, data.SyncInformation.SyncNodes.Length, "There should not be any sync blocks in this test");
            Assert.AreEqual(null, data.SyncInformation.NodeGroups);
        }

        [Test]
        public void MultiToSingle()
        {
            // Multi            | Single
            // S -> Process -> Sync -> Process -> End

            List<DependentNode> nodes = new List<DependentNode>();
            DependentNode start = new DependentNode(0, "start"),
                sync = new DependentNode(1, SyncNode.TypeName),
                pro1 = new DependentNode(2, "pro"),
                pro2 = new DependentNode(3, "pro"),
                end = new DependentNode(4, "end");

            nodes.Add(start);
            nodes.Add(pro1);
            nodes.Add(sync);
            nodes.Add(pro2);
            nodes.Add(end);

            TestHelpers.MatchSlots(start, pro1, 0, 0);
            TestHelpers.MatchSlots(pro1, sync, 0, 0);
            TestHelpers.MatchSlots(sync, pro2, 0, 0);
            TestHelpers.MatchSlots(pro2, end, 0, 0);

            DataStore staticData = new DataStore(true);
            SpecialNodeData data =
                SpecialNodeSearch.CheckForSpecialNodes(TestHelpers.ConvertToDictionary(nodes), staticData);

            Assert.AreEqual(1, data.SyncInformation.SyncNodes.Length);
            Assert.AreEqual(0, data.SyncInformation.NodeGroups[0].Dependents.Length);

            Assert.AreEqual(1, data.SyncInformation.NodeGroups[1].CalledBy);
        }

        [Test]
        public void MultiToMulti()
        {
            // Multi            | Multi
            // S -> Process -> Sync -> Process -> End
            //         v-----------------ᴧ

            List<DependentNode> nodes = new List<DependentNode>();
            DependentNode start = new DependentNode(0, "start"),
                sync = new DependentNode(1, SyncNode.TypeName),
                pro1 = new DependentNode(2, "pro"),
                pro2 = new DependentNode(3, "pro"),
                end = new DependentNode(4, "end");

            nodes.Add(start);
            nodes.Add(pro1);
            nodes.Add(sync);
            nodes.Add(pro2);
            nodes.Add(end);

            TestHelpers.MatchSlots(start, pro1, 0, 0);
            TestHelpers.MatchSlots(start, pro1, 1, 1);
            TestHelpers.MatchSlots(pro1, sync, 0, 0);
            TestHelpers.MatchSlots(pro1, sync, 1, 1);
            TestHelpers.MatchSlots(pro1, pro2, 1, 1);
            TestHelpers.MatchSlots(sync, pro2, 0, 0);
            TestHelpers.MatchSlots(pro2, end, 0, 0);

            DataStore staticData = new DataStore(true);
            SpecialNodeData data =
                SpecialNodeSearch.CheckForSpecialNodes(TestHelpers.ConvertToDictionary(nodes), staticData);

            Assert.AreEqual(1, data.SyncInformation.SyncNodes.Length);
            Assert.AreEqual(1, data.SyncInformation.NodeGroups[0].Dependents.Length);
            Assert.AreEqual(pro2.Id, data.SyncInformation.NodeGroups[0].Dependents[0]);
            Assert.AreEqual(sync.Id, data.SyncInformation.NodeGroups[1].CalledBy);
        }

        [Test]
        public void MultiToSingleToMulti()
        {
            // Multi            | Single           | Multi
            // S -> Process -> Sync -> Process -> Sync -> Process -> End
            //        v-------------------------------------ᴧ

            List<DependentNode> nodes = new List<DependentNode>();
            DependentNode start = new DependentNode(0, "start"),
                sync = new DependentNode(1, SyncNode.TypeName),
                sync2 = new DependentNode(6, SyncNode.TypeName),
                pro1 = new DependentNode(2, "pro"),
                pro2 = new DependentNode(3, "pro"),
                pro3 = new DependentNode(5, "pro"),
                end = new DependentNode(4, "end");

            nodes.Add(start);
            nodes.Add(pro1);
            nodes.Add(sync);
            nodes.Add(sync2);
            nodes.Add(pro2);
            nodes.Add(pro3);
            nodes.Add(end);

            TestHelpers.MatchSlots(start, pro1, 0, 0);
            TestHelpers.MatchSlots(pro1, sync, 0, 0);
            TestHelpers.MatchSlots(pro1, pro3, 1, 1);
            TestHelpers.MatchSlots(sync, pro2, 0, 0);
            TestHelpers.MatchSlots(pro2, sync2, 0, 0);
            TestHelpers.MatchSlots(sync2, pro3, 0, 0);
            TestHelpers.MatchSlots(pro3, end, 0, 0);

            DataStore staticData = new DataStore(true);
            SpecialNodeData data =
                SpecialNodeSearch.CheckForSpecialNodes(TestHelpers.ConvertToDictionary(nodes), staticData);

            Assert.AreEqual(2, data.SyncInformation.SyncNodes.Length);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups.Count);
            
            Assert.AreEqual(sync.Id, data.SyncInformation.NodeGroups[1].CalledBy);
            Assert.AreEqual(sync2.Id, data.SyncInformation.NodeGroups[2].CalledBy);
            Assert.IsTrue(data.SyncInformation.NodeGroups[0].Dependents.Contains(pro3.Id));
        }
    }
}
