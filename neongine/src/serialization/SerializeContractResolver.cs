using Microsoft.Xna.Framework.Graphics;
using neon;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;

namespace neongine
{
	/// <summary>
	/// Provides the list of all serializable members of a specific type.
	/// All fields and properties, public or private, are non serialized by default. They must be decorated with the [Serialize] attribute to be serialized.
	/// </summary>
	public class SerializeContractResolver : DefaultContractResolver
	{
		public static readonly SerializeContractResolver Instance = new SerializeContractResolver();

		protected override List<MemberInfo> GetSerializableMembers(Type objectType)
		{
			var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			MemberInfo[] serializableFields = objectType.GetFields(flags).Where(fieldInfo => {
				bool canBeSerialized = fieldInfo.GetCustomAttribute<SerializeAttribute>() != null;
				return canBeSerialized;
			}).ToArray();

			MemberInfo[] serializableProperties = objectType.GetProperties(flags).Where(propInfo =>
			{
				bool canBeSerialized = propInfo.CanWrite && propInfo.GetCustomAttribute<SerializeAttribute>() != null;
				return canBeSerialized;
			}).ToArray();

			List<MemberInfo> serializableMembers = (serializableFields.Concat(serializableProperties)).ToList();

			return serializableMembers;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, MemberSerialization.Fields);
        }
    }
}
