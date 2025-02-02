using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using neon;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System.Linq;

namespace neongine
{
    public class ComponentConverter : JsonConverter
    {
        private static JsonSerializer m_Serializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            ContractResolver = SerializeContractResolver.Instance,
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            Converters = AssetConverters.Converters.Concat(new JsonConverter[] {
                        new EntityIDConverter(),
                        new ComponentIDConverter(),
            }).ToArray()
        });

        public override bool CanConvert(Type objectType)
        {
            return typeof(IComponent).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            /*{
                component = FormatterServices.GetUninitializedObject(objectType);
            }*/

            object component = m_Serializer.Deserialize(reader, objectType);

            return component;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject o = new JObject();

            Type componentType = value.GetType();

            JsonObjectContract contract = serializer.ContractResolver.ResolveContract(componentType) as JsonObjectContract;

            foreach (var property in contract.Properties)
            {
                Debug.WriteLine("Writing for " + value.GetType().Name + ", property: " + property.PropertyName);

                object memberValue = GetObjectValue(property.PropertyName, value, componentType);

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
