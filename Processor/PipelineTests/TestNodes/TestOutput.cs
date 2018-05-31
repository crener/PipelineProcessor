using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginTypes;

namespace PipelineTests.TestNodes
{
    public abstract class TestOutput : IOutputPlugin
    {
        public int InputQty => 1;
        public virtual int OutputQty => 0;
        public string Description => "Test Node";
        public string Name { get; }

        public string TypeName => "Output/" + Name;

        public TestOutput()
        {
            Name = "TestOutput";
        }
        public TestOutput(string name)
        {
            Name = name;
        }

        public virtual string OutputType(int slot)
        {
            if (slot == 0) return "*";
            return "";
        }

        public virtual string OutputName(int slot)
        {
            if (slot == 0) return "out";
            return "";
        }

        public virtual string InputType(int slot)
        {
            if (slot == 0) return "*";
            return "";
        }

        public virtual string InputName(int slot)
        {
            if (slot == 0) return "in";
            return "";
        }

        public abstract bool ExportData(string path, List<byte[]> saveData);
    }

    public class OutputPathPlugin : TestOutput
    {
        public string OutputDir;
        public bool ExportResult = true;

        public OutputPathPlugin() : base() { }

        public OutputPathPlugin(string type) : base(type) { }

        public override bool ExportData(string path, List<byte[]> saveData)
        {
            OutputDir = path;
            return ExportResult;
        }
    }
}
