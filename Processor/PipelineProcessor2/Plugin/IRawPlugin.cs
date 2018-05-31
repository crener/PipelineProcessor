using PluginTypes;

namespace PipelineProcessor2.Plugin
{
    public interface IRawPlugin : IPlugin
    {
        string FullName { get; }
    }
}
