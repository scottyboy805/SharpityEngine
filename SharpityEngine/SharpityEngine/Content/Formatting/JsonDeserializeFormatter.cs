using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharpityEngine.Content.Contract;
using SharpityEngine.Scene;

namespace SharpityEngine.Content
{
    internal sealed class JsonDeserializeFormatter : JsonFormatter
    {
        // Type
        private class LateBindingGuidReference
        {
            // Public
            public object instance;
            public DataContractProperty property;
            public string referenceGuid;
        }

        private sealed class DataInstance
        {
            // Public
            public object obj;

            // Constructor
            public DataInstance() { }
            public DataInstance(object instance)
            {
                this.obj = instance;
            }
        }

        // Private
#if !SIMPLE2D_DEDICATEDSERVER
        private ContentProvider contentProvider = null;
        private IContentReader.ContentReadContext context = default;
#endif
        private TypeManager typeManager = null;
        private string contentInfo = null;

        private Dictionary<string, GameElement> localInstanceCache = new Dictionary<string, GameElement>(); // guid, instance
        private HashSet<LateBindingGuidReference> lateInstanceBindings = new HashSet<LateBindingGuidReference>();

        // Properties


        // Constructor
#if !SIMPLE2D_DEDICATEDSERVER
        public JsonDeserializeFormatter(IContentReader.ContentReadContext context)
        {
            this.context = context;
            this.contentProvider = context.ContentProvider;
            this.typeManager = context.ContentProvider.TypeManager;
        }
#else
        public JsonDeserializeFormatter(TypeManager typeManager)
        {
            this.typeManager = typeManager;
        }
#endif

        // Methods
        public async Task<object> DeserializeObject(JsonReader reader, DataContract contract = null, bool useRemoteFields = false)
        {
            // Create format settings
            JsonLoadSettings settings = new JsonLoadSettings();
            settings.CommentHandling = CommentHandling.Ignore;
            settings.DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace;
            settings.LineInfoHandling = LineInfoHandling.Ignore;

            // Load the object
            JObject obj = JObject.Load(reader, settings);

            // Get type info
            string typeName = ((string)obj["Type"]);

            // Check for type found
            if (string.IsNullOrEmpty(typeName) == false)
            {
                object instance;

                // Try to create instance
                if (CreateObjectInstance(typeName, out instance, out _) == false)
                    return null;

                // Check for read only
                bool readOnly = (instance is GameScene) == false;

                // Deserialize the object
                await DeserializeRootObject(obj, instance, contract, readOnly, useRemoteFields);

                // Register type instance - Important to do this after deserialize properties so that guid is correct
                if (instance != null && instance is GameElement)
                {
                    ((GameElement)instance).isReadOnly = readOnly;
                    localInstanceCache[((GameElement)instance).Guid] = (GameElement)instance;
                }

                // Do late binding
                PerformLateBinding();

                return instance;
            }

            // Get type id info
            string typeID = ((string)obj["TypeID"]);

            // Check for type found
            if(string.IsNullOrEmpty(typeID) == false)
            {
                // Try to resolve the type
                Type loadType = typeManager.ResolveTypeShortIdentifier(typeID);
                object instance;

                // Try to create instance
                if (CreateObjectInstance(loadType, out instance) == false)
                    return null;

                // Check for read only
                bool readOnly = (instance is GameScene) == false;

                // Deserialize the object
                await DeserializeRootObject(obj, instance, contract, readOnly, useRemoteFields);

                // Register type instance - Important to do this after deserialize properties so that guid is correct
                if (instance != null && instance is GameElement)
                    localInstanceCache[((GameElement)instance).Guid] = (GameElement)instance;

                // Do late binding
                PerformLateBinding();

                return instance;
            }
            return null;
        }

        public async Task DeserializeObjectExisting<T>(JsonReader reader, T instance, DataContract contract = null, bool useRemoteFields = false)
        {
            // Create format settings
            JsonLoadSettings settings = new JsonLoadSettings();
            settings.CommentHandling = CommentHandling.Ignore;
            settings.DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace;
            settings.LineInfoHandling = LineInfoHandling.Ignore;

            // Load the object
            JObject obj = JObject.Load(reader, settings);

            // Check for read only
            bool readOnly = (instance is GameElement) && (instance as GameElement).isReadOnly;

            // Deserialize the object
            await DeserializeRootObject(obj, instance, contract, readOnly, useRemoteFields);
        }

        private async Task DeserializeRootObject(JObject obj, object element, DataContract contract, bool readOnly, bool useRemoteFields)
        {
            Type elementType = element.GetType();

            // Get data contract
            if(contract == null)
                contract = DataContract.ForType(elementType);

            // Get properties
            IReadOnlyList<DataContractProperty> properties = contract.SerializeProperties;

            // Check for remote properties
            if (useRemoteFields == true)
                properties = contract.RemoteSerializeProperties;

            // Wrap instance
            DataInstance instance = new DataInstance(element);

            // Process all members
            foreach (DataContractProperty member in properties)
            {
                await DeserializeProperty(obj, member, instance, readOnly, useRemoteFields);
            }
        }

        private async Task<object> DeserializeAny(JToken token, DataContractProperty property, bool readOnly, bool useRemoteFields)
        {
            // Check for null
            if (token.Type == JTokenType.Null)
                return null;

            if (property.IsObject == true || token.Type == JTokenType.Object)
            {
                // Check for enum
                if (property.PropertyType.IsEnum == true)
                {
                    return token.ToObject(property.PropertyType);
                }
                else
                {
                    // Check for primitives as object
                    if (token.Type != JTokenType.Object && token.Type != JTokenType.Array)
                    {
                        // Get boxed value
                        return token.ToObject<object>();
                    }

                    return await DeserializeNestedObject(token, property.PropertyType, readOnly, useRemoteFields);
                }
            }
            else if(property.IsArray == true)
            {
                return await DeserializeNestedArray(token, property.PropertyType, readOnly, useRemoteFields);
            }
            else if(property.IsProperty == true || (token.Type != JTokenType.Object && token.Type != JTokenType.Array))
            {                
                return token.ToObject(property.PropertyType);
            }
            return null;
        }

        private async Task<object> DeserializeNestedObject(JToken token, Type elementType, bool readOnly, bool useRemoteFields)
        {
            // Check for null
            if (token.Type == JTokenType.Null)
                return null;

            JObject obj = (JObject)token;

            // Get type info
            string typeName = ((string)obj["Type"]);

            // Check for type found - we are dealing with a GameElement
            if (string.IsNullOrEmpty(typeName) == false)
            {
                // Create instance
                DataInstance instance = new DataInstance();
                Type loadType;

                // Try to create new instance
                if (CreateObjectInstance(typeName, out instance.obj, out loadType, elementType) == false)
                    return null;

                // Get the data contract
                DataContract contract = DataContract.ForType(loadType);

                // Get properties
                IReadOnlyList<DataContractProperty> properties = contract.SerializeProperties;

                // Check for remote properties
                if (useRemoteFields == true)
                    properties = contract.RemoteSerializeProperties;

                // Process all members
                foreach (DataContractProperty property in properties)
                {
                    await DeserializeProperty(obj, property, instance, readOnly, useRemoteFields);
                }

                // Register type instance - Important to do this after deserialize properties so that guid is correct
                if (instance.obj != null && instance.obj is GameElement)
                {
                    ((GameElement)instance.obj).isReadOnly = readOnly;
                    localInstanceCache[((GameElement)instance.obj).Guid] = (GameElement)instance.obj;
                }

                return instance.obj;
            }
            else
            {
                // Check for reference
                if (obj.ContainsKey(AssetReference.AssetRefName) == true)
                {
                    GameElement result = await contentProvider.LoadAsync((string)obj[AssetReference.AssetRefName]);

                    // Try to convert
                    if (result != null)
                        GetObjectInstanceAsType(ref result, elementType);

                    if (result != null)
                    {
                        // Check for read only and create instance
                        if (result.IsReadOnly == true || (result is GameAsset) == false)
                        {
                            
                            //throw new NotImplementedException("Instantiate not implemented");
                            // = result.Instantiate(); // ISSUE HERE - Struct properties are not copied
                            result.isReadOnly = readOnly;

                            // Check for asset
                            if (result is GameAsset asset)
                                asset.OnAfterLoaded();
                        }

                        DataInstance assetInstance = new DataInstance(result);

                        // Get the data contract
                        DataContract assetContract = DataContract.ForType(result.GetType());

                        // Get properties
                        IReadOnlyList<DataContractProperty> assetProperties = assetContract.SerializeProperties;

                        // Check for remote properties
                        if (useRemoteFields == true)
                            assetProperties = assetContract.RemoteSerializeProperties;

                        // Process all members
                        foreach (DataContractProperty property in assetProperties)
                        {
                            await DeserializeProperty(obj, property, assetInstance, readOnly, useRemoteFields);
                        }
                    }

                    return result;
                }
                // May be a local guid reference
                if(obj.ContainsKey(AssetReference.AssetRefName) == true)
                {
                    // Create guid reference
                    return new AssetReference { assetReference = (string)obj[AssetReference.AssetRefName] };
                }

                // Check for data reference
                if(obj.ContainsKey(DataReference.DataRefName) == true)
                {
                    // Check for string
                    if(elementType == typeof(string))
                    {
                        // Try to load the string
                        return await contentProvider.LoadDataTextAsync((string)obj[DataReference.DataRefName]);
                    }
                    // Check for bytes
                    else if(elementType == typeof(byte[]))
                    {
                        // Try to load the bytes
                        return await contentProvider.LoadDataBytesAsync((string)obj.GetValue(DataReference.DataRefName));
                    }
                }

                // Create non-GameElement instance
                DataInstance instance = new DataInstance();

                // Try to create instance of type
                if (CreateObjectInstance(elementType, out instance.obj) == false)
                    return null;

                // Get the data contract
                DataContract contract = DataContract.ForType(elementType);

                // Get properties
                IReadOnlyList<DataContractProperty> properties = contract.SerializeProperties;

                // Check for remote properties
                if (useRemoteFields == true)
                    properties = contract.RemoteSerializeProperties;

                // Process all members
                foreach (DataContractProperty property in properties)
                {
                    await DeserializeProperty(obj, property, instance, readOnly, useRemoteFields);
                }

                // Register type instance - Important to do this after deserialize properties so that guid is correct
                if (instance.obj != null && instance.obj is GameElement)
                    localInstanceCache[((GameElement)instance.obj).Guid] = (GameElement)instance.obj;

                return instance.obj;
            }
        }

        private async Task<object> DeserializeNestedArray(JToken token, Type arrayType, bool readOnly, bool useRemoteFields)
        {
            // Check for null
            if (token.Type == JTokenType.Null)
                return null;

            JArray arr = (JArray)token;

            // Create array
            DataArrayInstance array = new DataArrayInstance(arrayType, arr.Count);

            // Process all elements
            for(int i = 0; i < arr.Count; i++)
            {
                // Deserialize the element
                object element = await DeserializeAny(arr[i], array[i], readOnly, useRemoteFields);

                // Check for late binding
                element = CheckLateBindingPropertyValue(array.GetInstance(), array[i], element);

                // Add to array
                array[i].SetInstanceValue(ref array, element);
            }

            return array.GetInstance();
        }

        private async Task DeserializeProperty(JObject obj, DataContractProperty property, DataInstance instance, bool readOnly, bool userRemoteFields)
        {
            // Check for match
            if(obj.ContainsKey(property.SerializeName) == true)
            {
                // Get token
                JToken token = obj[property.SerializeName];

                // Deserialize any
                object value = await DeserializeAny(token, property, readOnly, userRemoteFields);

                // Check for late binding
                value = CheckLateBindingPropertyValue(instance.obj, property, value);

                // Set value
                property.SetInstanceValue(ref instance.obj, value);
            }
        }

        private object CheckLateBindingPropertyValue(object instance, DataContractProperty property, object value)
        {
            if(value is AssetReference assetRef && ContentProvider.IsGuid(assetRef.assetReference) == true)
            {
                lateInstanceBindings.Add(new LateBindingGuidReference
                {
                    instance = instance,
                    property = property,
                    referenceGuid = ((AssetReference)value).assetReference,
                });

                // Value will be assigned at late binding stage
                return null;
            }
            return value;
        }

        private void PerformLateBinding()
        {
            foreach(LateBindingGuidReference lateBinding in lateInstanceBindings)
            {
                GameElement bindValue;

                // Try to get matching guid
                if (localInstanceCache.TryGetValue(lateBinding.referenceGuid, out bindValue) == true)
                {
                    // Update property
                    lateBinding.property.SetInstanceValue(ref lateBinding.instance, bindValue);
                }
            }
        }

        private bool CreateObjectInstance(string typeName, out object instance, out Type loadType, Type assignType = null)
        {
            // Try to resolve type
            loadType = typeManager.ResolveType(typeName);

            // Check for not found
            if (loadType == null)
            {
                Debug.LogErrorF(LogFilter.Content, this, "Error in content file: {0} - Could not find registered type: {1}", contentInfo, typeName);
                instance = null;
                return false;
            }

            // Call through
            return CreateObjectInstance(loadType, out instance, assignType);
        }

        private bool CreateObjectInstance(Type loadType, out object instance, Type assignType = null)
        {
            instance = null;

            // Check for not found
            if (loadType == null)
            {
                Debug.LogErrorF(LogFilter.Content, this, "Error in content file: {0} - Could not find registered type: Unknown", contentInfo);
                return false;
            }

            // Check for type mismatch
            if (assignType != null && assignType.IsAssignableFrom(loadType) == false)
                throw new InvalidOperationException("Cannot deserialize type: " + loadType + " as expected type: " + assignType);

            Exception error = null;
            try
            {
                // Create instance
                instance = typeManager.CreateTypeInstance(loadType);
            }
            catch (Exception e)
            {
                instance = null;
                error = e;
            }

            // Check for error
            if (instance == null)
            {
                // Get extra error
                string extraError = (error != null) ? " - " + error : string.Empty;

                Debug.LogErrorF(LogFilter.Content, this, "Error in content file: {0} - Could not create instance of registered type: {1}{2}", contentInfo, loadType, extraError);
                return false;
            }

            // Success
            return true;
        }

        private bool GetObjectInstanceAsType(ref GameElement instance, Type assignType)
        {
            // Get type
            Type instanceType = instance.GetType();

            // Check for assignable
            if(assignType.IsAssignableFrom(instanceType) == false)
            {
                // Attempt to auto-convert
                if(instance is GameObject)
                {
                    // Try to get script of type
                    instance = ((GameObject)instance).GetComponent(assignType);

                    // Check for success
                    if(instance == null)
                    {
                        Debug.LogErrorF(LogFilter.Content, this, "Error in content file: {0} - Could not convert instance or registered type {1)] to property type {2}", contentInfo, instanceType, assignType);
                        return false;
                    }
                }
            }

            // The object is correct type
            return true;
        }
    }
}
