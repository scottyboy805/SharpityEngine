using LiteDB;
using Newtonsoft.Json;
using SharpityEngine;
using SharpityEngine.Content;
using System.Reflection;
using JsonReader = Newtonsoft.Json.JsonReader;
using JsonWriter = Newtonsoft.Json.JsonWriter;

namespace SharpityEditor.Content
{
    public sealed class ContentDatabase : ContentProvider
    {
        // Type
        private sealed class ContentInfo
        {
            // Public
            public int Id { get; set; }
            //[BsonId(false)]
            public string Guid { get; set; }
            public string Name { get; set; }
            public string Folder { get; set; }
            public string Path { get; set; }
            public string Extension { get; set; }
            public string Type { get; set; }
            public string[] Tags { get; set; }
            public long LastWriteTimeUTC { get; set; }

            // Constructor
            internal ContentInfo() { }
            internal ContentInfo(string path, string folder, string guid, string type)
            {
                this.Guid = guid;
                Update(path, folder, type);   
            }

            // Methods
            internal void Update(string path, string folder, string type)
            {
                this.Name = System.IO.Path.GetFileNameWithoutExtension(path);
                this.Folder = folder;
                this.Path = path;
                this.Extension = System.IO.Path.GetExtension(path);
                this.Type = type;
                this.LastWriteTimeUTC = File.GetLastWriteTimeUtc(path).Ticks;
            }
        }

        // Private
        private const string metaExtension = ".meta";
        private const string dbName = "Content.db";
        private const string dbContentCollection = "ContentsInfo";
        private const string dbContentDataCollection = "ContentsData";         // Stores file data for imported content
            
        private string projectPath = null;
        private string projectContentPath = null;
        private string projectCachePath = null;
        private LiteDatabase contentDB = null;
        private ILiteCollection<ContentInfo> contentInfoDB = null;
        private ILiteStorage<string> contentDataDB = null;
        private Dictionary<string, Type> contentImporters = new Dictionary<string, Type>();             // File extension, ContentImporter type

        // Properties
        public string ProjectPath
        {
            get { return projectPath; }
        }

        public string ContentPath
        {
            get { return projectContentPath; }
        }

        public string CachePath
        {
            get { return projectCachePath; }
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
            projectContentPath = Path.Combine(projectFolder, "Content");
            projectCachePath = Path.Combine(projectFolder, "Cache");            

            // Create folders if required
            if(Directory.Exists(projectContentPath) == false) Directory.CreateDirectory(projectContentPath);
            if(Directory.Exists(projectCachePath) == false) Directory.CreateDirectory(projectCachePath);

            // Create content database
            contentDB = new LiteDatabase(Path.Combine(projectCachePath, dbName));
            
            // Get the content info set
            contentInfoDB = contentDB.GetCollection<ContentInfo>(dbContentCollection);
            contentDataDB = contentDB.GetStorage<string>(dbContentDataCollection);

            // Load importers
            LoadContentImporters();
        }

        // Methods
        public override void Dispose()
        {
            base.Dispose();

            // Release database
            contentDB.Dispose();
            contentDB = null;
        }

        protected override Task<ContentReaderInfo> GetReadContentStreamFromPath(string contentPath, Type hintType)
        {
            throw new NotImplementedException();
        }

        protected override Task<ContentWriterInfo> GetWriteContentStreamFromPath(string contentPath, string guid, Type contentType)
        {
            throw new NotImplementedException();
        }

        internal void SyncContentOnDisk()
        {
            // Store all guids that have been found on disk
            HashSet<string> guidsOnDisk = new HashSet<string>();

            // Search in folder
            foreach(string contentPath in Directory.EnumerateFiles(projectContentPath, "*.*", SearchOption.AllDirectories))
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

                    // Check for already registered
                    if (HasContent(importer.Guid) == true)
                    {
                        // Register or update the content
                        RegisterOrUpdateContentDB(contentRelativePath, importer.Guid, importer.Type);
                    }
                    else
                    {
                        // Meta file exists but no cached content, so import is still required
                        ImportContent(contentRelativePath);
                    }

                    // Add entry
                    guidsOnDisk.Add(importer.Guid);
                }
                else
                {
                    // Must be new content which should be imported
                    ImportContent(contentRelativePath);
                }
            }

            // Remove any dead content from the database - content that has been deleted while the project was closed
            contentInfoDB.DeleteMany(
                i => guidsOnDisk.Contains(i.Guid) == false);
        }

        public void CreateContent(string contentPath, GameElement content)
        {
            // Check for null
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            // Check relative path
            CheckProjectContentRelativePath(contentPath);

            // Get content full path
            string contentFullPath = Path.Combine(projectPath, contentPath);

            // Check for already exists
            if (File.Exists(contentFullPath) == true)
                throw new ArgumentException("Content path already exists");

            // Check for registered
            if (HasContentPath(contentPath) == true)
                throw new InvalidOperationException("Content path is already registered");

            // Register the content with path
            RegisterContentDB(contentPath, content.Guid, content.Type);

            // Save the changes to the content
            SaveContent(content);
        }

        public GameElement ImportContent(string contentPath)
        {
            // Check relative path
            CheckProjectContentRelativePath(contentPath);

            // Get full path
            string contentFullPath = Path.Combine(projectPath, contentPath);

            // Check for file exists
            if (File.Exists(contentFullPath) == false)
                throw new ArgumentException("Content path does not exist");

            // Get content importer
            ContentImporter importer = GetContentImporter(contentFullPath);

            // Check for importer
            if (importer == null)
            {
                Debug.LogWarning("No importer available for content: " + contentPath);
                return null;
            }

            // Import the content file
            GameElement result = ReadContent(importer, contentFullPath, importer.Guid);

            // Update importer
            importer.UpdateImporterFromContent(result);

            // Register the new entry
            ContentInfo importedInfo = RegisterOrUpdateContentDB(contentPath, importer.Guid, importer.Type);

            // Get last write time
            long lastWriteTime = File.GetLastWriteTimeUtc(contentFullPath).Ticks;

            // Cache the content
            SaveCachedContent(importedInfo, result, lastWriteTime, importer);

            return result;
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

            // Get the content info
            ContentInfo contentInfo = contentInfoDB.FindOne(
                Query.EQ(nameof(ContentInfo.Guid), content.Guid));

            // Check for not found
            if (contentInfo == null)
                throw new InvalidOperationException("Cannot save unregistered content. You should first use CreateContent or ImportContent before saving");

            // Get content type
            Type contentType = content.GetType();

            // Get non-optimized content writer
            IContentWriter contentWriter = GetContentWriterInstance(contentType, false);

            // Check for writer
            if (contentWriter == null)
                throw new NotSupportedException("No content writer available: " + content);

            // Write the content to disk
            WriteContent(contentWriter, contentPath, contentType, content);

            // Get write time
            long lastWriteTime = File.GetLastAccessTimeUtc(contentPath).Ticks;

            // Update cache
            SaveCachedContent(contentInfo, content, lastWriteTime);
        }

        private void SaveCachedContent(ContentInfo contentInfo, GameElement content, long timeStamp, ContentImporter importer = null)
        {
            // Check time stamp - db may be up to date so no save is required
            if (contentInfo.LastWriteTimeUTC == timeStamp && File.Exists(importer.MetaPath) == true)
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

            // Write the content to the database
            WriteContentDB(contentWriter, contentInfo, content, contentType);

            // Update database write time
            contentInfo.LastWriteTimeUTC = timeStamp;
            contentInfoDB.Update(contentInfo);

            // Check for no importer provided
            if (importer == null)
            {
                // Get content importer
                importer = GetContentImporter(projectContentPath);

                // Check for importer
                if (importer == null)
                {
                    Debug.LogWarning("No importer available for content: " + projectContentPath);
                    return;
                }

                // Update importer
                importer.UpdateImporterFromContent(content);
            }

            // Write meta
            WriteContentImporterMeta(importer, importer.MetaPath);
        }

        public bool HasContent(string guid)
        {
            // Check for content with guid exists
            return contentInfoDB.Exists(
                Query.EQ(nameof(ContentInfo.Guid), guid));
        }

        public bool HasContent(GameElement content)
        {
            return HasContent(content.Guid);
        }

        public bool HasContentPath(string contentPath)
        {
            // Check relative path
            CheckProjectContentRelativePath(contentPath);

            // Check for exists
            return contentInfoDB.Exists(
                Query.EQ(nameof(ContentInfo.Path), contentPath));
        }

        public string GetContentGuid(string contentPath)
        {
            // Check relative path
            CheckProjectContentRelativePath(contentPath);

            // Find the entry
            ContentInfo result = contentInfoDB.FindOne(
                Query.EQ(nameof(ContentInfo.Path), contentPath));

            // Check for content found
            if (result != null)
                return result.Guid;

            return null;
        }

        public string GetContentPath(string guid)
        {
            // Check for invalid guid
            if (string.IsNullOrEmpty(guid) == true)
                throw new ArgumentException("Guid cannot be null or empty");

            // Find the entry
            ContentInfo result = contentInfoDB.FindOne(
                Query.EQ(nameof(ContentInfo.Guid), guid));

            // Check for content found
            if (result != null)
                return result.Path;

            return null;
        }

        public string GetContentPath(GameElement content)
        {
            // Check for null
            if(content == null)
                throw new ArgumentNullException(nameof(content));

            // Find by guid
            return GetContentPath(content.Guid);
        }

        public string GetContentRelativePath(string contentPath)
        {
            // Get relative path
            string relativeContentPath = Path.GetRelativePath(projectPath, contentPath);
            
            // Normalize the path to use the standard convention - forward slash only
            return NormalizeContentPath(relativeContentPath);
        }

        private string NormalizeContentPath(string contentPath)
        {
            // Convert to forward slash
            if (string.IsNullOrEmpty(contentPath) == false)
                contentPath = contentPath.Replace('\\', '/');

            return contentPath;
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

        private ContentInfo RegisterOrUpdateContentDB(string contentPath, string guid, string type)
        {
            // Check for exists
            if(HasContent(guid) == true)
            {
                // Find the entry
                ContentInfo result = contentInfoDB.FindOne(
                    Query.EQ(nameof(ContentInfo.Guid), guid));

                // Get the folder
                string folderPath = Directory.GetParent(contentPath).FullName;

                // Make relative
                string folderPathRelative = GetContentRelativePath(folderPath);

                // Update the entry
                result.Update(contentPath, folderPathRelative, type);

                // Update database
                contentInfoDB.Update(result);

                return result;
            }
            else
            {
                // Register new content
                return RegisterContentDB(contentPath, guid, type);
            }
        }

        private ContentInfo RegisterContentDB(string contentPath, string guid, string type)
        {
            // Get the folder
            string folderPath = Directory.GetParent(contentPath).FullName;

            // Make relative
            string folderPathRelative = GetContentRelativePath(folderPath);

            // Create content info
            ContentInfo info = new ContentInfo(contentPath, folderPathRelative, guid, type);

            // Make sure index is unique
            contentInfoDB.EnsureIndex(nameof(ContentInfo.Guid), true);

            // Register with db
            contentInfoDB.Insert(info);

            return info;
        }

        private void ReadContentImporterMeta(ContentImporter importer, string contentPath, bool createMeta = true)
        {
            // Check for null
            if (importer == null)
                return;

            // Check for meta file
            string metaFullPath = Path.ChangeExtension(contentPath, metaExtension);

            // Update importer path
            importer.MetaPath = metaFullPath;

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

        private void CheckProjectContentRelativePath(string contentPath)
        {
            // Check for invalid
            if (string.IsNullOrEmpty(contentPath) == true)
                throw new ArgumentException("Content path cannot be null or empty");

            // Check for rooted
            if (Path.IsPathRooted(contentPath) == true)
                throw new ArgumentException("Content path must be relative to the project folder");

            // Check for windows separator
            if (contentPath.Contains('\\') == true)
                throw new ArgumentException("Content path must use '/' forward separators");
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

        private void WriteContentDB(IContentWriter writer, ContentInfo contentInfo, GameElement content, Type contentType)
        {
            // Open cache file
            using (MemoryStream stream = new MemoryStream())
            {
                // Create context
                IContentWriter.ContentWriteContext context = new IContentWriter.ContentWriteContext(
                    contentType, this, null, content.Guid, projectContentPath);

                // Write file
                writer.WriteContentAsync(content, stream, context, default);

                // Upload to storage
                contentDataDB.Upload(content.Guid, projectContentPath, stream);
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
