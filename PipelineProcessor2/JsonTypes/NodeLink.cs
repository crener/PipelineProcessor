using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PipelineProcessor2.JsonTypes
{
    [JsonConverter(typeof(NodeInfoSerializer))]
    public struct NodeLinkInfo
    {
        public int Id { get; }
        public int OriginId { get; }
        public int OriginSlot { get; }
        public int TargetId { get; }
        public int TargetSlot { get; }

        public NodeLinkInfo(int id, int originId, int originSlot, int targetId, int targetSlot)
        {
            Id = id;
            OriginId = originId;
            OriginSlot = originSlot;
            TargetId = targetId;
            TargetSlot = targetSlot;
        }
    }

    class NodeInfoSerializer : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            var properties = jsonObject.Properties().ToList();

            return new NodeLinkInfo(
                properties[0].Value.ToObject<int>(),
                properties[1].Value.ToObject<int>(),
                properties[2].Value.ToObject<int>(),
                properties[3].Value.ToObject<int>(),
                properties[4].Value.ToObject<int>());
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(NodeLinkInfo);
        }
    }
}
