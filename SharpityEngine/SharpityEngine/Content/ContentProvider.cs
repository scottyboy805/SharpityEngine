using System.Collections.Concurrent;
using System.Reflection;
using Newtonsoft.Json;
using SharpityEngine.Scene;

namespace SharpityEngine.Content
{
    public abstract class ContentProvider : IDisposable
    {
        // Type
        protected internal struct ContentReaderInfo
        {
            // Public
            public Stream stream;
            public IContentReader reader;
            public IContentReader.ContentReadContext context;
        }

        // Internal
        internal TypeManager typeManager = null;

        // Private
        private Dictionary<string, Type> contentReaders = new Dictionary<string, Type>();   // Extension, IContentReader type
        private ConcurrentDictionary<string, GameElement> contentPathCache = new ConcurrentDictionary<string, GameElement>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, GameElement> contentGuidCache = new ConcurrentDictionary<string, GameElement>();

        // Internal
        internal ConcurrentDictionary<string, ContentBundle> contentBundleGuidAssetLookup = new ConcurrentDictionary<string, ContentBundle>();

        // Protected
        protected string contentRootPath = "";

        // Internal
        internal List<ContentBundle> contentBundles = new List<ContentBundle>();

        // Properties
        public string ContentFolder
        {
            get { return contentRootPath; }
        }

        public TypeManager TypeManager
        {
            get { return typeManager; }
        }

        // Constructor
        public ContentProvider(string contentRootPath, TypeManager typeManager = null)
        {
            // Check for type manager
            if (typeManager == null)
                typeManager = new TypeManager();
            
            this.contentRootPath = contentRootPath;
            this.typeManager = typeManager;

            Debug.Log(LogFilter.Content, "Initialize content directory: " + contentRootPath);

            // Load readers
            LoadContentReaders();
        }

        // Methods
        public void Save(GameElement element, string file)
        {
            string savePath = file;

            // Get output path
            if(Path.IsPathRooted(file) == false)
                savePath = Path.Combine(contentRootPath, file);

            JsonSerializeFormatter formatter = new JsonSerializeFormatter(typeManager);

            using (JsonWriter writer = new JsonTextWriter(File.CreateText(savePath)))
            {
                // Before save
                if (element is IContentCallback)
                    ((IContentCallback)element).OnBeforeContentSave();
                
                writer.Formatting = Formatting.Indented;
                formatter.SerializeObject(writer, element);
            }
        }

        public GameScene LoadScene(string contentPathOrGuid, bool autoActivate = true, ContentBundle bundle = null)
        {
            Debug.Log(LogFilter.Content, "Load scene: " + contentPathOrGuid);

            // issue load request
            GameScene scene = Load<GameScene>(contentPathOrGuid, bundle);

            // Check for success
            //if (scene != null && autoActivate == true)
            //    scene.Activate();

            return scene;
        }

        public async Task<GameScene> LoadSceneAsync(string contentPathOrGuid, bool autoActivate = true, ContentBundle bundle = null)
        {
            Debug.Log(LogFilter.Content, "Load scene async: " + contentPathOrGuid);

            // Issue load request
            GameScene scene = await LoadAsync<GameScene>(contentPathOrGuid, bundle);

            // Check for success
            //if (scene != null && autoActivate == true)
            //    scene.Activate();

            return scene;
        }

        public T Load<T>(string contentPathOrGuid, ContentBundle bundle = null) where T : class
        {
            // Load request
            GameElement result = Load(contentPathOrGuid, bundle, typeof(T));

            // Get as T
            if (result is T)
                return result as T;

            // Check for element
            if (result is GameObject)
                return ((GameObject)result).GetComponent<T>();

            return null;
        }

        public async Task<T> LoadAsync<T>(string contentPathOrGuid, ContentBundle bundle = null) where T : class
        {
            // Load request
            GameElement result = await LoadAsync(contentPathOrGuid, bundle, typeof(T));

            // Get as T
            if (result is T)
                return result as T;

            // Check for element
            if (result is GameObject)
                return ((GameObject)result).GetComponent<T>();

            return null;
        }

        public GameElement Load(string contentPathOrGuid, ContentBundle bundle = null, Type type = null)
        {
            // Check for invalid
            if (string.IsNullOrEmpty(contentPathOrGuid) == true)
                throw new ArgumentException("Content path cannot be null or empty");

            // Check for cached
            GameElement result;
            if (GetCachedContent(contentPathOrGuid, out result) == true)
                return result;

            Debug.Log(LogFilter.Content, "Load content: " + contentPathOrGuid);

            // Check for null type
            if (type == null)
                type = typeof(GameElement);

            // Get stream
            ContentReaderInfo readerInfo;
            Task<ContentReaderInfo> streamTask = GetContentStream(contentPathOrGuid, bundle, type);

            if (streamTask.IsFaulted == true)
                Debug.LogException(streamTask.Exception);

            // Wait for stream to load
            streamTask.Wait();
            readerInfo = streamTask.Result;

            // Check for valid stream
            if (readerInfo.stream == null)
            {
                Debug.LogError(LogFilter.Content, "Could not find content: " + contentPathOrGuid);
                return null;
            }

            // Make sure stream is disposed
            using (readerInfo.stream)
            {
                // Check for error
                if (readerInfo.reader == null)
                    throw new NotSupportedException("No content reader for file format: " + contentPathOrGuid);
                
                // Check for game element reader - Block main thread during load
                result = readerInfo.reader.ReadContentAsync(GetRequiredContentStream(readerInfo.stream, readerInfo.reader), readerInfo.context, default)
                    .Result as GameElement;

                // Check for success
                if (result != null)
                {
                    result.ContentPath = contentPathOrGuid;
                    result.ContentBundle = readerInfo.context.ContainingBundle;
                }
                else
                    Debug.LogError(LogFilter.Content, "Could not load content: " + contentPathOrGuid);

                // Run load events
                if (result is IContentCallback)
                    ((IContentCallback)result).OnAfterContentLoad();

                // Cache the loaded content
                return AddCachedContent(result, contentPathOrGuid);
            }
        }

        public async Task<GameElement> LoadAsync(string contentPathOrGuid, ContentBundle bundle = null, Type type = null, CancellationToken cancelToken = default)
        {
            // Check for invalid
            if (string.IsNullOrEmpty(contentPathOrGuid) == true)
                throw new ArgumentException("Content path cannot be null or empty");

            // Check for cached
            GameElement result;
            if (GetCachedContent(contentPathOrGuid, out result) == true)
                return result;

            Debug.Log(LogFilter.Content, "Load content async: " + contentPathOrGuid);

            // Check for null type
            if (type == null)
                type = typeof(GameElement);

            // Get stream
            ContentReaderInfo readerInfo = await GetContentStream(contentPathOrGuid, bundle, type);

            // Check for valid stream
            if (readerInfo.stream == null)
            {
                Debug.LogError(LogFilter.Content, "Could not find content: " + contentPathOrGuid);
                return null;
            }

            // Make sure stream is disposed
            using (readerInfo.stream)
            {
                // Check for error
                if (readerInfo.reader == null)
                    throw new NotSupportedException("No content reader for file format: " + contentPathOrGuid);

                // Check for game element importer
                result = await readerInfo.reader.ReadContentAsync(GetRequiredContentStream(readerInfo.stream, readerInfo.reader), readerInfo.context, cancelToken) as GameElement;


                // Check for success
                if (result != null)
                {
                    result.ContentPath = contentPathOrGuid;
                    result.ContentBundle = readerInfo.context.ContainingBundle;
                }
                else
                    Debug.LogError(LogFilter.Content, "Could not load content: " + contentPathOrGuid);

                // Run load events
                if (result is IContentCallback)
                    ((IContentCallback)result).OnAfterContentLoad();

                // Cache the loaded content
                return AddCachedContent(result, contentPathOrGuid);
            }
        }

        public string LoadDataText(string contentPathOrGuid, ContentBundle bundle = null)
        {
            using (Stream contentStream = LoadDataStream(contentPathOrGuid, bundle))
            {
                // Check for stream not found
                if (contentStream == null)
                    return null;

                using (StreamReader reader = new StreamReader(contentStream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public async Task<string> LoadDataTextAsync(string contentPathOrGuid, ContentBundle bundle = null)
        {
            using(Stream contentStream = await LoadDataStreamAsync(contentPathOrGuid, bundle))
            {
                // Check for stream not found
                if (contentStream == null)
                    return null;

                using(StreamReader reader = new StreamReader(contentStream))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public byte[] LoadDataBytes(string contentPathOrGuid, ContentBundle bundle = null)
        {
            using(Stream contentStream = LoadDataStream(contentPathOrGuid, bundle))
            {
                MemoryStream memory = new MemoryStream();

                // Copy data
                contentStream.CopyTo(memory);

                return memory.ToArray();
            }
        }

        public async Task<byte[]> LoadDataBytesAsync(string contentPathOrGuid, ContentBundle bundle = null)
        {
            using (Stream contentStream = await LoadDataStreamAsync(contentPathOrGuid, bundle))
            {
                MemoryStream memory = new MemoryStream();

                // Copy data
                await contentStream.CopyToAsync(memory);

                return memory.ToArray();
            }
        }

        public Stream LoadDataStream(string contentPathOrGuid, ContentBundle bundle = null)
        {
            // Check for invalid
            if (string.IsNullOrEmpty(contentPathOrGuid) == true)
                throw new ArgumentException("Content path cannot be null or empty");

            // Try to get the bundle stream
            ContentReaderInfo importerContext;
            Task<ContentReaderInfo> streamTask = GetContentStream(contentPathOrGuid, bundle, null);

            // Wait for stream to load
            streamTask.Wait();
            importerContext = streamTask.Result;

            // Get the stream
            return importerContext.stream;
        }

        public async Task<Stream> LoadDataStreamAsync(string contentPathOrGuid, ContentBundle bundle = null)
        {
            // Check for invalid
            if (string.IsNullOrEmpty(contentPathOrGuid) == true)
                throw new ArgumentException("Content path cannot be null or empty");

            // Try to get stream
            ContentReaderInfo importerContext = await GetContentStream(contentPathOrGuid, bundle, null);

            // Get the stream
            return importerContext.stream;
        }

        public ContentBundle LoadBundle(string nameOrPath)
        {
            Debug.Log(LogFilter.Content, "Load bundle: " + nameOrPath);

            // Try to get the bundle stream
            ContentReaderInfo importerContext;
            Task<ContentReaderInfo> streamTask = GetContentStream(nameOrPath, null, typeof(ContentBundle), false);

            // Wait for stream to load
            streamTask.Wait();
            importerContext = streamTask.Result;

            // Check for error
            if (importerContext.stream == null)
                return null;

            // Load the target bundle - keep stream open
            ContentBundle bundle = ContentBundle.LoadBundle(this, importerContext.stream, nameOrPath);

            // Register bundle
            if (bundle != null)
            {
                lock (contentBundles)
                {
                    contentBundles.Add(bundle);
                }
            }

            return bundle;
        }

        public async Task<ContentBundle> LoadBundleAsync(string nameOrPath)
        {
            Debug.Log(LogFilter.Content, "Load bundle async: " + nameOrPath);

            // Try to get the bundle stream
            ContentReaderInfo importerContext = await GetContentStream(nameOrPath, null, typeof(ContentBundle), false);

            // Check for error
            if (importerContext.stream == null)
                return null;

            // Load the target bundle - keep stream open
            ContentBundle bundle = await Task.Run(() => ContentBundle.LoadBundle(this, importerContext.stream, nameOrPath));

            // Register bundle
            if (bundle != null)
            {
                lock (contentBundles)
                {
                    contentBundles.Add(bundle);
                }
            }

            return bundle;
        }

        public void Unload(GameElement element)
        {
            // Check for null
            if (element == null)
                return;

            // Check for asset
            if (element.IsDestroyed == false && element.IsDestroying == false)
                throw new InvalidOperationException("Element must be destroyed before it can be unloaded. Consider using Destroy or DestroyImmediate");

            // Unload the element
            UnloadElement(element);
        }

        public void Unload(string guid)
        {
            // Check for null
            if (guid == null)
                throw new ArgumentNullException(nameof(guid));

            // Check for guid
            if (IsGuid(guid) == false)
                throw new ArgumentException("Not a valid guid");

            // Try to get the asset
            GameElement element;

            // Check if asset is loaded
            if(contentGuidCache.TryGetValue(guid, out element) == true)
            {
                // Unload the element
                UnloadElement(element);
            }
        }

        public void UnloadAll()
        {
            // Get all loaded elements
            List<GameElement> elements = new List<GameElement>(contentGuidCache.Values);

            // Process all elements
            foreach (GameElement element in elements)
            {
                // Unload the element
                UnloadElement(element);
            }
        }

        private void UnloadElement(GameElement element)
        {
            // Remove path caching
            if (element.HasContentPath == true && contentPathCache.ContainsKey(element.ContentPath) == true)
            {
#if !SIMPLE2D_WEB
                lock (contentPathCache)
#endif
                {
                    contentPathCache.Remove(element.ContentPath, out _);
                }
            }

            // Remove guid caching
            if (contentGuidCache.ContainsKey(element.Guid) == true)
            {
#if !SIMPLE2D_WEB
                lock (contentGuidCache)
#endif
                {
                    contentGuidCache.Remove(element.Guid, out _);
                }
            }
        }        

        public bool IsLoaded(string contentPathOrGuid)
        {
            // Check for cached guid
            if (IsGuid(contentPathOrGuid) == true)
                return contentGuidCache.ContainsKey(contentPathOrGuid);

            // Check for cached path
            return contentPathCache.ContainsKey(contentPathOrGuid);
        }

        public void Dispose()
        {
            // Destroy all elements
            //foreach(GameElement element in contentPathCache.Values)
            //{
            //    element.DestroyImmediate();
            //}

            //foreach(GameElement element in contentGuidCache.Values)
            //{
            //    element.DestroyImmediate();
            //}

            contentPathCache.Clear();
            contentGuidCache.Clear();
        }

        protected abstract Task<ContentReaderInfo> GetContentStreamFromPath(string contentPath, Type hintType);

        private async Task<ContentReaderInfo> GetContentStream(string contentPathOrGuid, ContentBundle bundle, Type hintType, bool allowBundle = true)
        {
            // Check for bundle
            if (bundle != null)
                return bundle.GetContentStream(contentPathOrGuid, hintType);

            // Check for guid
            if(IsGuid(contentPathOrGuid) == true && allowBundle == true)
            {
                return GetContentStreamFromGuid(contentPathOrGuid, hintType);
            }

            // Try to get import context
            ContentReaderInfo result = await GetContentStreamFromPath(contentPathOrGuid, hintType);

            // Check for success
            if (result.stream != null)
                return result;

            // Check all bundles
            foreach(ContentBundle loadedBundle in contentBundles)
            {
                // Try to get content
                result = loadedBundle.GetContentStream(contentPathOrGuid, hintType);

                // Check for success
                if (result.stream != null)
                    return result;
            }

            return default;
        }

        private ContentReaderInfo GetContentStreamFromGuid(string guid, Type hintType)
        {
            // Check for bundle
            ContentBundle bundle;

            // Check if any loaded bundle contains asset
            if (contentBundleGuidAssetLookup.TryGetValue(guid, out bundle) == false)
                return default;

            // Get asset from bundle
            return bundle.GetContentStream(guid, hintType);
        }

        private bool GetCachedContent(string pathOrGuid, out GameElement element)
        {
            // Check path cache
            if (contentPathCache.TryGetValue(pathOrGuid, out element) == true)
                return true;

            // Check guid cache
            if (contentGuidCache.TryGetValue(pathOrGuid, out element) == true)
                return true;

            element = null;
            return false;
        }

        private Stream GetRequiredContentStream(Stream stream, IContentReader contentReader)
        {
            if(contentReader.RequireStreamSeeking == true && stream.CanSeek == false)
            {
                MemoryStream tempStream = new MemoryStream();
                stream.CopyTo(tempStream);

                // Release source stream
                stream.Dispose();

                // Return to start of data
                tempStream.Position = 0;

                return tempStream;
            }
            return stream;
        }

        internal IContentReader GetContentReaderInstance(string fileExtension)
        {
            // Try to get read type
            Type readerType;
            if (contentReaders.TryGetValue(fileExtension.ToLower(), out readerType) == false)
                return null;

            // Create instance
            return Activator.CreateInstance(readerType) as IContentReader;
        }

        private GameElement AddCachedContent(GameElement element, string hintPath)
        {
            // Check for null
            if (element == null)
                return null;

            // Add to guid cache
            contentGuidCache[element.Guid] = element;

            // Add to path cache
            if (string.IsNullOrEmpty(hintPath) == false && IsGuid(hintPath) == false)
                contentPathCache[hintPath] = element;

            return element;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", typeof(ContentProvider).FullName, ContentFolder);
        }

        internal static bool IsGuid(string input)
        {
            // Perform simple checks first - guid is always fixed length of 36 characters so we can do this initial check to help with performance
            if (input == null || input.Length != 36)
                return false;

            return Guid.TryParse(input, out _);
        }

        private void LoadContentReaders()
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
                        foreach (ContentReaderAttribute attrib in type.GetCustomAttributes<ContentReaderAttribute>())
                        {
                            // Check for derived type
                            if (typeof(IContentReader).IsAssignableFrom(type) == false)
                            {
                                Debug.LogError(LogFilter.Content, "Content reader must implement `IContentReader`: " + type);
                                continue;
                            }

                            // Get extension
                            string ext = attrib.FileExtension.ToLower();

                            // Check for overwrite content reader
                            if(contentReaders.ContainsKey(ext) == true)
                            {
                                Debug.LogErrorF(LogFilter.Content, "A content reader already exists for extension `{0}`: {1}", type, attrib.FileExtension);
                                continue;
                            }

                            // Store reader type
                            contentReaders[ext] = type;
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