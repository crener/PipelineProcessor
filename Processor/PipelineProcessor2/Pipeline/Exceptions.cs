using System;

namespace PipelineProcessor2.Pipeline.Exceptions
{
    public class PipelineException : Exception
    {
        public PipelineException() { }
        public PipelineException(string message) : base(message) { }
        public PipelineException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class NodeException: PipelineException
    {
        public NodeException() { }
        public NodeException(string message) : base(message) { }
        public NodeException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class MissingNodeException : NodeException
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

    public class InvalidNodeException : NodeException
    {
        public InvalidNodeException() { }
        public InvalidNodeException(string message) : base(message) { }
        public InvalidNodeException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class MissingPluginException : PipelineException
    {
        public MissingPluginException() { }
        public MissingPluginException(string message) : base(message) { }
        public MissingPluginException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InputPluginQuantityMismatchException : PipelineException
    {
        public InputPluginQuantityMismatchException() { }
        public InputPluginQuantityMismatchException(string message) : base(message) { }
        public InputPluginQuantityMismatchException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class MissingPluginDataException : PipelineException
    {
        public MissingPluginDataException() { }
        public MissingPluginDataException(string message) : base(message) { }
        public MissingPluginDataException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class DataSlotAlreadyInUseException : NodeException
    {
        public DataSlotAlreadyInUseException() { }
        public DataSlotAlreadyInUseException(string message) : base(message) { }
        public DataSlotAlreadyInUseException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class InvalidConnectionException : NodeException
    {
        public InvalidConnectionException() { }
        public InvalidConnectionException(string message) : base(message) { }
        public InvalidConnectionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class SlotLimitExceeded : NodeException
    {
        public SlotLimitExceeded() { }
        public SlotLimitExceeded(string message) : base(message) { }
        public SlotLimitExceeded(string message, Exception innerException) : base(message, innerException) { }
    }

    public class CoDependentLoopException : PipelineException
    {
        public CoDependentLoopException() { }
        public CoDependentLoopException(string message) : base(message) { }
        public CoDependentLoopException(string message, Exception innerException) : base(message, innerException) { }
    }
}
