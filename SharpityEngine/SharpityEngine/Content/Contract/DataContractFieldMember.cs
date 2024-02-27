using System.Reflection;

namespace SharpityEngine.Content.Contract
{
    internal sealed class DataContractFieldMember : DataContractProperty
    {
        // Private
        private FieldInfo field = null;

        // Constructor
        public DataContractFieldMember(FieldInfo field)
            : base(field.Name, GetSerializeName(field), field.FieldType)
        {
            this.field = field;
        }

        // Methods
        protected override object GetInstanceValueImpl(object instance)
        {
            return field.GetValue(instance);
        }

        protected override object SetInstanceValueImpl(object instance, object value)
        {
            field.SetValue(instance, value);
            return instance;
        }

        protected override T GetAttributeImpl<T>()
        {
            return field.GetCustomAttribute<T>();
        }

        public override string ToString()
        {
            return string.Format("Data Field ({0}): {1}", PropertyName, PropertyType);
        }
    }
}
