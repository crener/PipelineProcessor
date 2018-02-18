using System.Collections.Generic;

namespace PipelineProcessor2.Nodes
{
    public interface IGeneratorPlugin : IPlugin
    {
        List<byte[]> StaticData();
    }
}
