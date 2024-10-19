using SharpityEngine;
using SharpityEngine.Content;
using System.Runtime.Serialization;

namespace SharpityEditor.Content
{
    [DataContract]
    public abstract class ContentImporter : IContentReader
    {
        // Private
        [DataMember(Name = "Guid")]
        private string guid = "";
        [DataMember(Name = "Type")]
        private string type = "";

        private string metaPath = null;

        // Properties
        public virtual bool RequireStreamSeeking => false;

        internal string MetaPath
        {
            get { return metaPath; }
            set {  metaPath = value; }
        }

        public string Guid
        {
            get { return guid; }
        }

        public string Type
        {
            get { return type; }
        }

        // Methods
        public abstract Task<object> ReadContentAsync(Stream readStream, IContentReader.ContentReadContext context, CancellationToken cancelToken = default);

        internal void UpdateImporterFromContent(GameElement content)
        {
            // Check for null
            if (content == null)
                return;

            // Update guid
            if (string.IsNullOrEmpty(guid) == true)
                guid = content.Guid;

            // Update type
            if(string.IsNullOrEmpty(type) == true)
                type = content.Type;
        }
    }
}
