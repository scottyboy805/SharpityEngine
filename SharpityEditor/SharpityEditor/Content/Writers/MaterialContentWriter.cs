using SharpityEngine.Content;
using SharpityEngine.Graphics;

namespace SharpityEditor.Content.Writers
{
    [ContentWriter(typeof(Material), true)]
    internal sealed class MaterialContentWriter : IContentWriter
    {
        // Properties
        public bool RequireStreamSeeking => false;

        // Methods
        public async Task<bool> WriteContentAsync(object content, Stream writeStream, IContentWriter.ContentWriteContext context, CancellationToken cancelToken)
        {
            return true;
        }
    }
}
