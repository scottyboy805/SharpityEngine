
using SharpityEngine.Graphics;

namespace SharpityEngine.Content
{
    public interface IContentReader
    {
        // Type
        public struct ContentReadContext
        {
            // Private
            private Game game;

            // Public
            public Type HintType;            
            public ContentProvider ContentProvider;
            public ContentBundle ContainingBundle;
            public string ContentGuid;
            public string ContentPath;
            public string ContentName;
            public string ContentExtension;
            public long ContentSize;
            public bool IsDependency;

            // Properties
            public GraphicsDevice GraphicsDevice
            {
                get { return game.GraphicsDevice; }
            }

            // Constructor
            internal ContentReadContext(Type hintType, ContentProvider provider, ContentBundle bundle, string guid, string path, long size = -1, bool dependency = false)
            {
                this.game = Game.Current;
                this.HintType = hintType;
                this.ContentProvider = provider;
                this.ContainingBundle = bundle;
                this.ContentGuid = guid;
                this.ContentPath = path;
                this.ContentName = Path.GetFileNameWithoutExtension(path);
                this.ContentExtension = Path.GetExtension(path);
                this.ContentSize = size;
                this.IsDependency = dependency;
            }
        }

        // Private
        bool RequireStreamSeeking { get; }

        // Methods
        Task<object> ReadContentAsync(Stream readStream, in ContentReadContext context, CancellationToken cancelToken);
    }
}
