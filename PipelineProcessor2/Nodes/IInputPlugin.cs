using System.Collections;
using System.Collections.Generic;

namespace PipelineProcessor2.Nodes
{
    public interface IInputPlugin : IPlugin
    {
        IEnumerable<List<byte[]>> RetrieveData(string path);
        int InputDataQuantity(string path);
    }
}
