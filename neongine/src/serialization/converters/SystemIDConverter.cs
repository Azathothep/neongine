using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using neon;
using System.Diagnostics;

namespace neongine
{
	public class SystemIDConverter : JsonConverter
	{
        public static string IDKey = "id";

		public override bool CanConvert(Type objectType)
		{
			return typeof(ISystem).IsAssignableFrom(objectType);
		}

		public override bool CanRead => false;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
            throw new NotImplementedException();
        }

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject o = new JObject();

			ISystem system = (ISystem)value;

			o.Add(new JProperty(SystemIDConverter.IDKey, system.GetHashCode()));

            o.WriteTo(writer);
		}
	}
}
