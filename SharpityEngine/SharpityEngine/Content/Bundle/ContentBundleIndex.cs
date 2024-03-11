using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SharpityEngine.Content
{
    [DataContract]
    internal sealed class ContentBundleAssetEntry
    {
        // Public
        [DataMember(Name = "Guid")]
        public string guid = "";
        [DataMember(Name = "Path")]
        public string path = "";
        [DataMember(Name = "Extension")]
        public string extension = "";
        [DataMember(Name = "DataSize")]
        public long dataSize = 0;
    }

    [DataContract]
    internal sealed class ContentBundleAssetIndex
    {
        // Private
        [DataMember(Name = "Type")]
        private string type = typeof(ContentBundleAssetIndex).FullName;

        // Internal
        [DataMember(Name = "AssetEntries")]
        internal List<ContentBundleAssetEntry> assetEntries = new List<ContentBundleAssetEntry>();

        // Properties
        public IReadOnlyList<ContentBundleAssetEntry> AssetEntries
        {
            get { return assetEntries; }
        }

        public int AssetCount
        {
            get { return assetEntries.Count; }
        }
    }
}