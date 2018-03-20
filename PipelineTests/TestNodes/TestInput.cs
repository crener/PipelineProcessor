using System.Collections.Generic;
using PluginTypes;

namespace PipelineTests.TestNodes
{
    public class TestInput : IInputPlugin
    {
        public int InputQty => 0;
        public virtual int OutputQty => 1;
        public string Description => "Test Node";
        public string Name { get; }

        public string TypeName => "Input/" + Name;

        public TestInput()
        {
            Name = "TestInput";
        }
        public TestInput(string name)
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

        public virtual IEnumerable<List<byte[]>> RetrieveData(string path)
        {
            List<byte[]> output = new List<byte[]>();
            yield return output;
        }

        public virtual int InputDataQuantity(string path)
        {
            return 1;
        }
    }


    public class BuildInputPlugin : TestInput
    {
        private int size;

        public BuildInputPlugin(int resultSize)
        {
            size = resultSize;
        }

        public BuildInputPlugin(int resultSize, string name) : base(name)
        {
            size = resultSize;
        }

        public override IEnumerable<List<byte[]>> RetrieveData(string path)
        {
            for (int i = 0; i < size; i++)
            {
                yield return new List<byte[]>();
            }
        }

        public override int InputDataQuantity(string path)
        {
            return size;
        }
    }

    public class PrematureEndPlugin : TestInput
    {
        private int size, displaySize;

        public PrematureEndPlugin(int resultSize, int advertizedSize, string name) : base(name)
        {
            size = resultSize;
            displaySize = advertizedSize;
        }

        public override IEnumerable<List<byte[]>> RetrieveData(string path)
        {
            for (int i = 0; i < size; i++)
            {
                yield return new List<byte[]>();
            }
        }

        public override int InputDataQuantity(string path)
        {
            return displaySize;
        }
    }

    public class ErrorInputPlugin : TestInput
    {
        private int size;

        public ErrorInputPlugin(int resultSize, string name) : base(name)
        {
            size = resultSize;
        }

        public override IEnumerable<List<byte[]>> RetrieveData(string path)
        {
            return null;
        }

        public override int InputDataQuantity(string path)
        {
            return size;
        }
    }
}
