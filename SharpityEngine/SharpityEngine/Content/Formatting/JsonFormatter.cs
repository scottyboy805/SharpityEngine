using System.Runtime.Serialization;

namespace SharpityEngine.Content
{
    public abstract class JsonFormatter
    {
        // Types
        [DataContract]
        protected class AssetReference
        {
            // Public
            public const string AssetRefName = "AssetRef";

            [DataMember(Name = AssetRefName)]
            public string assetReference;
        }

        [DataContract]
        protected class DataReference
        {
            // Public
            public const string DataRefName = "DataRef";

            [DataMember(Name = DataRefName)]
            public string dataReference;
        }
    }
}
