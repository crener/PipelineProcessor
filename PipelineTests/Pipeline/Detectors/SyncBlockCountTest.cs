using System.Collections.Generic;
using NUnit.Framework;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline;
using PipelineProcessor2.Pipeline.Detectors;
using PipelineProcessor2.Pipeline.Exceptions;
using PipelineProcessor2.Plugin;
using PipelineTests.TestNodes;

namespace PipelineTests.Pipeline.Detectors
{
    [TestFixture]
    public class SyncBlockCountTest
    {
        private static BuildInputPlugin inputPlugin3 = new BuildInputPlugin(3, "in3");
        private static BuildInputPlugin inputPlugin4 = new BuildInputPlugin(4, "in4");

        [OneTimeSetUp]
        public void Setup()
        {
            PluginStore.Init();
            PluginStore.AddPlugin(inputPlugin3);
            PluginStore.AddPlugin(inputPlugin4);
        }

        [OneTimeTearDown]
        public void Finish()
        {
            PluginStore.ClearAll();
        }

        [Test]
        public void SingleCount()
        {
            // Multi            | Single
            // S -> Process -> Sync -> Process -> End

            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "in", inputPlugin3.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "pro"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "sync", SyncNode.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(3, "pro"));
            nodes.Add(TestHelpers.BuildGraphNode(4, "end"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[2], nodes[3], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[3], nodes[4], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            PipelineExecutor[] executor = PipelineState.BuildPipesTestOnly();
            SpecialNodeData data = PipelineState.SpecialNodeDataTestOnly;

            Assert.AreEqual(2, data.SyncInformation.NodeGroups.Count);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[0].pipes.Length);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[0].RequiredPipes);
            Assert.IsTrue(data.SyncInformation.NodeGroups[0].Input);
            Assert.AreEqual(1, data.SyncInformation.NodeGroups[1].pipes.Length);
            Assert.AreEqual(1, data.SyncInformation.NodeGroups[1].RequiredPipes);
            Assert.IsFalse(data.SyncInformation.NodeGroups[1].Input);
        }

        [Test]
        public void Linked()
        {
            // Multi            | Multi
            // S -> Process -> Sync -> Process -> End
            //         v-----------------ᴧ

            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "in", inputPlugin3.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "pro"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "sync", SyncNode.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(3, "pro"));
            nodes.Add(TestHelpers.BuildGraphNode(4, "end"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[3], 0, 1));
            links.Add(TestHelpers.MatchSlots(nodes[2], nodes[3], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[3], nodes[4], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            PipelineExecutor[] executor = PipelineState.BuildPipesTestOnly();
            SpecialNodeData data = PipelineState.SpecialNodeDataTestOnly;

            Assert.AreEqual(2, data.SyncInformation.NodeGroups.Count);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[0].pipes.Length);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[0].RequiredPipes);
            Assert.IsTrue(data.SyncInformation.NodeGroups[0].Input);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[1].pipes.Length);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[1].RequiredPipes);
            Assert.IsFalse(data.SyncInformation.NodeGroups[1].Input);
        }

        [Test]
        public void MultiToSingleToMulti()
        {
            // Multi            | Single           | Multi
            // S -> Process -> Sync -> Process -> Sync -> Process -> End
            //        v-------------------------------------ᴧ

            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "in", inputPlugin3.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "pro"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "sync", SyncNode.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(3, "pro"));
            nodes.Add(TestHelpers.BuildGraphNode(4, "sync", SyncNode.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(5, "pro"));
            nodes.Add(TestHelpers.BuildGraphNode(6, "end"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[5], 0, 1));
            links.Add(TestHelpers.MatchSlots(nodes[2], nodes[3], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[3], nodes[4], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[4], nodes[5], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[5], nodes[6], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            PipelineExecutor[] executor = PipelineState.BuildPipesTestOnly();
            SpecialNodeData data = PipelineState.SpecialNodeDataTestOnly;

            Assert.AreEqual(3, data.SyncInformation.NodeGroups.Count);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[0].pipes.Length);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[0].RequiredPipes);
            Assert.IsTrue(data.SyncInformation.NodeGroups[0].Input);
            Assert.AreEqual(1, data.SyncInformation.NodeGroups[1].pipes.Length);
            Assert.AreEqual(1, data.SyncInformation.NodeGroups[1].RequiredPipes);
            Assert.IsFalse(data.SyncInformation.NodeGroups[1].Input);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[2].pipes.Length);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[2].RequiredPipes);
            Assert.IsFalse(data.SyncInformation.NodeGroups[2].Input);
        }



        [Test]
        public void Extrainput()
        {
            // Multi            | Multi
            // Input1 -> Process -> Sync -> Process -> End
            //                                 ᴧ
            //                               Input2

            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "in", inputPlugin3.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "pro"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "sync", SyncNode.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(3, "in", inputPlugin4.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(4, "pro"));
            nodes.Add(TestHelpers.BuildGraphNode(5, "end"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[2], nodes[4], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[3], nodes[4], 0, 1));
            links.Add(TestHelpers.MatchSlots(nodes[4], nodes[5], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            PipelineExecutor[] executor = PipelineState.BuildPipesTestOnly();
            SpecialNodeData data = PipelineState.SpecialNodeDataTestOnly;

            Assert.AreEqual(2, data.SyncInformation.NodeGroups.Count);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[0].pipes.Length);
            Assert.AreEqual(3, data.SyncInformation.NodeGroups[0].RequiredPipes);
            Assert.IsTrue(data.SyncInformation.NodeGroups[0].Input);
            Assert.AreEqual(4, data.SyncInformation.NodeGroups[1].pipes.Length);
            Assert.AreEqual(4, data.SyncInformation.NodeGroups[1].RequiredPipes);
            Assert.IsTrue(data.SyncInformation.NodeGroups[1].Input);
        }
    }
}
