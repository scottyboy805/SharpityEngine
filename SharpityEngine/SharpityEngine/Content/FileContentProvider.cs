
namespace SharpityEngine.Content
{
    internal sealed class FileContentProvider : ContentProvider
    {
        // Constructor
        public FileContentProvider(string contentRootPath, TypeManager typeManager = null)
            : base(contentRootPath, typeManager)
        {
            // Check for error
            if (Directory.Exists(contentRootPath) == false)
                throw new ArgumentException("Content path must be a valid folder that already exists!");
        }

        // Methods
        protected override Task<ContentReaderInfo> GetReadContentStreamFromPath(string contentPath, Type hintType)
        {
            // Check for extension
            bool hasExtension = Path.HasExtension(contentPath);

            // Check for allow extension
            if (hasExtension == true && File.Exists(contentPath) == true)
            {
                return Task.FromResult(new ContentReaderInfo
                {
                    // Open the read stream
                    Stream = File.OpenRead(contentPath),

                    // Create reader
                    Reader = GetContentReaderInstance(Path.GetExtension(contentPath).ToLower()),

                    // Create context
                    Context = new IContentReader.ContentReadContext(hintType, this, 
                        null, null, contentPath),           
                });
            }

            // Catch path exceptions
            try
            {
                // Try to find content
                IEnumerable<string> assetFiles = (hasExtension == true)
                    ? Directory.EnumerateFiles(contentRootPath, contentPath)
                    : Directory.EnumerateFiles(contentRootPath, contentPath + ".*");

                // Check for any
                foreach (string assetFile in assetFiles)
                {
                    return Task.FromResult(new ContentReaderInfo
                    {
                        // Open the read stream
                        Stream = File.OpenRead(assetFile),

                        // Create reader
                        Reader = GetContentReaderInstance(Path.GetExtension(assetFile).ToLower()),

                        // Create context
                        Context = new IContentReader.ContentReadContext(hintType, this,
                            null, null, assetFile),
                    });
                }
            }
            catch { }

            return Task.FromResult(default(ContentReaderInfo));
        }

        protected override Task<ContentWriterInfo> GetWriteContentStreamFromPath(string contentPath, string guid, Type contentType)
        {
            return Task.FromResult(new ContentWriterInfo
            {
                // Open the write stream
                Stream = File.Create(contentPath),

                // Create writer
                Writer = GetContentWriterInstance(contentType, false),

                // Create context
                Context = new IContentWriter.ContentWriteContext(contentType, this, 
                    null, guid, contentPath),
            }); 
        }
    }
}
