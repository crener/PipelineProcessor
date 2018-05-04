using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Pipeline;
using PipelineProcessor2.Pipeline.Detectors;
using PipelineProcessor2.Pipeline.Exceptions;
using PipelineProcessor2.Plugin;
using PipelineTests.TestNodes;

namespace PipelineTests.Pipeline
{
    [TestFixture]
    public class PipelineDirectoryOverrideTests
    {
#if DEBUG
        private static InputPathPlugin oneInput = new InputPathPlugin(1, "One");
        private static OutputPathPlugin output = new OutputPathPlugin("Output");

        private static string InputDir = "StandardIn";
        private static string OutputDir = "StandardOut";

        [OneTimeSetUp]
        public void Setup()
        {
            PluginStore.AddPlugin(oneInput);

            PipelineState.InputDirectory = InputDir;
            PipelineState.OutputDirectory = OutputDir;
        }

        [OneTimeTearDown]
        public void Finish()
        {
            PluginStore.ClearAll();
        }

        [OneTimeTearDown]
        public void Clear()
        {
            PluginStore.ClearAll();

            PipelineState.InputDirectory = PipelineState.OutputDirectory = "";
        }

        [TearDown]
        public void CleanUp()
        {
            PipelineState.ClearAll();

            oneInput.InputDataQuantityPath = oneInput.InputRetrieveDataPath = "";
            output.OutputDir = "";
        }

        [Test]
        public void StandardInput()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "input", oneInput.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "output"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            PipelineExecutor[] results = PipelineState.BuildPipesTestOnly();

            Assert.AreEqual(1, results.Length);
            Assert.AreEqual(InputDir, oneInput.InputDataQuantityPath);
            Assert.AreEqual(InputDir, oneInput.InputRetrieveDataPath);
        }

        [Test]
        public void OverrideInput()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "input", oneInput.TypeName, "Override"));
            nodes.Add(TestHelpers.BuildGraphNode(1, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "output"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            PipelineExecutor[] results = PipelineState.BuildPipesTestOnly();

            Assert.AreEqual(1, results.Length);
            Assert.AreEqual("Override", oneInput.InputDataQuantityPath);
            Assert.AreEqual("Override", oneInput.InputRetrieveDataPath);
        }

        [Test]
        public void StandardOutput()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "output", output.TypeName));
            nodes.Add(TestHelpers.BuildGraphNode(1, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "output"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));

            DataStore staticStore = new DataStore(true), standardData = new DataStore(0, "");
            List<DependentNode> dependents = TestHelpers.ConvertToDependentNodes(nodes);
            Dictionary<int, DependentNode> dependentNodes = TestHelpers.ConvertToDictionary(dependents);
            SpecialNodeData special = SpecialNodeSearch.CheckForSpecialNodes(dependentNodes, staticStore);

            PipelineExecutor pipeline = new PipelineExecutor(dependentNodes, staticStore, 0, special);
            TaskRunner runner = new TaskRunner(output, dependents[2], standardData, staticStore, pipeline, 0);

            Task task = runner.getTask();
            task.Start();
            task.Wait();

            Assert.AreEqual(OutputDir, output.OutputDir);
        }

        [Test]
        public void OverrideOutput()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(TestHelpers.BuildGraphNode(0, "input"));
            nodes.Add(TestHelpers.BuildGraphNode(1, "process"));
            nodes.Add(TestHelpers.BuildGraphNode(2, "output", output.TypeName, "Override"));

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(TestHelpers.MatchSlots(nodes[0], nodes[1], 0, 0));
            links.Add(TestHelpers.MatchSlots(nodes[1], nodes[2], 0, 0));

            DataStore staticStore = new DataStore(true), standardData = new DataStore(0, "");
            List<DependentNode> dependents = TestHelpers.ConvertToDependentNodes(nodes);
            Dictionary<int, DependentNode> dependentNodes = TestHelpers.ConvertToDictionary(dependents);
            SpecialNodeData special = SpecialNodeSearch.CheckForSpecialNodes(dependentNodes, staticStore);

            PipelineExecutor pipeline = new PipelineExecutor(dependentNodes, staticStore, 0, special);
            TaskRunner runner = new TaskRunner(output, dependents[2], standardData, staticStore, pipeline, 0);

            Task task = runner.getTask();
            task.Start();
            task.Wait();

            Assert.AreEqual("Override", output.OutputDir);
        }
#endif
    }
}
