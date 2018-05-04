using System.Collections.Generic;
using NUnit.Framework;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Nodes.Internal;
using PipelineProcessor2.Pipeline;
using PipelineProcessor2.Plugin;
using PipelineTests.TestNodes;

namespace PipelineTests.Pipeline.PipelineStateTests
{
    [TestFixture]
    public class PipelineStateAdvancedPipelineBuildTest
    {
#if DEBUG

        private const int DataSize = 5;
        private static BuildInputPlugin inputPlugin = new BuildInputPlugin(DataSize);

        [OneTimeSetUp]
        public void Setup()
        {
            PluginStore.ClearAll();
            PluginStore.AddPlugin(inputPlugin);
            PluginStore.Init();
        }

        [OneTimeTearDown]
        public void Finish()
        {
            PluginStore.ClearAll();
        }

        [Test]
        public void SharedLoops()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "in", inputPlugin.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "loopStart", LoopStart.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(2, "pro"));
            nodes.Add(TestHelpers.BuildGraphNode(3, "loopEnd", LoopEnd.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(4, "end", "end"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 2, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[3], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[2], nodes[3], 0, 2));
            links.Add(TestHelpers.MatchSlots(nodes[3], nodes[4], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            PipelineExecutor[] results = PipelineState.BuildPipesTestOnly();

            Assert.AreEqual(DataSize, results.Length);
            Assert.AreEqual(1, results[0].getLoops().Count);

            for (int i = 0; i < results.Length; i++)
                for (int j = i+1; j < results.Length; j++)
                    Assert.AreNotSame(results[i], results[j]);
        }

#endif
    }
}
