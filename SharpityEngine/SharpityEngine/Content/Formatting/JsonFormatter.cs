using System.Runtime.Serialization;

namespace SharpityEngine.Content
{
    public abstract class JsonFormatter
    {
        // Types
        [DataContract]
        protected class ExternalFileReference
        {
            // Public
            [DataMember(Name = "ReferenceFile")]
            public string referenceFile;
        }

        [DataContract]
        protected class ExternalGuidReference
        {
            // Public
            [DataMember(Name = "ReferenceGuid")]
            public string referenceGuid;
        }
    }
}
