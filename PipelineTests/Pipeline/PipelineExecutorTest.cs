using System;
using System.Collections;
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
        [OneTimeSetUp]
        public void Setup()
        {

        }

        [Test]
        public void BasicGraphValidityTest()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(new GraphNode { id = 0, title = "used",
                outputs = new[] { new GraphLinkReference { Name = "out1", Type = "o" } }
            });
            nodes.Add(new GraphNode { id = 1, title = "used",
                inputs = new[] { new GraphLinkReference { Name = "in1", Type = "o" } },
                outputs = new[] { new GraphLinkReference { Name = "out1", Type = "p" } }
            });
            nodes.Add(new GraphNode { id = 2, title = "used",
                inputs = new[] {
                    new GraphLinkReference { Name = "in1", Type = "p" },
                    new GraphLinkReference { Name = "in1", Type = "p" } },
                outputs = new[]{ new GraphLinkReference { Name = "out2", Type = "q" } }
            });
            nodes.Add(new GraphNode { id = 3, title = "unused",
                inputs = new[] { new GraphLinkReference { Name = "in1", Type = "p" } },
                outputs = new[] { new GraphLinkReference { Name = "out1", Type = "" } }
            });

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
            links.Add(new NodeLinkInfo(8, 0, 0, 3, 1));
            links.Add(new NodeLinkInfo(4, 0, 0, 5, 0));
            links.Add(new NodeLinkInfo(7, 3, 0, 5, 1));
            links.Add(new NodeLinkInfo(6, 5, 0, 7, 0));
            links.Add(new NodeLinkInfo(5, 7, 0, 6, 0));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
            DependencyTest();
        }

        private void DependencyTest()
        {
            Dictionary<int, DependentNode> graph = PipelineState.DependencyGraph;
            IEqualityComparer<DependentNode> comparer = new IdComparer();

            //check all nodes have dependencies set correctly
            foreach (KeyValuePair<int, DependentNode> node in graph)
            {
                bool invalid1 = false, invalid2 = false;

                //check node dependents
                if (node.Value.Dependents.Length > 0)
                {
                    NodeSlot[] dependents = node.Value.Dependents;
                    foreach (NodeSlot depId in dependents)
                    {
                        bool found = false;
                        for (int i = 0; i < graph[depId.NodeId].Dependencies.Length; i++)
                        {
                            if (graph[depId.NodeId].Dependencies[i].NodeId == node.Key)
                            {
                                found = true;
                                break;
                            }
                        }

                        Assert.IsTrue(found);
                    }
                }
                else invalid1 = true;

                //check node dependencies
                if (node.Value.Dependencies.Length > 0)
                {
                    NodeSlot[] dependencies = node.Value.Dependencies;
                    foreach (NodeSlot depId in dependencies)
                    {
                        bool found = false;
                        for (int i = 0; i < graph[depId.NodeId].Dependents.Length; i++)
                        {
                            if (graph[depId.NodeId].Dependents[i].NodeId == node.Key)
                            {
                                found = true;
                                break;
                            }
                        }

                        Assert.IsTrue(found);
                    }
                }
                else invalid2 = true;

                //Nodes in the graph should always be connected to other nodes
                if (invalid1 && invalid2) Assert.Fail("Node " + node.Key + " does not have any dependencies or dependents!");
            }
        }

        private class IdComparer : IEqualityComparer<DependentNode>
        {
            public bool Equals(DependentNode x, DependentNode y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(DependentNode obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
