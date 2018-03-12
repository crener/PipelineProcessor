namespace PluginTypes
{
    public interface  IPlugin
    {
        int InputQty { get; }
        int OutputQty { get; }
        string Description { get; }
        string Name { get; }

        string OutputType(int slot);
        string OutputName(int slot);
        string InputType(int slot);
        string InputName(int slot);
    }
}
