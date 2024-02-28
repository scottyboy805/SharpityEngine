using System;

namespace SharpityEngine
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false)]
    public sealed class TypeManagerIgnoreAttribute : Attribute
    {
        // Empty class
    }
}
