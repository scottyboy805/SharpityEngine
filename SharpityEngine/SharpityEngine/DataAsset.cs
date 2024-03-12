using System.Runtime.Serialization;
using System.Text;

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

        private byte[] dataBytes = null;

        // Properties
        public bool LoadOnDemand
        {
            get { return loadOnDemand; }
        }

        public long DataSize
        {
            get { return dataSize; }
        }

        // Constructor
        internal DataAsset(string name = null)
            : base(name)
        {
        }

        protected internal DataAsset(byte[] byteData, string name = null)
            : base(name)
        {
            this.dataBytes = byteData;
        }

        protected internal DataAsset(string textData, string name = null)
            : base(name)
        {
            this.dataBytes = Encoding.UTF8.GetBytes(textData);
        }

        // Methods
        protected override void OnLoaded()
        {
            // Check for load on demand
            if (loadOnDemand == false)
                GetBytesAsync();
        }

        public async Task<string> GetTextAsync()
        {
            // Check for runtime created
            if(dataBytes != null)
                return Encoding.UTF8.GetString(dataBytes);

            return null;
        }

        public async Task<byte[]> GetBytesAsync()
        {
            // Check for runtime created
            if (dataBytes != null)
                return dataBytes;

            return null;
        }
    }
}
