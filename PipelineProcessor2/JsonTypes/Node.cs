using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PipelineProcessor2
{
    [JsonConverter(typeof(NodeSerializer))]
    public struct Node
    {
        public string title;
        [JsonProperty("desc")]
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

    public class NodeSerializer : JsonConverter
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

            /*JObject jsonObject = JObject.Load(reader);
            var properties = jsonObject.Properties().ToList();
            return new Node();
            {
                category = properties[]
                Name = properties[0].Name.Replace("$", ""),
                Value = (string)properties[0].Value
            };*/
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Node);
        }
    }
}
