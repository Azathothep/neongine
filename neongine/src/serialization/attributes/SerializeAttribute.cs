using System;

namespace neongine
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
	public class SerializeAttribute : Attribute
	{
	}
}
