using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using NUnit.Framework;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline;
using PipelineProcessor2.Pipeline.Detectors;
using PipelineProcessor2.Pipeline.Exceptions;
using PipelineProcessor2.Plugin;
using PipelineTests.TestNodes;

namespace PipelineTests.Pipeline
{
    [TestFixture]
    public class PipelineStateSyncPipelineBuildTest
    {
#if DEBUG

        private const int DataSize = 5;
        private static BuildInputPlugin inputPlugin = new BuildInputPlugin(DataSize),
            inputPlugin2 = new BuildInputPlugin(DataSize * 2, "TestInput2");
        private static PrematureEndPlugin
            EarlyEnd = new PrematureEndPlugin(DataSize, DataSize / 2, "LowShow"),
            LateEnd = new PrematureEndPlugin(DataSize, DataSize * 3, "HighShow"),
            EarlyData = new PrematureEndPlugin(DataSize / 2, DataSize, "LowData"),
            LateData = new PrematureEndPlugin(DataSize * 23, DataSize, "HighData");
        private static ErrorInputPlugin ErrorPlugin = new ErrorInputPlugin(DataSize, "ErrorPlugin");

        [OneTimeSetUp]
        public void Setup()
        {
            PluginStore.Init();
            PluginStore.AddPlugin(inputPlugin);
            PluginStore.AddPlugin(inputPlugin2);
            PluginStore.AddPlugin(EarlyEnd);
            PluginStore.AddPlugin(LateEnd);
            PluginStore.AddPlugin(LateData);
            PluginStore.AddPlugin(EarlyData);
            PluginStore.AddPlugin(ErrorPlugin);
        }

        [TearDown]
        public void CleanUp()
        {
            PipelineState.ClearAll();
        }

        [Test]
        public void ManyToSingle()
        {
            // Multi            | Single
            // S -> Process -> Sync -> Process -> End

            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "input", inputPlugin.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "sync", SyncNode.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(3, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(4, "output"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[2], nodes[3], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[3], nodes[4], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            PipelineExecutor[] results = PipelineState.BuildPipesTestOnly();

            Assert.AreEqual(DataSize, results.Length + 1);
        }

        [Test]
        public void ManyToMore()
        {
            // Multi            | Many
            // S -> Process -> Sync -> Process -> End
            //                  Input ----ᴧ

            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "input", inputPlugin.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "sync", SyncNode.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(3, "input", inputPlugin2.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(4, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(5, "output"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[2], nodes[4], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[3], nodes[4], 0, 1));
            links.Add(TestHelpers.MatchSlots(nodes[4], nodes[5], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            PipelineExecutor[] results = PipelineState.BuildPipesTestOnly();

            Assert.AreEqual(DataSize + (DataSize * 2), results.Length);
        }

        [Test]
        public void ManyToMany()
        {
            // Multi            | Multi
            // S -> Process -> Sync -> Process -> End
            //         v-----------------ᴧ

            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "input", inputPlugin.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "sync", SyncNode.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(3, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(4, "output"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[3], 0, 1));
            links.Add(TestHelpers.MatchSlots(nodes[2], nodes[3], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[3], nodes[4], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            PipelineExecutor[] results = PipelineState.BuildPipesTestOnly();

            Assert.AreEqual(DataSize, results.Length);
        }

        [Test]
        public void ManyToManyWithInput()
        {
            // Multi            | Multi
            //         ᴧ------------------v
            // S -> Process -> Sync -> Process -> End
            //                  Input ----ᴧ

            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "input", inputPlugin.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "sync", SyncNode.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(3, "input", inputPlugin2.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(4, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(5, "output"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[4], 0, 2));
            links.Add(TestHelpers.MatchSlots(nodes[2], nodes[4], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[3], nodes[4], 0, 1));
            links.Add(TestHelpers.MatchSlots(nodes[4], nodes[5], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            Assert.Throws<PipelineException>(() => PipelineState.BuildPipesTestOnly());
        }

        private class BuildInputPlugin : TestInput
        {
            private int size;

            public BuildInputPlugin(int resultSize)
            {
                size = resultSize;
            }
            public BuildInputPlugin(int resultSize, string name) : base(name)
            {
                size = resultSize;
            }

            public override IEnumerable<List<byte[]>> RetrieveData(string path)
            {
                for (int i = 0; i < size; i++)
                {
                    yield return new List<byte[]>();
                }
            }

            public override int InputDataQuantity(string path)
            {
                return size;
            }
        }

        private class PrematureEndPlugin : TestInput
        {
            private int size, displaySize;

            public PrematureEndPlugin(int resultSize, int advertizedSize, string name) : base(name)
            {
                size = resultSize;
                displaySize = advertizedSize;
            }

            public override IEnumerable<List<byte[]>> RetrieveData(string path)
            {
                for (int i = 0; i < size; i++)
                {
                    yield return new List<byte[]>();
                }
            }

            public override int InputDataQuantity(string path)
            {
                return displaySize;
            }
        }

        private class ErrorInputPlugin : TestInput
        {
            private int size;

            public ErrorInputPlugin(int resultSize, string name) : base(name)
            {
                size = resultSize;
            }

            public override IEnumerable<List<byte[]>> RetrieveData(string path)
            {
                return null;
            }

            public override int InputDataQuantity(string path)
            {
                return size;
            }
        }

#endif
    }
}
