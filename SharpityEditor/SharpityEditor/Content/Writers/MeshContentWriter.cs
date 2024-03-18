using SharpityEngine.Content;
using SharpityEngine.Graphics;

namespace SharpityEditor.Content.Writers
{
    [ContentWriter(typeof(Mesh), true)]
    internal sealed class MeshContentWriter : IContentWriter
    {
        // Properties
        public bool RequireStreamSeeking => false;

        // Methods
        public async Task<bool> WriteContentAsync(object content, Stream writeStream, IContentWriter.ContentWriteContext context, CancellationToken cancelToken)
        {
            // Get mesh
            Mesh mesh = (Mesh)content;

            return true;
        }
    }
}
