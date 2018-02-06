using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineProcessor2.Pipeline.Exceptions
{
    public class PipelineException : System.Exception
    {
        public PipelineException() { }
        public PipelineException(string message) : base(message) { }
        public PipelineException(string message, System.Exception innerException) : base(message, innerException) { }
    }

    public class MissingNodeException : PipelineException
    {
        public MissingNodeException() { }
        public MissingNodeException(string message) : base(message) { }
        public MissingNodeException(string message, System.Exception innerException) : base(message, innerException) { }
    }

    public class MissingLinkException : PipelineException
    {
        public MissingLinkException() { }
        public MissingLinkException(string message) : base(message) { }
        public MissingLinkException(string message, System.Exception innerException) : base(message, innerException) { }
    }

    public class InvalidNodeException : PipelineException
    {
        public InvalidNodeException() { }
        public InvalidNodeException(string message) : base(message) { }
        public InvalidNodeException(string message, System.Exception innerException) : base(message, innerException) { }
    }

    public class MissingPluginException : PipelineException
    {
        public MissingPluginException() { }
        public MissingPluginException(string message) : base(message) { }
        public MissingPluginException(string message, System.Exception innerException) : base(message, innerException) { }
    }

    public class InputPluginQuantityMismatchException : PipelineException
    {
        public InputPluginQuantityMismatchException() { }
        public InputPluginQuantityMismatchException(string message) : base(message) { }
        public InputPluginQuantityMismatchException(string message, System.Exception innerException) : base(message, innerException) { }
    }

    public class MissingPluginDataException : PipelineException
    {
        public MissingPluginDataException() { }
        public MissingPluginDataException(string message) : base(message) { }
        public MissingPluginDataException(string message, System.Exception innerException) : base(message, innerException) { }
    }

    public class DataSlotAlreadyInUse : PipelineException
    {
        public DataSlotAlreadyInUse() { }
        public DataSlotAlreadyInUse(string message) : base(message) { }
        public DataSlotAlreadyInUse(string message, System.Exception innerException) : base(message, innerException) { }
    }
}
