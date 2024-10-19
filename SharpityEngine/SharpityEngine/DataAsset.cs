using System.Runtime.Serialization;
using System.Text;

namespace SharpityEngine
{
    [DataContract]
    public class DataAsset : GameAsset
    {
        // Private
        [DataMember(Name = "DataSize")]
        private long dataSize = 0;
        [DataMember(Name = "Data")]
        private string data = "";

        // Properties
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
            this.data = Encoding.UTF8.GetString(byteData);
        }

        protected internal DataAsset(string textData, string name = null)
            : base(name)
        {
            this.data = textData;
        }

        // Methods
        public string GetText()
        {
            // Get the data
            if (data != null)
                return data;

            // Default to empty string
            return string.Empty;
        }

        public byte[] GetBytes()
        {
            // Get the data decoded
            if(data != null)
                return Encoding.UTF8.GetBytes(data);

            return null;
        }
    }
}
