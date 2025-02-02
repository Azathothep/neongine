using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using neon;

namespace neongine
{
	public class EntityIDConverter : JsonConverter
	{
		public static string IDKey = "id";

		public override bool CanConvert(Type objectType)
		{
			return typeof(EntityID).IsAssignableFrom(objectType);
		}

		public override bool CanRead => false;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject o = new JObject();

			EntityID entity = (EntityID)value;

			o.Add(new JProperty(EntityIDConverter.IDKey, (UInt32)entity));

			o.WriteTo(writer);
		}
	}
}
