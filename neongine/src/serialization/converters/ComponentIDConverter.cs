using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using neon;
using System.Linq;

namespace neongine
{
	/// <summary>
	/// Converts component references to json by only storing their ID
	/// </summary>
	public class ComponentIDConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return typeof(Component).IsAssignableFrom(objectType);
		}

		public override bool CanRead => false;

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
            throw new NotImplementedException();
        }

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			JObject o = new JObject();

			Component component = (Component)value;

			o.Add(new JProperty(NewtonsoftJsonSerializer.GUID, (UInt32)(component.EntityID)));

            o.WriteTo(writer);
		}
	}
}
