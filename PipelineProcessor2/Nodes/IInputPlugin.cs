using System.Collections;

namespace PipelineProcessor2.Nodes
{
    public interface IInputPlugin : IPlugin
    {
        IEnumerable RetrieveData(string path);
        int InputDataQuantity(string path);
    }
}
