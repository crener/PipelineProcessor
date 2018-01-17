using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PipelineProcessor2.Pipeline.Exceptions;

namespace PipelineProcessor2.Pipeline
{
    public class DependentNode
    {
        /// <summary>
        /// Nodes which require the output of this node to start
        /// </summary>
        public NodeSlot[] Dependents
        {
            get
            {
                NodeSlot[] deps = new NodeSlot[totalDependents];

                int i = 0;
                foreach (KeyValuePair<int, List<NodeSlot>> slot in dependents)
                foreach (NodeSlot nodeSlot in slot.Value)
                {
                    deps[i] = nodeSlot;
                    i++;
                }

                return deps;
            }
        }

        /// <summary>
        /// Nodes which this node requires to start
        /// </summary>
        public NodeSlot[] Dependencies
        {
            get
            {
                return dependencies.Values.ToArray();
            }
        }

        public int Id { get; private set; }
        public string Type { get; private set; }

        //private List<NodeSlot> dependents, dependencies;
        private Dictionary<int, NodeSlot> dependencies;
        private Dictionary<int, List<NodeSlot>> dependents;
        private int totalDependents = 0;

        public DependentNode(int id, string type)
        {
            Id = id;
            Type = type;

            dependents = new Dictionary<int, List<NodeSlot>>();
            dependencies = new Dictionary<int, NodeSlot>();
        }

        public void AddDependency(int originId, int originSlot, int targetSlot)
        {
            NodeSlot nodeSlot = new NodeSlot(originId, originSlot);
            if (dependencies.ContainsKey(targetSlot))
                throw new DataSlotAlreadyInUse("Slot " + targetSlot + " of node " + Id + " has already been assigned");

            dependencies.Add(targetSlot, nodeSlot);
        }

        public void AddDependent(int targetId, int targetSlot, int originSlot)
        {
            NodeSlot nodeSlot = new NodeSlot(targetId, targetSlot);

            if (!dependents.ContainsKey(originSlot)) dependents.Add(originSlot, new List<NodeSlot>());
            if (dependents[originSlot].Contains(nodeSlot))
            {
                //clean up unused slot
                if (dependents[originSlot].Count == 0) dependents.Remove(originSlot);

                throw new DataSlotAlreadyInUse("Slot " + targetSlot + " of node " + Id + " has already been assigned");
            }

            dependents[originSlot].Add(nodeSlot);
            totalDependents++;
        }
    }
}
