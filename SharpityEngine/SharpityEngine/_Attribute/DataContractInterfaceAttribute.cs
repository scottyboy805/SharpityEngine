using System;

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class DataContractInterfaceAttribute : Attribute
    {
        // Empty class
    }
}
