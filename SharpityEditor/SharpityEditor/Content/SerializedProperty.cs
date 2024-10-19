using SharpityEngine.Content.Contract;

namespace SharpityEditor.Content
{
    public sealed class SerializedProperty
    {
        // Private
        private SerializedElement element = null;
        private DataContractProperty property = null;

        // Properties
        public SerializedElement Element
        {
            get { return element; }
        }

        public string PropertyName
        {
            get { return property.PropertyName; }
        }

        public Type Type
        { 
            get { return property.PropertyType; } 
        }

        public bool IsArray
        {
            get { return property.IsArray; }
        }

        public bool IsReadOnly
        {
            get { return property.CanWrite == false; }
        }

        // Constructor
        internal SerializedProperty(SerializedElement element, DataContractProperty property)
        {
            this.element = element;
            this.property = property;
        }

        // Methods
        public void SetPropertyValue<T>(T value)
        {
            element.SetPropertyValue<T>(property, value);
        }

        public T GetPropertyValue<T>(out bool isMixed)
        {
            return element.GetPropertyValue<T>(property, out isMixed);
        }
    }
}
