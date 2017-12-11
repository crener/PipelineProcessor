using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.JsonTypes;

namespace PipelineProcessor2.Pipeline
{
    public class PipelineExecutor
    {
        public Dictionary<int, DependentNode> DependencyGraph => dependencyGraph;


        private Dictionary<int, DependentNode> dependencyGraph;

        public PipelineExecutor()
        {
            BuildDependencyGraph(PipelineState.ActiveNodes, PipelineState.ActiveLinks);
        }

        public PipelineExecutor(GraphNode[] nodes, NodeLinkInfo[] links)
        {
            BuildDependencyGraph(nodes, links);
        }

        private void BuildDependencyGraph(GraphNode[] nodes, NodeLinkInfo[] links)
        {
            dependencyGraph = new Dictionary<int, DependentNode>();
            foreach (GraphNode node in nodes)
                if (!DependencyGraph.ContainsKey(node.id))
                    DependencyGraph.Add(node.id, new DependentNode(node.id));

            foreach (NodeLinkInfo info in links)
            {
                DependencyGraph[info.OriginId].AddDependent(info.TargetId);
                DependencyGraph[info.TargetId].AddDependency(info.TargetId);
            }
        }

    }

    public class DependentNode
    {
        /// <summary>
        /// Nodes which require the output of this node to start
        /// </summary>
        public int[] Dependents => dependents.ToArray();

        /// <summary>
        /// Nodes which this node requires to start
        /// </summary>
        public int[] Dependencies => dependencies.ToArray();
        public int Id { get; private set; }

        private List<int> dependents, dependencies;

        public DependentNode(int id)
        {
            Id = id;
            dependents = new List<int>();
            dependencies = new List<int>();
        }

        public void AddDependency(int id)
        {
            if (dependencies.Contains(id)) dependencies.Add(id);
        }

        public void AddDependent(int id)
        {
            if (dependents.Contains(id)) dependents.Add(id);
        }
    }
}
