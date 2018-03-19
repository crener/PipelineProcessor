using System;
using System.Collections.Generic;
using NUnit.Framework;
using PipelineProcessor2;
using PipelineProcessor2.JsonTypes;
using PipelineProcessor2.Pipeline;
using PipelineProcessor2.Pipeline.Exceptions;

namespace PipelineTests.Pipeline
{
    [TestFixture]
    public class PipelineStateTest
    {
        [TearDown]
        public void CleanUp()
        {
            PipelineState.ClearAll();
        }

        [Test]
        public void Simple()
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

            nodes.RemoveAt(3); //remove unused

            GraphNode[] result = PipelineState.ActiveNodes;
            Assert.AreEqual(nodes.ToArray(), result);
            Assert.AreEqual(links.ToArray(), PipelineState.ActiveLinks);
        }

        [Test]
        public void OneLinkToMany()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(new GraphNode { id = 0, title = "used" });
            nodes.Add(new GraphNode { id = 1, title = "used" });
            nodes.Add(new GraphNode { id = 2, title = "used" });
            nodes.Add(new GraphNode { id = 3, title = "used" });
            nodes.Add(new GraphNode { id = 4, title = "used" });
            nodes.Add(new GraphNode { id = 5, title = "used" });
            nodes.Add(new GraphNode { id = 6, title = "used" });

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(new NodeLinkInfo(0, 0, 0, 1, 0));
            links.Add(new NodeLinkInfo(0, 0, 0, 2, 0));
            links.Add(new NodeLinkInfo(0, 0, 0, 3, 1));
            links.Add(new NodeLinkInfo(0, 0, 0, 4, 1));
            links.Add(new NodeLinkInfo(0, 0, 0, 5, 1));
            links.Add(new NodeLinkInfo(0, 0, 0, 6, 1));

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());

            GraphNode[] result = PipelineState.ActiveNodes;
            Assert.AreEqual(nodes.ToArray(), result);
            Assert.AreEqual(links.ToArray(), PipelineState.ActiveLinks);
        }

        [Test]
        public void UnorderedIds()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(new GraphNode { id = 6, title = "used" });
            nodes.Add(new GraphNode { id = 3, title = "used" });
            nodes.Add(new GraphNode { id = 5, title = "used" });
            nodes.Add(new GraphNode { id = 1, title = "used" });
            nodes.Add(new GraphNode { id = 2, title = "used" });
            nodes.Add(new GraphNode { id = 7, title = "used" });
            nodes.Add(new GraphNode { id = 0, title = "used" });
            nodes.Add(new GraphNode { id = 4, title = "unused" });

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

            nodes.RemoveAt(nodes.Count-1);

            GraphNode[] result = PipelineState.ActiveNodes;
            Assert.AreEqual(nodes.ToArray(), result);
            Assert.AreEqual(links.ToArray(), PipelineState.ActiveLinks);
        }

        [Test]
        public void UnreferencedNode()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            nodes.Add(new GraphNode { id = 6, title = "used" });
            nodes.Add(new GraphNode { id = 3, title = "used" });
            nodes.Add(new GraphNode { id = 5, title = "used" });
            nodes.Add(new GraphNode { id = 1, title = "used" });
            nodes.Add(new GraphNode { id = 2, title = "used" });
            nodes.Add(new GraphNode { id = 0, title = "used" });
            nodes.Add(new GraphNode { id = 4, title = "unused" });

            List<NodeLinkInfo> links = new List<NodeLinkInfo>();
            links.Add(new NodeLinkInfo(1, 2, 0, 1, 0));
            links.Add(new NodeLinkInfo(2, 2, 0, 0, 0));
            links.Add(new NodeLinkInfo(0, 1, 0, 3, 0));
            links.Add(new NodeLinkInfo(8, 0, 0, 3, 0));
            links.Add(new NodeLinkInfo(4, 0, 0, 5, 0));
            links.Add(new NodeLinkInfo(7, 3, 0, 5, 0));
            links.Add(new NodeLinkInfo(6, 5, 0, 7, 0));
            links.Add(new NodeLinkInfo(5, 7, 0, 6, 0));

            //nodes[7] does not exist and should throw an exception as it is referenced in the links

            try
            {
                PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());
                Assert.Fail("GraphNode id 7 doesn't exist, therefor the links are referencing a node that doesn't exist. this should throw an exception");
            }
            catch(Exception ex)
            {
                Assert.IsTrue(ex.GetType() == typeof(MissingNodeException));
            }
        }


        [Test]
        public void NoNodes()
        {
            List<GraphNode> nodes = new List<GraphNode>();
            List<NodeLinkInfo> links = new List<NodeLinkInfo>();

            PipelineState.UpdateActiveGraph(nodes.ToArray(), links.ToArray());

            GraphNode[] result = PipelineState.ActiveNodes;
            Assert.AreEqual(nodes.ToArray(), result);
            Assert.AreEqual(links.ToArray(), PipelineState.ActiveLinks);
        }
    }
}
