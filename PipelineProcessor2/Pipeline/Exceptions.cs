using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Pipeline
{
    public class PipelineException : Exception
    {
        public PipelineException() { }
        public PipelineException(string message) : base(message) { }
        public PipelineException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class MissingNodeException : PipelineException
    {
        public MissingNodeException() { }
        public MissingNodeException(string message) : base(message) { }
        public MissingNodeException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class MissingLinkException : PipelineException
    {
        public MissingLinkException() { }
        public MissingLinkException(string message) : base(message) { }
        public MissingLinkException(string message, Exception innerException) : base(message, innerException) { }
    }
}
