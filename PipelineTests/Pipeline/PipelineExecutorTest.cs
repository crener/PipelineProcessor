using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework.Internal;
using NUnit.Framework;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Pipeline;

namespace PipelineTests.Pipeline
{
    [TestFixture]
    public class PipelineExecutorTest
    {
        [Test]
        public void BasicGraphValidityTest()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(new GraphNode { id = 0, title = "used" });
            nodes.Add(new GraphNode { id = 1, title = "used" });
            nodes.Add(new GraphNode { id = 2, title = "used" });
            nodes.Add(new GraphNode { id = 3, title = "unused" });

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(new NodeLinkInfo(0, 0, 0, 1, 0));
            links.Add(new NodeLinkInfo(1, 1, 0, 2, 0));
            links.Add(new NodeLinkInfo(2, 1, 0, 2, 1));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            DependencyTest();
        }

        [Test]
        public void AdvancedGraphValidityTest()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(new GraphNode { id = 6, title = "used" });
            nodes.Add(new GraphNode { id = 3, title = "used" });
            nodes.Add(new GraphNode { id = 5, title = "used" });
            nodes.Add(new GraphNode { id = 1, title = "used" });
            nodes.Add(new GraphNode { id = 2, title = "used" });
            nodes.Add(new GraphNode { id = 0, title = "used" });
            nodes.Add(new GraphNode { id = 4, title = "unused" });
            nodes.Add(new GraphNode { id = 7, title = "used" });

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(new NodeLinkInfo(1, 2, 0, 1, 0));
            links.Add(new NodeLinkInfo(2, 2, 0, 0, 0));
            links.Add(new NodeLinkInfo(0, 1, 0, 3, 0));
            links.Add(new NodeLinkInfo(8, 0, 0, 3, 0));
            links.Add(new NodeLinkInfo(4, 0, 0, 5, 0));
            links.Add(new NodeLinkInfo(7, 3, 0, 5, 0));
            links.Add(new NodeLinkInfo(6, 5, 0, 7, 0));
            links.Add(new NodeLinkInfo(5, 7, 0, 6, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            DependencyTest();
        }

        private void DependencyTest()
        {
            PipelineExecutor testobject = new PipelineExecutor();

            Dictionary<int, DependentNode> graph = testobject.DependencyGraph;

            //check all nodes have dependencies set correctly
            foreach (KeyValuePair<int, DependentNode> node in graph)
            {
                bool valid1 = false, valid2 = false;

                if (node.Value.Dependents.Length > 0)
                {
                    int[] dependents = node.Value.Dependents;
                    foreach (int depId in dependents)
                        Assert.IsTrue(graph[depId].Dependencies.Contains(node.Key));
                }
                else valid1 = true;

                if (node.Value.Dependencies.Length > 0)
                {
                    int[] dependencies = node.Value.Dependencies;
                    foreach (int depId in dependencies)
                        Assert.IsTrue(graph[depId].Dependents.Contains(node.Key));
                }
                else valid2 = true;

                if (valid1 && valid2) Assert.Fail("Node " + node.Key + " does not have any dependencies or dependents!");
            }
        }
    }
}
