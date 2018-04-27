using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PipelineProcessor2.JsonTypes;

namespace OverheadBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            string value;
            while ((value = Console.ReadLine()) != "")
            {
                int total = int.Parse(value);
                if (total <= 2) return;

                List<GraphNode> nodes = new List<GraphNode>();
                Dictionary<string, NodeLinkInfo> links = new Dictionary<string, NodeLinkInfo>();

                nodes.Add(new GraphNode(0, "Image Import", "Input/ImageImport",
                    new GraphLinkReference[0],
                    new[] { new GraphLinkReference { Name = "image", Type = "jpg", LinkIds = new[] { 0 } } }));

                for (int i = 1; i < total - 1; i++)
                {
                    nodes.Add(new GraphNode(i, "Image Blur", "Process/ImageBlur",
                        new[] { new GraphLinkReference { Name = "image", Type = "jpg", LinkIds = new int[] { i - 1 } } },
                        new[] { new GraphLinkReference { Name = "image", Type = "jpg", LinkIds = new[] { i } } }));
                }

                nodes.Add(new GraphNode(total - 1, "Jpg Export", "Output/JpgExport",
                    new[] { new GraphLinkReference { Name = "image", Type = "jpg", LinkIds = new[] { total - 2 } } },
                    new GraphLinkReference[0]));


                for (int i = 0; i < total-1; i++)
                    links.Add(i.ToString(), new NodeLinkInfo(i, i, 0, i + 1, 0));


                JsonObject obj = new JsonObject();
                obj.nodes = nodes.ToArray();
                obj.links = links;
                obj.input = "E:\\in3";
                obj.output = "D:\\out";

                using (StreamWriter writer = new StreamWriter("D:\\" + value + ".txt"))
                    writer.Write(JsonConvert.SerializeObject(obj));

                Console.WriteLine("Done\n");
            }
            Console.ReadLine();
        }

        private struct JsonObject
        {
            public string input, output;
            public GraphNode[] nodes;
            public Dictionary<string, NodeLinkInfo> links;
        }
    }
}
