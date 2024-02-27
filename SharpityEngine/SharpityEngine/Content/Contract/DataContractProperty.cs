using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;

namespace SharpityEngine.Content.Contract
{
    public abstract class DataContractProperty
    {
        // Type
        protected enum DataType
        {
            Object,
            Array,
            Property,
        }

        [Flags]
        protected enum AccessFlags
        {
            Read = 1,
            Write = 2,
        }

        // Protected
        protected string propertyName = "";
        protected string serializeName = "";
        protected Type propertyType = null;
        protected Type elementType = null;
        protected DataType dataType = 0;
        protected AccessFlags dataAccess = 0;

        // Properties
        public string PropertyName
        {
            get { return propertyName; }
        }

        public string SerializeName
        {
            get { return serializeName; }
        }

        public Type PropertyType
        {
            get { return propertyType; }
        }

        public Type ElementType
        {
            get { return elementType; }
        }

        public bool CanRead
        {
            get { return (dataAccess & AccessFlags.Read) != 0; }
        }

        public bool CanWrite
        {
            get { return (dataAccess & AccessFlags.Write) != 0; }
        }

        public bool CanReadAndWrite
        {
            get { return CanRead == true && CanWrite == true; }
        }

        public bool IsObject
        {
            get { return dataType == DataType.Object; }
        }

        public bool IsArray
        {
            get { return dataType == DataType.Array; }
        }

        public bool IsProperty
        {
            get { return dataType == DataType.Property; }
        }

        // Constructor
        protected DataContractProperty(string propertyName, Type propertyType, AccessFlags accessFlags = AccessFlags.Read | AccessFlags.Write)
            : this(propertyName, propertyName, propertyType, accessFlags)
        {
        }

        protected DataContractProperty(string propertyName, string serializeName, Type propertyType, AccessFlags accessFlags = AccessFlags.Read | AccessFlags.Write)
        {
            this.propertyName = propertyName;
            this.serializeName = serializeName;
            this.propertyType = propertyType;
            this.elementType = GetElementType(propertyType);
            this.dataType = GetMemberDataType(propertyType);
            this.dataAccess = accessFlags;
        }

        // Methods
        protected abstract object GetInstanceValueImpl(object instance);

        protected abstract object SetInstanceValueImpl(object instance, object value);

        protected abstract T GetAttributeImpl<T>() where T : Attribute;

        public override abstract string ToString();

        public object GetInstanceValue(object instance)
        {
            // Check for null
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            // Check for read
            if (CanRead == false)
                throw new InvalidOperationException("Cannot read property: " + PropertyName);

            // Return value
            return GetInstanceValueImpl(instance);
        }

        public void SetInstanceValue<T>(ref T instance, object value)
        {
            // Check for null
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            // Check for read
            if (CanWrite == false)
                throw new InvalidOperationException("Cannot write property: " + PropertyName);

            // Set new value - Note that we must copy back the instance to support structs
            instance = (T)SetInstanceValueImpl(instance, value);
        }

        public DataArrayInstance GetInstanceArray(object instance)
        {
            // Get the instance value
            object value = GetInstanceValue(instance);

            // Check for null
            if (value == null)
                return null;

            // Create array
            return new DataArrayInstance((IList)value);
        }

        public void SetInstanceArray(object instance, DataArrayInstance array)
        {
            // Set instance value
            SetInstanceValue(ref instance, array.GetInstance());
        }

        public T GetAttribute<T>() where T : Attribute
        {
            return GetAttributeImpl<T>();
        }

        public bool HasAttribute<T>() where T : Attribute
        {
            return GetAttributeImpl<T>() != null;
        }

        protected static DataType GetMemberDataType(Type type)
        {
            // Check for array and lists
            if (IsMemberArray(type) == true || IsMemberGenericList(type) == true)
            {
                return DataType.Array;
            }
            else if (type.IsPrimitive == false && type != typeof(string))
            {
                return DataType.Object;
            }

            return DataType.Property;
        }

        internal static Type GetElementType(Type type)
        {
            if (IsMemberArray(type) == true)
            {
                return type.GetElementType();
            }
            else if (IsMemberGenericList(type) == true)
            {
                return type.GetGenericArguments()[0];
            }
            return null;
        }

        public static bool IsMemberArray(Type type)
        {
            return type.IsArray;
        }

        public static bool IsMemberGenericList(Type type)
        {
            return (type.IsGenericType == true && type.GetGenericTypeDefinition() == typeof(List<>));
        }

        public static bool CheckMemberSerializable(FieldInfo field)
        {
            // Check for ignored
            if (field.GetCustomAttribute<DataContractIgnoreAttribute>() != null)
                return false;

            // Check for public
            if (field.IsPublic == true)
                return true;

            // Check for attribute
            if (field.GetCustomAttribute<DataMemberAttribute>() != null)
            {
                // Check for ignore
                if (field.GetCustomAttribute<IgnoreDataMemberAttribute>() != null)
                    return false;

                // Can be serialized
                return true;
            }
            // Cannot be serialized
            return false;
        }

        public static bool CheckMemberSerializable(PropertyInfo property)
        {
            // Check for ignored
            if (property.GetCustomAttribute<DataContractIgnoreAttribute>() != null)
                return false;

            // Check for no accessor
            if (property.GetMethod == null || property.SetMethod == null)
                return false;

            // Check for public
            //if (property.GetMethod.IsPublic == true)
            //    return true;

            // Check for attribute
            if (property.GetCustomAttribute<DataMemberAttribute>() != null)
            {
                // Check for ignore
                if (property.GetCustomAttribute<IgnoreDataMemberAttribute>() != null)
                    return false;

                // Can be serialized
                return true;
            }
            // Cannot be serialized
            return false;
        }

        public static string GetSerializeName(MemberInfo member)
        {
            // Get attribute
            DataMemberAttribute attrib = member.GetCustomAttribute<DataMemberAttribute>();

            // Get name from attribute
            if (attrib != null && string.IsNullOrEmpty(attrib.Name) == false && attrib.Name != member.Name)
                return attrib.Name;

            return member.Name;
        }
    }
}
