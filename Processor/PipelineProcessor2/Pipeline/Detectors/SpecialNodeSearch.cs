using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Pipeline.Detectors
{
    /// <summary>
    /// responsible for searching dependency graphs for special nodes with special behavior
    /// </summary>
    public static class SpecialNodeSearch
    {
        /// <summary>
        /// Initializes any special nodes that are used in the pipeline
        /// </summary>
        /// <param name="dependencyGraph"></param>
        public static SpecialNodeData CheckForSpecialNodes(Dictionary<int, DependentNode> dependencyGraph, DataStore staticData)
        {
            return new SpecialNodeData
            {
                Loops = null,
                SyncInformation = SyncBlockSearcher.GatherData(dependencyGraph, staticData)
            };
        }
    }
}
