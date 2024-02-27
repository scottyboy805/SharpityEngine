using System.Collections;

namespace SharpityEngine.Content.Contract
{
    internal sealed class DataContractElement : DataContractProperty
    {
        // Private
        private int elementIndex = 0;

        // Constructor
        public DataContractElement(Type elementType, int elementIndex)
            : base(GetElementName(elementIndex), elementType)
        {
            this.elementIndex = elementIndex;
        }

        // Methods
        protected override object GetInstanceValueImpl(object instance)
        {
            // Unwrap array
            if (instance is DataArrayInstance)
                instance = ((DataArrayInstance)instance).GetInstance();

            // Get array or list instance
            IList list = (IList)instance;

            // Return value
            return list[elementIndex];
        }

        protected override object SetInstanceValueImpl(object instance, object value)
        {
            object root = instance;

            // Unwrap array
            if (instance is DataArrayInstance)
                instance = ((DataArrayInstance)instance).GetInstance();

            // Get array of list instance
            IList list = (IList)instance;

            // Set element value
            list[elementIndex] = value;
            return root;
        }

        protected override T GetAttributeImpl<T>()
        {
            return null;
        }

        private static string GetElementName(int index)
        {
            return string.Concat("_", index);
        }

        public override string ToString()
        {
            return string.Format("Data Element[{0}]: {1}", elementIndex, PropertyType);
        }
    }
}
