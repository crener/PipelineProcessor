using System.Collections.Generic;

namespace PipelineProcessor2.Nodes
{
    public interface IProcessPlugin : IPlugin
    {
        List<byte[]> ProcessData(List<byte[]> input);
    }
}
