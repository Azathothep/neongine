using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace neongine
{
    public class AssetConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);

            string assetName = (string)obj["asset"];

            return Assets.GetAsset(assetName, typeof(T));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JToken t = JToken.FromObject(value);

            if (t.Type != JTokenType.Object)
            {
                t.WriteTo(writer);
            }
            else
            {
                JObject o = (JObject)t;

                string name = Assets.GetPath(value, typeof(T));

                o.ReplaceAll(new JProperty("asset", name));

                o.WriteTo(writer);
            }
        }
    }
}
