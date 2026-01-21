using System;

namespace neongine
{
    /// <summary>
    /// Specify this component-type class to not be serialized
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DoNotSerializeAttribute : Attribute
    {
    }
}
