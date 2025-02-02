using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using neon;
using System.Linq;

namespace neongine
{
	public class ComponentIDConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(IComponent).IsAssignableFrom(objectType);
		}

		public override bool CanRead => false;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
/*            JToken token = JToken.Load(reader);
			IComponent component = (IComponent)token.ToObject(objectType);
            return component;*/

            throw new NotImplementedException();
        }

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject o = new JObject();

			IComponent component = (IComponent)value;

			o.Add(new JProperty(EntityIDConverter.IDKey, (UInt32)(component.EntityID)));

            o.WriteTo(writer);
		}
	}
}
