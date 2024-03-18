
using SharpityEngine.Graphics;

namespace SharpityEngine.Content
{
    public interface IContentWriter
    {
        // Type
        public struct ContentWriteContext
        {
            // Private
            private Game game;

            // Properties
            public GraphicsDevice GraphicsDevice
            {
                get { return game.GraphicsDevice; }
            }

            // Public
            public Type ContentType;
            public ContentProvider ContentProvider;
            public ContentBundle ContainingBundle;
            public string ContentGuid;
            public string ContentPath;
            public string ContentName;
            public string ContentExtension;
            public bool IsDependency;

            // Constructor
            internal ContentWriteContext(Type contentType, ContentProvider provider, ContentBundle bundle, string guid, string path, bool dependency = false)
            {
                this.game = Game.Current;
                this.ContentType = contentType;
                this.ContentProvider = provider;
                this.ContainingBundle = bundle;
                this.ContentGuid = guid;
                this.ContentPath = path;
                this.ContentName = Path.GetFileNameWithoutExtension(path);
                this.ContentExtension = Path.GetExtension(path);
                this.IsDependency = dependency;
            }
        }

        // Properties
        bool RequireStreamSeeking { get; }

        // Methods
        Task<bool> WriteContentAsync(object content, Stream writeStream, ContentWriteContext context, CancellationToken cancelToken);
    }
}
