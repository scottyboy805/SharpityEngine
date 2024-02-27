using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using SharpityEngine.Content.Contract;
using DataContract = SharpityEngine.Content.Contract.DataContract;

namespace SharpityEngine
{
    public interface ITypeResolver
    {
        // Properties
        int Priority { get; }

        // Methods
        Type ResolveType(string typeName);
    }

    public sealed class TypeManager
    {
        // Type
        private class TypeResolverPriorityComparer : IComparer<ITypeResolver>
        {
            // Methods
            public int Compare(ITypeResolver x, ITypeResolver y)
            {
                return x.Priority.CompareTo(y.Priority);
            }
        }

        // Events
        public GameEvent<Type> OnTypeRegistered = new GameEvent<Type>();

        // Private
        private static readonly TypeResolverPriorityComparer typeResolverComparer = new TypeResolverPriorityComparer();
        private static byte[] typeIdentifierBytes = new byte[255];

        private List<Assembly> cachedAssemblies = new List<Assembly>();
        private Dictionary<string, Type> cachedTypes = new Dictionary<string, Type>();          // typeName, Type
        private Dictionary<Type, Func<object>> customTypeCreators = new Dictionary<Type, Func<object>>();
        private List<ITypeResolver> customTypeResolvers = new List<ITypeResolver>();

        // Properties
        public IEnumerable<Assembly> Assemblies
        {
            get { return cachedAssemblies; }
        }

        // Methods
        public Type GetUsableType(Type type)
        {
            // Check for null
            if (type == null)
                return null;

            // Check for attribute
            while (type.IsDefined(typeof(TypeManagerIgnoreAttribute), false) == true)
                type = type.BaseType;

            return type;
        }

        public T CreateTypeInstanceAs<T>(string typeName) where T : class
        {
            return CreateTypeInstance(typeName) as T;
        }

        public T CreateTypeInstanceAs<T>(Type type) where T : class
        {
            return CreateTypeInstance(type) as T;
        }

        public object CreateTypeInstance(string typeName)
        {
            // Resolve type by name
            Type targetType = ResolveType(typeName);

            // Create the instance
            if (targetType != null)
                return CreateTypeInstance(targetType);

            // Failed to find the type
            return null;
        }

        public object CreateTypeInstance(Type type)
        {
            // Check for null
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Check for custom type creator
            Func<object> creator;

            // Try to get creator
            if (customTypeCreators.TryGetValue(type, out creator) == true)
            {
                try
                {
                    return creator();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception while invoking custom type creator, falling back to default create behaviour...");
                    Debug.LogException(e);
                }
            }

            // Try to create instance
            try
            {
                // Try to create instance
                return Activator.CreateInstance(type, true);
            }
            catch
            {
                // Constructor maybe missing or failed - create uninitialized instance
                object result = FormatterServices.GetUninitializedObject(type);

                // Run initializer to setup game context
                if (result is Object)
                    Object.initializer.Invoke(result, null);

                return result;
            }
        }

        public Type ResolveType(string typeName)
        {
            Type result = null;

            // Check for cached
            if (cachedTypes.TryGetValue(typeName, out result) == true)
                return result;

            // Run custom type resolvers
            foreach (ITypeResolver customTypeResolver in customTypeResolvers)
            {
                try
                {
                    // Try to resolve type
                    result = customTypeResolver.ResolveType(typeName);

                    // Check for type resolved
                    if (result != null)
                        break;
                }
                catch { }
            }

            // Get result
            return result;
        }

        public Type ResolveTypeShortIdentifier(string typeIdentifier)
        {
            // Read the bytes into buffer
            int bytes = Encoding.UTF8.GetBytes(typeIdentifier, 0, typeIdentifier.Length, typeIdentifierBytes, 0);

            // Create stream
            using (MemoryStream stream = new MemoryStream(typeIdentifierBytes, 0, bytes))
            {
                // Reset stream
                stream.Seek(0, SeekOrigin.Begin);
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    return ReadTypeShortIdentifier(reader);
                }
            }
        }

        private Type ReadTypeShortIdentifier(BinaryReader reader)
        {
            // Read index
            int index = reader.ReadSByte();
            int typeMetadata = reader.ReadInt32();

            Assembly asm = null;

            // Check for known assembly
            if (index >= 0)
            {
                asm = cachedAssemblies[index];
            }
            else
            {
                // Get assembly by name
                asm = Assembly.Load(reader.ReadString());
            }

            // Get number of generics
            int genericCount = reader.ReadByte();

            // Check for generics
            Type[] generics = null;

            if (genericCount > 0)
            {
                // Init generics
                generics = new Type[genericCount];

                // Read all generic types
                for (int i = 0; i < genericCount; i++)
                {
                    generics[i] = ReadTypeShortIdentifier(reader);
                }
            }

            // Try to resolve the type
            foreach (Module module in asm.GetModules())
            {
                // Try to resolve
                Type type = module.ResolveType(typeMetadata, generics, null);

                if (type != null)
                {
                    // Make generic
                    if (genericCount > 0)
                        return type.MakeGenericType(generics);

                    return type;
                }
            }

            return null;
        }

        public void RegisterTypeCreator(Type type, Func<object> creator)
        {
            // Check for null
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Check for already added
            if (customTypeCreators.ContainsKey(type) == true)
                throw new InvalidOperationException("A type creator is already registered for the type: " + type);

            // Add
            customTypeCreators[type] = creator;
        }

        public void UnregisterTypeCreator(Type type)
        {
            if (customTypeCreators.ContainsKey(type) == true)
                customTypeCreators.Remove(type);
        }

        public void RegisterTypeResolver(ITypeResolver typeResolver)
        {
            if (typeResolver != null && customTypeResolvers.Contains(typeResolver) == false)
            {
                customTypeResolvers.Add(typeResolver);
                customTypeResolvers.Sort(typeResolverComparer);
            }
        }

        public void UnregisterTypeResolver(ITypeResolver typeResolver)
        {
            if (customTypeResolvers.Contains(typeResolver) == true)
                customTypeResolvers.Remove(typeResolver);
        }

        public void RegisterAssembly(Assembly asm)
        {
            Debug.Log(LogFilter.Content, "Register assembly module: " + asm.FullName);

            // Add assembly
            if (cachedAssemblies.Contains(asm) == false)
                cachedAssemblies.Add(asm);

            // Process all types
            foreach (Type type in asm.GetTypes())
            {
                RegisterType(type);
            }
        }

        public void RegisterType(Type type)
        {
            // Check for null
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Check for attribute
            if (type.IsDefined(typeof(TypeManagerIgnoreAttribute), false) == true)
                return;

            string typeName = type.FullName;

            // Get the full name
            if (type.Assembly != typeof(TypeManager).Assembly)
                typeName = type.FullName + ", " + type.Assembly.GetName().Name; //type.AssemblyQualifiedName;

            // Check for already registered
            if (cachedTypes.ContainsKey(typeName) == true)
                throw new InvalidOperationException("Type is already registered: " + type);

            // Cache the type
            cachedTypes[typeName] = type;

            // Trigger event
            OnTypeRegistered.Raise(type);

            // Get nested types
            //foreach (Type nested in type.GetNestedTypes())
            //{
            //        RegisterType(nested, useAssemblyQualifiedName);
            //}
        }

        public void RegisterType(Type type, string typeName)
        {
            // Check for null
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // Check for empty
            if (string.IsNullOrEmpty(typeName) == true)
                throw new ArgumentException(nameof(typeName) + " cannot be null or empty");

            // Check for already registered
            if (cachedTypes.ContainsKey(typeName) == true)
                throw new InvalidOperationException("Type name is already registered: " + typeName);

            // Cache the type
            cachedTypes[typeName] = type;
        }

        public bool IsTypeRegistered(Type type)
        {
            // Check for null
            if (type == null)
                return false;

            // Get the type name
            string typeName = GetTypeName(type, false);

            // Type is not registered
            if (string.IsNullOrEmpty(typeName) == true)
                return false;

            // Check for registed
            return cachedTypes.ContainsKey(typeName);
        }

        public bool IsTypeRegistered(string typeName)
        {
            // Check for null or empty
            if (string.IsNullOrEmpty(typeName) == true)
                return false;

            // Check for registered
            return cachedTypes.ContainsKey(typeName);
        }

        public string GetTypeName(Type type, bool throwOnError = true)
        {
            if (cachedAssemblies.Contains(type.Assembly) == false)
            {
                if (throwOnError == true)
                    throw new InvalidOperationException("Type is not registered: " + type);

                // No type
                return null;
            }

            // Use assembly qualified name
            if (typeof(TypeManager).Assembly != type.Assembly)
                return type.AssemblyQualifiedName;

            // use shortened full name
            return type.FullName;
        }

        public string GetTypeShortIdentifier(Type type)
        {
            using (MemoryStream stream = new MemoryStream(typeIdentifierBytes, true))
            {
                // Reset stream
                stream.Seek(0, SeekOrigin.Begin);
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // Write the type
                    WriteTypeShortIdentifier(writer, type);

                    // Get as string
                    return Encoding.UTF8.GetString(typeIdentifierBytes, 0, (int)stream.Position);
                }
            }
        }

        private void WriteTypeShortIdentifier(BinaryWriter writer, Type type)
        {
            int index = cachedAssemblies.IndexOf(type.Assembly);

            // Write assembly and type metadata
            writer.Write((sbyte)index);

            // Write assembly name
            if (index == -1)
                writer.Write(type.Assembly.GetName().Name);

            writer.Write(type.MetadataToken);

            // Check for generics
            Type[] generics = type.GenericTypeArguments;

            writer.Write((byte)generics.Length);

            for (int i = 0; i < generics.Length; i++)
            {
                // Write generic info
                WriteTypeShortIdentifier(writer, generics[i]);
            }
        }

        internal void InitializeElementTypeInstance(Object element)
        {
            // Initialize members
            DataContract contract = DataContract.ForType(element.elementType);

            // Process all serialize fields
            foreach (DataContractProperty property in contract.SerializeProperties)
            {
                // Check for reference types
                if (property.IsObject == true)
                {
                    // Check for null
                    if (property.GetInstanceValue(element) == null && property.PropertyType.GetCustomAttribute<DataContractNonNullAttribute>() != null)
                    {
                        // Create instance
                        object instance = CreateTypeInstance(property.PropertyType);

                        // Assign instance
                        if (instance != null)
                        {
                            // Initialize instance
                            //InitializeTypeInstance(instance, property.PropertyType);

                            // Assign value
                            property.SetInstanceValue(ref element, instance);
                        }
                    }
                }
            }
        }

        //internal void InitializeTypeInstance(object instance, Type type)
        //{
        //    // Check for string
        //    if (type == typeof(string))
        //        return;

        //    // Check for object
        //    if (type.IsClass == true || (type.IsValueType == true && type.IsPrimitive == false))
        //    {
        //        // Initialize members
        //        DataContract contract = DataContract.ForType(type);

        //        // Process all serialize fields
        //        foreach (DataContractProperty property in contract.SerializeProperties)
        //        {
        //            // Check for reference types
        //            if (property.IsObject == true)
        //            {
        //                // Check for null
        //                if (property.GetInstanceValue(instance) == null && property.PropertyType.GetCustomAttribute<DataContractNonNullAttribute>() != null)
        //                {
        //                    // Create instance - Note that this call may fail if type is abstract for example
        //                    object defaultInstance = CreateTypeInstance(property.PropertyType);

        //                    // Assign instance
        //                    if (instance != null)
        //                    {
        //                        // Recursive initialize
        //                        InitiailizeTypeInstance(defaultInstance, property.PropertyType);

        //                        // Assign instance
        //                        property.SetInstanceValue(ref instance, defaultInstance);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //internal bool CopyTypeInstance<TSrc, TDest>(in TSrc instance, Type type, ref TDest copyTo)
        //{
        //    // Check for null
        //    if (copyTo == null)
        //        return false;

        //    // Get data contract
        //    DataContract contract = DataContract.ForType(type);

        //    // Get the destination type
        //    Type destType = copyTo.GetType();

        //    // Make sure types are compatible
        //    if (type.IsAssignableFrom(destType) == false)
        //        return false;

        //    // Process all fields
        //    foreach (DataContractProperty property in contract.SerializeProperties)
        //    {
        //        // Copy the value
        //        object clonedPropertyValue = CopyObjectInstance(instance, property);

        //        // Apply copied property
        //        property.SetInstanceValue(ref copyTo, clonedPropertyValue);
        //    }

        //    return true;
        //}

        //internal object CopyObjectInstance<TSrc>(in TSrc value, DataContractProperty property)
        //{
        //    // Check for trivial case
        //    if (value == null)
        //        return null;

        //    // Check for array or collection
        //    if (property.IsArray == true)
        //    {
        //        // Get the array property
        //        DataArrayInstance array = property.GetInstanceArray(value);

        //        // Create duplicate array
        //        DataArrayInstance copyArray = new DataArrayInstance(array.CollectionType, array.Count);

        //        // Get the new array instance
        //        IList copyList = copyArray.GetInstance();

        //        // Copy all elements
        //        for (int i = 0; i < array.Count; i++)
        //        {
        //            // Get the element property
        //            DataContractProperty elementProperty = array[i];

        //            // Clone the element
        //            object cloneElementValue = CopyObjectInstance(array, elementProperty);

        //            // Insert cloned element
        //            copyList[i] = cloneElementValue;
        //        }

        //        // Return collection
        //        return copyList;
        //    }
        //    else
        //    {
        //        // Get the property value
        //        object propertyValue = property.GetInstanceValue(value);

        //        // Check for null
        //        if (propertyValue == null)
        //            return null;

        //        switch (propertyValue)
        //        {
        //            case sbyte: return (sbyte)propertyValue;
        //            case byte: return (byte)propertyValue;
        //            case short: return (short)propertyValue;
        //            case ushort: return (ushort)propertyValue;
        //            case int: return (int)propertyValue;
        //            case uint: return (uint)propertyValue;
        //            case long: return (long)propertyValue;
        //            case ulong: return (ulong)propertyValue;
        //            case float: return (float)propertyValue;
        //            case double: return (double)propertyValue;
        //            case decimal: return (decimal)propertyValue;
        //            case string: return (string)((string)propertyValue).Clone(); //string.Copy((string)propertyValue);
        //            case bool: return (bool)propertyValue;
        //        }

        //        // Check for element
        //        if (propertyValue is Object)
        //        {
        //            // Check for asset - Assets are shared and should not be cloned by default
        //            if (propertyValue is GameAsset || ((Object)propertyValue).IsReadOnly == true)
        //                return propertyValue;

        //            // Create clone
        //            return (propertyValue as Object).DoExplicitInstantiate();
        //        }

        //        // Get the target instance type
        //        Type newInstanceType = propertyValue.GetType();

        //        // Check for user type
        //        object newInstance = CreateTypeInstance(newInstanceType);

        //        if (newInstance != null)
        //        {
        //            // Try to copy onto new instance
        //            CopyTypeInstance(propertyValue, newInstanceType, ref newInstance);

        //            // Return result
        //            return newInstance;
        //        }
        //    }

        //    // Failed to clone
        //    throw new NotSupportedException("Cannot copy instance value: " + property.PropertyType);
        //}

        //internal static void DestroyElementTypeInstance(Object element)
        //{
        //    // Call destroy
        //    element.DestroyImmediate();

        //    // Initialize members
        //    DataContract contract = DataContract.ForType(element.elementType);

        //    // Process all serialize fields
        //    foreach (DataContractProperty property in contract.SerializeProperties)
        //    {
        //        // Check for reference types
        //        if (property.IsObject == true && typeof(Object).IsAssignableFrom(property.PropertyType) == true)
        //        {
        //            // Create instance
        //            Object instance = property.GetInstanceValue(element) as Object;

        //            if (instance != null)
        //                instance.DestroyImmediate();
        //        }
        //    }
        //}
    }
}
