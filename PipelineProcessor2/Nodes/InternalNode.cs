using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Nodes
{
    /// <summary>
    /// Attribute used to denote if a node has special internal behavior
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    internal class InternalNode : Attribute
    {
        /// <summary>
        /// Show this class as an available plugin inside web requests
        /// </summary>
        public bool ShowExternal { get; set; }
    }
}
