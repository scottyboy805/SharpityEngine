using Newtonsoft.Json;
using SharpityEngine.Content.Contract;

namespace SharpityEngine.Content
{
    internal sealed class JsonSerializeFormatter : JsonFormatter
    {
        // Private
        private TypeManager typeManager = null;
        private HashSet<GameElement> localInstanceCache = new HashSet<GameElement>();

        // Constructor
        public JsonSerializeFormatter(TypeManager typeManager)
        {
            this.typeManager = typeManager;
        }

        // Methods
        public void SerializeObject(JsonWriter writer, object element, DataContract contract = null, bool useRemoteFields = false)
        {
            // Check for null
            if (element == null)
                return;

            // Serialize the object
            SerializeRootObject(writer, element, contract, useRemoteFields);
        }

        private void SerializeRootObject(JsonWriter writer, object element, DataContract contract, bool useRemoteFields)
        {
            Type elementType = element.GetType();

            // Cache element in local file
            if(element is GameElement)
                localInstanceCache.Add((GameElement)element);

            // Start object
            writer.WriteStartObject();
            {
                // Create data contract
                if (contract == null)
                    contract = DataContract.ForType(elementType);

                // Get properties
                IReadOnlyList<DataContractProperty> properties = contract.SerializeProperties;

                // Check for remote properties
                if (useRemoteFields == true)
                    properties = contract.RemoteSerializeProperties;

                // Process all members
                foreach (DataContractProperty member in properties)
                {
                    SerializeMember(writer, member, element, useRemoteFields);
                }
            }
            writer.WriteEndObject();
        }

        private void SerializeAny(JsonWriter writer, DataContractProperty member, object instance, bool useRemoteFields)
        {
            // Get member value
            object value = member.GetInstanceValue(instance);

            // Check for null
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            // Check for object
            if (member.IsObject == true)
            {
                // Check for GameElement
                if (value is GameElement)
                {
                    // Check if we can save by file reference
                    if (((GameElement)value).HasContentPath == true)
                    {
                        // Check for an external asset which must be referenced by file
                        if (localInstanceCache.Contains((GameElement)value) == false)
                        {
                            SerializeNestedObject(writer, new ExternalFileReference { referenceFile = ((GameElement)value).ContentPath }, typeof(ExternalFileReference), useRemoteFields);
                        }
                        // Check for an internal asset which must be reference by guid to avoid infinite load cycle
                        else
                        {
                            SerializeNestedObject(writer, new ExternalGuidReference { referenceGuid = ((GameElement)value).Guid }, typeof(ExternalGuidReference), useRemoteFields);
                        }
                        return;
                    }

                    // Check for Game asset - cannot be saved inline
#if !SIMPLE2D_DEDICATEDSERVER
                    if (value is GameAsset)
                    {
                        writer.WriteNull();
                        return;
                    }
#endif

                    // Cache element
                    //localInstanceCache.Add((GameElement)value);
                }

                // Check for enum
                if (member.PropertyType.IsEnum == true)
                {
                    writer.WriteValue(value);
                }
                else
                {
                    SerializeNestedObject(writer, value, value != null ? value.GetType() : member.PropertyType, useRemoteFields);
                }
            }
            else if (member.IsArray == true)
            {
                // Get array
                DataArrayInstance array = member.GetInstanceArray(instance);

                // Pass through
                SerializeNestedArray(writer, array, useRemoteFields);
            }
            else if (member.IsProperty == true)
            {
                writer.WriteValue(value);
            }
        }

        private void SerializeNestedObject(JsonWriter writer, object element, Type elementType, bool useRemoteFields)
        {
            // Start object
            writer.WriteStartObject();
            {
                // Create contract
                DataContract contract = DataContract.ForType(elementType);

                // Get properties
                IReadOnlyList<DataContractProperty> properties = contract.SerializeProperties;

                // Check for remote properties
                if (useRemoteFields == true)
                    properties = contract.RemoteSerializeProperties;

                bool didWriteType = false;

                // Process all members
                foreach (DataContractProperty member in properties)
                {
                    SerializeMember(writer, member, element, useRemoteFields);

                    // Check for type
                    if(member.SerializeName == "Type")
                        didWriteType = true;
                }

                // Write type info
                if(didWriteType == false && contract.AlwaysSpecifyType == true)
                {
                    writer.WritePropertyName("Type");
                    writer.WriteValue(typeManager.GetTypeName(contract.ContractType));
                }
            }
            writer.WriteEndObject();
        }

        private void SerializeNestedArray(JsonWriter writer, DataArrayInstance array, bool useRemoteFields)
        {
            // Start array
            writer.WriteStartArray();
            {
                for (int i = 0; i < array.Count; i++)
                {
                    // Get the element value
                    DataContractProperty elementProperty = array[i];

                    // Write json
                    SerializeAny(writer, elementProperty, array, useRemoteFields);
                }
            }
            writer.WriteEndArray();
        }

        private void SerializeMember(JsonWriter writer, DataContractProperty member, object instance, bool useRemoteFields)
        {
            // Start property
            writer.WritePropertyName(member.SerializeName);

            // Write value
            SerializeAny(writer, member, instance, useRemoteFields);
        }
    }
}
