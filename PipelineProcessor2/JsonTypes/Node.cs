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
        public string title;
        public string description;
        public string category;
        public string menuName;
        public List<NodeInputOutput> input, output;

        public Node(string title, string description, string category)
        {
            this.title = title;
            this.description = description;
            this.category = category;

            input = new List<NodeInputOutput>();
            output = new List<NodeInputOutput>();
            menuName = "";
        }

        public string getTypeVal()
        {
            if (menuName == "")
            {
                string noSpace = category + "/";
                int pos = 0;
                while (pos < title.Length)
                {
                    if (title[pos] != ' ') noSpace += title[pos];
                    ++pos;
                }
                return noSpace;
            }

            return category + "/" + menuName;
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
            serializer.Serialize(writer, node.title);

            writer.WritePropertyName("desc");
            serializer.Serialize(writer, node.description);

            writer.WritePropertyName("category");
            serializer.Serialize(writer, node.title);

            writer.WritePropertyName("type");
            serializer.Serialize(writer, node.getTypeVal());

            writer.WritePropertyName("input");
            serializer.Serialize(writer, node.input);

            writer.WritePropertyName("output");
            serializer.Serialize(writer, node.output);

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
            //todo http://blog.maskalik.com/asp-net/json-net-implement-custom-serialization/

            JObject jsonObject = JObject.Load(reader);
            var properties = jsonObject.Properties().ToList();
            Node node = new Node();

            node.title = properties[1].ToString();
            //node.Id = properties[0].Value.ToObject<int>();
            node.input = properties[6].ToObject<List<NodeInputOutput>>();
            node.output = properties[7].ToObject<List<NodeInputOutput>>();

            string[] type = properties[2].Value.ToString().Split('/');
            node.category = type[0];
            node.menuName = type[1];

            return node;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Node);
        }
    }
}
