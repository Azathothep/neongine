using System;

namespace neongine
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DoNotSerializeAttribute : Attribute
    {
    }
}
