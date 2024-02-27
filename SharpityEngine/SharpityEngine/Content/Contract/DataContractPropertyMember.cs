using System.Reflection;

namespace SharpityEngine.Content.Contract
{
    internal sealed class DataContractPropertyMember : DataContractProperty
    {
        // Private
        private PropertyInfo property = null;

        // Constructor
        public DataContractPropertyMember(PropertyInfo property)
            : base(property.Name, GetSerializeName(property), property.PropertyType, GetPropertyFlags(property))
        {
            this.property = property;
        }

        protected override object GetInstanceValueImpl(object instance)
        {
            return property.GetValue(instance);
        }

        protected override object SetInstanceValueImpl(object instance, object value)
        {
            property.SetValue(instance, value);
            return instance;
        }

        protected override T GetAttributeImpl<T>()
        {
            return property.GetCustomAttribute<T>();
        }

        private static AccessFlags GetPropertyFlags(PropertyInfo property)
        {
            AccessFlags flags = 0;

            if (property.CanRead == true) flags |= AccessFlags.Read;
            if (property.CanWrite == true) flags |= AccessFlags.Write;

            return flags;
        }

        public override string ToString()
        {
            return string.Format("Data Property ({0}): {1}", PropertyName, PropertyType);
        }
    }
}
