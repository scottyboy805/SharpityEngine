using Newtonsoft.Json.Linq;
using SharpityEngine.Content.Contract;

namespace SharpityEditor.Content
{
    public sealed class SerializedElement
    {
        // Private
        private object[] instances = null;
        private DataContract contract = null;
        private SerializedProperty[] properties = null;

        // Properties
        public DataContract Contract
        {
            get { return contract; }
        }

        public IReadOnlyList<SerializedProperty> Properties
        {
            get { return properties; }
        }

        // Constructor
        internal SerializedElement(object[] selection, DataContract contract = null)
        {
            // Check for any selection
            if (selection == null || selection.Length == 0 || selection[0] == null)
                throw new ArgumentException("No selection");

            this.instances = selection;
            this.contract = contract != null ? contract : DataContract.ForType(selection[0].GetType());

            // Create properties
            this.properties = CreateProperties(contract);
        }

        // Methods
        internal void SetPropertyValue<T>(DataContractProperty property, T value)
        {
            // Check compatible
            if (property.PropertyType.IsAssignableFrom(typeof(T)) == false)
                throw new ArgumentException(string.Format("Cannot convert value `{0}` to type: {1}", value, property.PropertyType));

            // Update all instances
            foreach(object instance in instances)
            {
                object inst = instance;
                property.SetInstanceValue(ref inst, value);
            }
        }

        internal T GetPropertyValue<T>(DataContractProperty property, out bool isMixed)
        {
            // Check compatible
            if (property.PropertyType.IsAssignableFrom(typeof(T)) == false)
                throw new ArgumentException(string.Format("Cannot return value `{0}` from type: {1}", typeof(T), property.PropertyType));

            // Get value from all instances
            T result = default;
            T first = default;
            isMixed = false;

            // Get all instances
            for(int i = 0; i < instances.Length; i++)
            {
                // Get current value
                T current = (T)property.GetInstanceValue(instances[i]);

                // Check for first value
                if (i == 0)
                {
                    result = current;
                    first = current;
                }

                // Check for mixed
                if (result.Equals(first) == false)
                {
                    isMixed = true;
                    break;
                }
            }
            return result;
        }

        private SerializedProperty[] CreateProperties(DataContract contract)
        {
            // Create array
            SerializedProperty[] properties = new SerializedProperty[contract.SerializeProperties.Count];

            // Initialize elements
            for(int i = 0; i < properties.Length; i++)
            {
                properties[i] = new SerializedProperty(this, contract.SerializeProperties[i]);
            }

            return properties;
        }
    }
}
