using System;

namespace neongine
{
	/// <summary>
	/// Specify this property, field or system-type class to be serialized
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
	public class SerializeAttribute : Attribute
	{
	}
}
