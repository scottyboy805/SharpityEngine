using Newtonsoft.Json;
using SharpityEditor._Attribute;
using SharpityEngine;
using SharpityEngine.Content;
using System.Reflection;

namespace SharpityEditor.Content
{
    public sealed class ContentDatabase : ContentProvider
    {
        // Type
        private struct ContentInfo
        {
            // Public
            public string ContentGuid;
            public string ContentPath;            
            public string ContentExtension;
            public long LastWriteTime;
        }

        // Private
        private const string metaExtension = ".meta";

        private string projectPath = null;
        private string contentPath = null;
        private string cachePath = null;
        private Dictionary<string, Type> contentImporters = new Dictionary<string, Type>();             // File extension, ContentImporter type
        private Dictionary<string, ContentInfo> guidContent = new Dictionary<string, ContentInfo>();    // Guid, ContentInfo
        private Dictionary<string, string> pathContent = new Dictionary<string, string>();              // ContentPath, Guid

        // Properties
        public string ProjectPath
        {
            get { return projectPath; }
        }

        public string ContentPath
        {
            get { return contentPath; }
        }

        public string CachePath
        {
            get { return cachePath; }
        }

        // Constructor
        public ContentDatabase(string projectFolder, TypeManager typeManager = null)
            : base(projectFolder, typeManager)
        {
            // Check for invalid path
            if (string.IsNullOrEmpty(projectFolder) == true)
                throw new ArgumentException("Project folder cannot be null or empty");

            // Make sure folder exists
            if (Directory.Exists(projectFolder) == false)
                throw new ArgumentException("Project folder must exist");

            this.projectPath = projectFolder;
            contentPath = Path.Combine(projectFolder, "Content");
            cachePath = Path.Combine(projectFolder, "Cache");


            // Create folders if required
            if(Directory.Exists(contentPath) == false) Directory.CreateDirectory(contentPath);
            if(Directory.Exists(cachePath) == false) Directory.CreateDirectory(cachePath);

            // Load importers
            LoadContentImporters();
        }

        // Methods
        protected override Task<ContentReaderInfo> GetReadContentStreamFromPath(string contentPath, Type hintType)
        {
            throw new NotImplementedException();
        }

        protected override Task<ContentWriterInfo> GetWriteContentStreamFromPath(string contentPath, string guid, Type contentType)
        {
            throw new NotImplementedException();
        }

        internal void ScanContent()
        {
            // Search in folder
            foreach(string contentPath in Directory.EnumerateFiles(contentPath, "*.*", SearchOption.AllDirectories))
            {
                // Check for meta
                if (Path.GetExtension(contentPath) == metaExtension)
                    continue;

                // Get relative path
                string contentRelativePath = GetContentRelativePath(contentPath);

                // Get meta path
                string contentMetaPath = Path.ChangeExtension(contentPath, metaExtension);

                // Check for meta file
                if (File.Exists(contentMetaPath) == true)
                {
                    // Load meta
                    ContentImporter importer = GetContentImporter(contentPath);

                    // Get cache path
                    string contentCachePath = Path.Combine(cachePath, importer.Guid);

                    // Check for cached
                    if (File.Exists(contentCachePath) == true)
                    {
                        // Create content entry
                        guidContent[importer.Guid] = new ContentInfo
                        {
                            ContentGuid = importer.Guid,
                            ContentPath = contentRelativePath,
                            ContentExtension = Path.GetExtension(contentPath),
                            LastWriteTime = File.GetLastWriteTimeUtc(contentPath).Ticks,
                        };
                        pathContent[contentPath] = importer.Guid;
                    }
                    else
                        ImportContent(contentRelativePath);
                }
                else
                {
                    // Must be new content which should be imported
                    ImportContent(contentRelativePath);
                }
            }
        }

        public void SaveContent(GameElement content)
        {
            // Check for null
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            // Get the content path
            string contentPath = GetContentPath(content);

            // Check for no path
            if (string.IsNullOrEmpty(contentPath) == true)
                throw new InvalidOperationException("Content does not have a persistent path: " + content);

            // Get content type
            Type contentType = content.GetType();

            // Get non-optimized content writer
            IContentWriter contentWriter = GetContentWriterInstance(contentType, false);

            // Check for writer
            if (contentWriter == null)
                throw new NotSupportedException("No content writer available: " + content);

            // Write the content to disk
            WriteContent(contentWriter, contentPath, contentType, content);

            // Update cache
            SaveCachedContent(contentPath, content);
        }

        internal void SaveCachedContent(string contentPath, GameElement content, long timeStamp = -1, ContentImporter importer = null)
        {
            // Get cache path
            string contentCachePath = Path.Combine(cachePath, content.Guid);

            // Check time stamp - cache may be up to date so no save is required
            if (timeStamp != -1 && File.Exists(contentCachePath) == true && File.GetLastWriteTimeUtc(contentCachePath).Ticks == timeStamp)
                return;

            // Get content type
            Type contentType = content.GetType();

            // Get content writer
            IContentWriter contentWriter = GetContentWriterInstance(contentType, true);

            // Check for writer
            if (contentWriter == null)
            {
                Debug.LogError("No content writer for type: " + contentType);
                return;
            }

            // Write the content to disk
            WriteContent(contentWriter, contentCachePath, contentType, content);

            // Check for no importer provided
            if (importer == null)
            {
                // Get content importer
                importer = GetContentImporter(contentPath);

                // Check for importer
                if (importer == null)
                {
                    Debug.LogWarning("No importer available for content: " + contentPath);
                    return;
                }

                // Set importer guid
                if(string.IsNullOrEmpty(importer.Guid) == true)
                    importer.Guid = content.Guid;
            }

            // Write meta
            WriteContentImporterMeta(importer, contentPath);
        }

        public string GetContentGuid(string contentPath)
        {
            // Check for content found
            string guid;
            if (pathContent.TryGetValue(contentPath, out guid) == false)
                return null;

            return guid;
        }

        public string GetContentPath(GameElement content)
        {
            // Check for null
            if(content == null)
                throw new ArgumentNullException(nameof(content));

            // Get load path
            ContentInfo info;
            if (guidContent.TryGetValue(content.Guid, out info) == false)
                return null;

            // Get path
            return info.ContentPath;
        }

        public string GetContentRelativePath(string contentPath)
        {
            // Get relative path
            string relativeContentPath = Path.GetRelativePath(projectPath, contentPath);

            // Convert to forward slash
            if (string.IsNullOrEmpty(relativeContentPath) == false)
                relativeContentPath = relativeContentPath.Replace('\\', '/');

            return relativeContentPath;
        }

        public void CreateContent(string contentPath, GameElement content)
        {
            // Check for null
            if(content == null)
                throw new ArgumentNullException(nameof(content));

            // Check relative path
            contentPath = CheckProjectContentRelativePath(contentPath);
        }

        public GameElement ImportContent(string contentPath)
        {
            // Store relative path
            string contentRelativePath = contentPath;

            // Check relative path
            contentPath = CheckProjectContentRelativePath(contentPath);

            // Check for file exists
            if (File.Exists(contentPath) == false)
                throw new ArgumentException("Content path does not exist");

            // Get content importer
            ContentImporter importer = GetContentImporter(contentPath);

            // Check for importer
            if(importer == null)
            {
                Debug.LogWarning("No importer available for content: " + contentPath);
                return null;
            }

            // Import the content file
            GameElement result = ReadContent(importer, contentPath, importer.Guid);

            // Apply guid
            if (string.IsNullOrEmpty(importer.Guid) == true)
                importer.Guid = result.Guid;

            // Get time stamp of content file
            long timeStamp = File.GetLastWriteTimeUtc(contentPath).Ticks;

            // Create content entry
            guidContent[importer.Guid] = new ContentInfo
            {
                ContentGuid = importer.Guid,
                ContentPath = contentRelativePath,
                ContentExtension = Path.GetExtension(contentRelativePath),
                LastWriteTime = timeStamp,
            };
            pathContent[contentRelativePath] = importer.Guid;

            // Cache the content
            SaveCachedContent(contentPath, result, timeStamp, importer);

            return result;
        }

        public T GetContentImporter<T>(string contentPath) where T : ContentImporter
        {
            return GetContentImporter(contentPath) as T;
        }

        public ContentImporter GetContentImporter(string contentPath)
        {
            // Get full project path
            contentPath = GetProjectContentPath(contentPath);

            // Get extension
            string ext = Path.GetExtension(contentPath);

            // Check for extension
            if (string.IsNullOrEmpty(ext) == true)
                throw new ArgumentException("Content path must have a file extension");

            // Try to get importer
            Type importerType;
            if (contentImporters.TryGetValue(ext.ToLower(), out importerType) == false)
                return null;

            // Create importer
            ContentImporter importer = (ContentImporter)Activator.CreateInstance(importerType);

            // Read importer metadata
            ReadContentImporterMeta(importer, contentPath);

            return importer;
        }

        private void ReadContentImporterMeta(ContentImporter importer, string contentPath, bool createMeta = true)
        {
            // Check for null
            if (importer == null)
                return;

            // Check for meta file
            string metaFullPath = Path.ChangeExtension(contentPath, metaExtension);

            // Check for meta file
            if (File.Exists(metaFullPath) == true)
            {
                // Create import context
                IContentReader.ContentReadContext metaContext = new IContentReader.ContentReadContext(
                    importer.GetType(), this, null, importer.Guid, metaFullPath);

                // Create json reader
                JsonDeserializeFormatter jsonDeserializer = new JsonDeserializeFormatter(metaContext);

                // Read json
                using (JsonReader reader = new JsonTextReader(File.OpenText(metaFullPath)))
                {
                    // Load settings for importer
                    jsonDeserializer.DeserializeObjectExisting(reader, importer)
                        .Wait();
                }
            }
            else
            {
                // Need to create the meta file
            }
        }

        private void WriteContentImporterMeta(ContentImporter importer, string contentPath)
        {
            // Check for null
            if (importer == null)
                return;

            // Check for meta file
            string metaFullPath = Path.ChangeExtension(contentPath, metaExtension);

            // Create write context
            IContentWriter.ContentWriteContext metaContext = new IContentWriter.ContentWriteContext(
                importer.GetType(), this, null, importer.Guid, metaFullPath);

            // Create json writer
            JsonSerializeFormatter jsonSerializer = new JsonSerializeFormatter(metaContext);

            // Write json
            using (JsonWriter writer = new JsonTextWriter(File.CreateText(metaFullPath)))
            {
                // Save settings for importer
                jsonSerializer.SerializeObject(writer, importer);
            }
        }

        private string GetProjectContentPath(string contentPath)
        {
            // Check for rooted
            if (Path.IsPathRooted(contentPath) == false)
                contentPath = Path.GetFullPath(contentPath);

            // Get full path
            return Path.Combine(projectPath, contentPath);
        }

        private string CheckProjectContentRelativePath(string contentPath)
        {
            // Check for invalid
            if (string.IsNullOrEmpty(contentPath) == true)
                throw new ArgumentException("Content path cannot be null or empty");

            // Check for rooted
            if (Path.IsPathRooted(contentPath) == true)
                throw new ArgumentException("Content path must be relative to the project folder");

            // Get full path
            string fullPath = Path.Combine(projectPath, contentPath);

            // Check for separator
            if (Path.DirectorySeparatorChar == '\\')
                fullPath = fullPath.Replace('/', '\\');

            return fullPath;
        }

        private GameElement ReadContent(IContentReader reader, string contentPath, string guid = "")
        {
            // Import the content file
            object result = null;
            using (Stream contentStream = File.OpenRead(contentPath))
            {
                // Create read context
                IContentReader.ContentReadContext context = new IContentReader.ContentReadContext(
                    null, this, null, guid, contentPath);

                // Import the asset
                result = reader.ReadContentAsync(contentStream, context, default)
                    .Result;
            }

            // Get element
            return result as GameElement;
        }

        private void WriteContent(IContentWriter writer, string contentPath, Type contentType, GameElement content)
        {
            // Open cache file
            using (Stream stream = File.Create(contentPath))
            {
                // Create context
                IContentWriter.ContentWriteContext context = new IContentWriter.ContentWriteContext(
                    contentType, this, null, content.Guid, contentPath);

                // Write file
                writer.WriteContentAsync(content, stream, context, default);
            }
        }

        private void LoadContentImporters()
        {
            // Get this assembly name
            AssemblyName thisAssembly = Assembly.GetExecutingAssembly().GetName();

            try
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // Check if we are scanning an external assembly
                    if (assembly != Assembly.GetExecutingAssembly())
                    {
                        // Check if the assembly references sharpity
                        AssemblyName[] referenceNames = assembly.GetReferencedAssemblies();
                        bool referenced = false;

                        foreach (AssemblyName assemblyName in referenceNames)
                        {
                            if (thisAssembly.FullName == assemblyName.FullName)
                            {
                                referenced = true;
                                break;
                            }
                        }

                        // Check for referenced
                        if (referenced == false)
                            continue;
                    }

                    foreach (Type type in assembly.GetTypes())
                    {
                        foreach (ContentImporterAttribute attrib in type.GetCustomAttributes<ContentImporterAttribute>())
                        {
                            // Check for derived type
                            if (typeof(ContentImporter).IsAssignableFrom(type) == false)
                            {
                                Debug.LogError(LogFilter.Content, "Content importer must derive from `ContentImporter`: " + type);
                                continue;
                            }

                            // Get extension
                            string ext = attrib.FileExtension.ToLower();

                            // Check for overwrite content importer
                            if (contentImporters.ContainsKey(ext) == true)
                            {
                                Debug.LogErrorF(LogFilter.Content, "A content importer already exists for extension `{0}`: {1}", type, attrib.FileExtension);
                                continue;
                            }

                            // Store reader type
                            contentImporters[ext] = type;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
