using System.Collections;
using System.Reflection;

namespace SharpityEngine.Content.Contract
{
    public sealed class DataArrayInstance
    {
        // Private
        private IList array = null;
        private List<DataContractElement> elements = new List<DataContractElement>();
        private bool isArray = false;
        private Type collectionType = null;
        private Type elementType = null;

        // Properties
        public int Count
        {
            get { return array.Count; }
        }

        public Type CollectionType
        {
            get { return collectionType; }
        }

        public Type ElementType
        {
            get { return elementType; }
        }

        public DataContractProperty this[int index]
        {
            get { return elements[index]; }
        }

        // Constructor
        public DataArrayInstance(Array array)
            : this((IList)array)
        {
        }

        public DataArrayInstance(IList instance)
        {
            // Check for null
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            this.array = instance;
            this.isArray = (instance is Array);
            this.collectionType = instance.GetType();
            this.elementType = DataContractProperty.GetElementType(collectionType);

            // Create array elements
            InitializeElements();
        }

        public DataArrayInstance(Type arrayOrListType, int count = 0)
        {
            // Check for null
            if (arrayOrListType == null)
                throw new ArgumentNullException(nameof(arrayOrListType));

            // Check for array
            if (DataContractProperty.IsMemberArray(arrayOrListType) == true)
            {
                // Create array
                this.array = Array.CreateInstance(arrayOrListType.GetElementType(), count);
                this.collectionType = arrayOrListType;
                this.elementType = arrayOrListType.GetElementType();
                this.isArray = true;
            }
            // Check for list
            else if (DataContractProperty.IsMemberGenericList(arrayOrListType) == true)
            {
                // Create list
                this.array = (IList)Activator.CreateInstance(arrayOrListType);
                this.collectionType = arrayOrListType;
                this.elementType = arrayOrListType.GetGenericArguments()[0];
                this.isArray = false;

                MethodInfo adder = arrayOrListType.GetMethod("Add", new Type[] { elementType });

                // Add all elements
                for (int i = 0; i < count; i++)
                    adder.Invoke(array, new object[] { null });
                    //array.Add(null);
            }
            else
                throw new ArgumentException("The specified type must be an array or generic list");

            // Create array elements
            InitializeElements();
        }

        // Methods
        public IList GetInstance()
        {
            return array;
        }

        public IList GetInstanceWithoutNull()
        {
            // Check for null
            if(array.Contains(null) == true)
            {
                while (array.Contains(null) == true)
                    array.Remove(null);
            }

            return array;
        }

        public override string ToString()
        {
            return string.Format("Data Array({0}): {1}", Count, CollectionType);
        }

        private void InitializeElements()
        {
            // Initialize elements
            for (int i = 0; i < array.Count; i++)
            {
                // Get the value
                object arrayElementValue = array[i];

                // Check for type
                Type finalElementType = (arrayElementValue != null)
                    ? arrayElementValue.GetType()
                    : elementType;

                // Add the element
                elements.Add(new DataContractElement(finalElementType, i));
            }
        }
    }
}
