using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public class DataAsset : GameAsset
    {
        // Private
        [DataMember(Name = "LoadOnDemand")]
        private bool loadOnDemand = true;
        [DataMember(Name = "DataSize")]
        private long dataSize = 0;

        // Properties
        public bool LoadOnDemand
        {
            get { return loadOnDemand; }
        }

        public long DataSize
        {
            get { return dataSize; }
        }

        // Methods
        public async Task<string> GetTextAsync()
        {
            return null;
        }

        public Task<byte[]> GetBytesAsync()
        {
            return null;
        }
    }
}
