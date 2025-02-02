using System;

namespace neongine
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class SerializeAttribute : Attribute
	{
	}
}
