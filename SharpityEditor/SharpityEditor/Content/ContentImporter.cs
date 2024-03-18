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

        // Properties
        public virtual bool RequireStreamSeeking => false;

        public string Guid
        {
            get { return guid; }
            internal set { guid = value; }
        }

        // Methods
        public abstract Task<object> ReadContentAsync(Stream readStream, IContentReader.ContentReadContext context, CancellationToken cancelToken = default);
    }
}
