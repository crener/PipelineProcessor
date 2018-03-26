using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PipelineProcessor2.JsonTypes
{
    [JsonConverter(typeof(NodeSerializer))]
    public struct Node
    {
        public string Title;
        public string Description;
        public string Category;
        public string MenuName;
        public bool UseValue;
        public string DefaultValue;
        public List<NodeInputOutput> Input, Output;

        public Node(string title, string description, string category)
        {
            this.Title = title;
            this.Description = description;
            this.Category = category;

            Input = new List<NodeInputOutput>();
            Output = new List<NodeInputOutput>();
            MenuName = "";

            UseValue = false;
            DefaultValue = "";
        }

        public string getTypeVal()
        {
            if (MenuName == "")
            {
                string noSpace = Category + "/";
                int pos = 0;
                while (pos < Title.Length)
                {
                    if (Title[pos] != ' ') noSpace += Title[pos];
                    ++pos;
                }
                return noSpace;
            }

            return Category + "/" + MenuName;
        }
    }

    public struct NodeInputOutput
    {
        public string name, type;

        NodeInputOutput(string Name, string Type)
        {
            name = Name;
            type = Type;
        }
    }

    class NodeSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Node node = (Node)value;

            writer.WriteStartObject();

            writer.WritePropertyName("title");
            serializer.Serialize(writer, node.Title);

            writer.WritePropertyName("desc");
            serializer.Serialize(writer, node.Description);

            writer.WritePropertyName("category");
            serializer.Serialize(writer, node.Title);

            writer.WritePropertyName("type");
            serializer.Serialize(writer, node.getTypeVal());

            writer.WritePropertyName("input");
            serializer.Serialize(writer, node.Input);

            writer.WritePropertyName("output");
            serializer.Serialize(writer, node.Output);

            writer.WritePropertyName("useValue");
            serializer.Serialize(writer, node.UseValue);

            writer.WritePropertyName("defaultValue");
            serializer.Serialize(writer, node.DefaultValue);

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
            //todo http://blog.maskalik.com/asp-net/json-net-implement-custom-serialization/

            /*JObject jsonObject = JObject.Load(reader);
            var properties = jsonObject.Properties().ToList();
            Node node = new Node();

            node.title = properties[1].ToString();
            //node.Id = properties[0].Value.ToObject<int>();
            node.input = properties[6].ToObject<List<NodeInputOutput>>();
            node.output = properties[7].ToObject<List<NodeInputOutput>>();

            string[] type = properties[2].Value.ToString().Split('/');
            node.category = type[0];
            node.menuName = type[1];

            return node;*/
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Node);
        }
    }
}
