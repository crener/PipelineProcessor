using System.Collections;

namespace PipelineProcessor2.Nodes
{
    interface IInputPlugin : IPlugin
    {
        IEnumerable RetrieveData(string path);
    }
}
