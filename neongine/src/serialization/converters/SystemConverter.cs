using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using neon;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using System.Linq;

namespace neongine
{
    /// <summary>
    /// Converts a system into json and back into object from json
    /// </summary>
    public class SystemConverter : JsonConverter
    {
        private static JsonSerializer m_Serializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            ContractResolver = SerializeContractResolver.Instance,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Converters = AssetConverters.Converters.Concat(new JsonConverter[] {
                        new EntityIDConverter(),
                        new ComponentIDConverter(),
                        new SystemIDConverter()
            }).ToArray()
        });

        public override bool CanConvert(Type objectType)
        {
            return typeof(ISystem).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object system = m_Serializer.Deserialize(reader, objectType);

            return system;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject o = new JObject();

            Type systemType = value.GetType();

            JsonObjectContract contract = serializer.ContractResolver.ResolveContract(systemType) as JsonObjectContract;

            foreach (var property in contract.Properties)
            {
                object memberValue = GetObjectValue(property.PropertyName, value, systemType);

                o.Add(property.PropertyName, JToken.FromObject(memberValue, m_Serializer));
            }

            o.WriteTo(writer);
        }

        private object GetObjectValue(string memberName, object value, Type componentType)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            FieldInfo fieldInfo = componentType.GetField(memberName, flags);
            if (fieldInfo != null)
                return fieldInfo.GetValue(value);

            PropertyInfo propertyInfo = componentType.GetProperty(memberName, flags);
            if (propertyInfo != null)
                return propertyInfo.GetValue(value);

            return null;
        }
    }
}
