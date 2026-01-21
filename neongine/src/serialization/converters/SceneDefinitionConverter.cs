using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace neongine
{
    /// <summary>
    /// Converts a SceneDefinition to json and back into object from json
    /// </summary>
    public class SceneDefinitionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SceneDefinition);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            SceneDefinition sceneDefinition = token.ToObject<SceneDefinition>();
            return sceneDefinition;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JsonSerializer newSerializer = JsonSerializer.CreateDefault();
            newSerializer.Formatting = Formatting.Indented;
            newSerializer.Serialize(writer, value);
        }
    }
}
