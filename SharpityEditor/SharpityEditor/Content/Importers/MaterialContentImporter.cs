using SharpityEditor._Attribute;
using SharpityEngine.Content;
using SharpityEngine.Graphics;
using System.Runtime.Serialization;

namespace SharpityEditor.Content.Importers
{
    [DataContract]
    [ContentImporter(".mat")]
    public sealed class MaterialContentImporter : ContentImporter
    {
        // Methods
        public override Task<object> ReadContentAsync(Stream readStream, IContentReader.ContentReadContext context, CancellationToken cancelToken = default)
        {
            Material result = new Material();
            result.Guid = Guid;
            return Task.FromResult<object>(result);
        }
    }
}
