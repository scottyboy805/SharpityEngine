using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            public Stream Stream;
            public IContentReader Reader;
            public IContentReader.ContentReadContext Context;
        }

        protected internal struct ContentWriterInfo
        {
            // Public
            public Stream Stream;
            public IContentWriter Writer;
            public IContentWriter.ContentWriteContext Context;
        }

        // Internal
        internal TypeManager typeManager = null;

        // Private
        private Dictionary<string, Type> contentReaders = new Dictionary<string, Type>();   // Extension, IContentReader type
        private Dictionary<(Type, bool), Type> contentWriters = new Dictionary<(Type, bool), Type>();       // Tuple(Content Type, optimized), IContentWriter type
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
            LoadContentWriters();
        }

        // Methods
        public async void SaveAsync(GameElement element, string file, CancellationToken cancelToken = default)
        {
            // Check for null
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            // Start timing
            Stopwatch timer = Stopwatch.StartNew();

            string savePath = file;

            // Get output path
            if(Path.IsPathRooted(file) == false)
                savePath = Path.Combine(contentRootPath, file);

            // Get content writer
            ContentWriterInfo writerInfo = await GetWriteContentStream(file, element, element.GetType());

            // Check for valid stream
            if(writerInfo.Stream == null)
            {
                Debug.LogError(LogFilter.Content, "Could not find path: " + file);
                return;
            }

            // Make sure stream is disposed
            using(writerInfo.Stream)
            {
                // Check for error
                if (writerInfo.Writer == null)
                    throw new NotSupportedException("No content writer for file format: " + file);

                // Create for write game element
                await writerInfo.Writer.WriteContentAsync(element, GetRequiredContentStream(writerInfo.Stream, writerInfo.Writer), writerInfo.Context, cancelToken);
            }

            // Report saved
            Debug.Log(LogFilter.Content, "Save content: " + file + " - " + timer.ElapsedMilliseconds + "ms");
            timer.Stop();
        }

        public GameScene LoadScene(string contentPathOrGuid, bool autoActivate = true, ContentBundle bundle = null)
        {
            Debug.Log(LogFilter.Content, "Load scene: " + contentPathOrGuid);

            // issue load request
            GameScene scene = Load<GameScene>(contentPathOrGuid, bundle);

            // Check for success
            if (scene != null && autoActivate == true)
                scene.Activate();

            return scene;
        }

        public async Task<GameScene> LoadSceneAsync(string contentPathOrGuid, bool autoActivate = true, ContentBundle bundle = null)
        {
            Debug.Log(LogFilter.Content, "Load scene async: " + contentPathOrGuid);

            // Issue load request
            GameScene scene = await LoadAsync<GameScene>(contentPathOrGuid, bundle);

            // Check for success
            if (scene != null && autoActivate == true)
                scene.Activate();

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

            // Start timing
            Stopwatch timer = Stopwatch.StartNew();

            // Get stream
            ContentReaderInfo readerInfo;
            Task<ContentReaderInfo> streamTask = GetReadContentStream(contentPathOrGuid, bundle, type);

            if (streamTask.IsFaulted == true)
                Debug.LogException(streamTask.Exception);

            // Wait for stream to load
            streamTask.Wait();
            readerInfo = streamTask.Result;

            // Check for valid stream
            if (readerInfo.Stream == null)
            {
                Debug.LogError(LogFilter.Content, "Could not find content: " + contentPathOrGuid);
                return null;
            }

            // Make sure stream is disposed
            using (readerInfo.Stream)
            {
                // Check for error
                if (readerInfo.Reader == null)
                    throw new NotSupportedException("No content reader for file format: " + contentPathOrGuid);

                try
                {
                    // Check for game element reader - Block main thread during load
                    result = readerInfo.Reader.ReadContentAsync(GetRequiredContentStream(readerInfo.Stream, readerInfo.Reader), readerInfo.Context, default)
                        .Result as GameElement;
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }

                // Check for success
                if (result != null)
                {
                    result.ContentPath = contentPathOrGuid;
                    result.ContentBundle = readerInfo.Context.ContainingBundle;
                }
                else
                {
                    Debug.LogError(LogFilter.Content, "Could not load content: " + contentPathOrGuid);
                    return null;
                }

                // Run load events
                if (result is IContentCallback)
                    ((IContentCallback)result).OnAfterContentLoad();

                // Report loaded
                Debug.Log(LogFilter.Content, "Load content: " + contentPathOrGuid + " - " + timer.ElapsedMilliseconds + "ms");
                timer.Stop();

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

            // Start timing
            Stopwatch timer = Stopwatch.StartNew();

            // Get stream
            ContentReaderInfo readerInfo = await GetReadContentStream(contentPathOrGuid, bundle, type);

            // Check for valid stream
            if (readerInfo.Stream == null)
            {
                Debug.LogError(LogFilter.Content, "Could not find content: " + contentPathOrGuid);
                return null;
            }

            // Make sure stream is disposed
            using (readerInfo.Stream)
            {
                // Check for error
                if (readerInfo.Reader == null)
                    throw new NotSupportedException("No content reader for file format: " + contentPathOrGuid);

                try
                {
                    // Check for game element importer
                    result = await readerInfo.Reader.ReadContentAsync(GetRequiredContentStream(readerInfo.Stream, readerInfo.Reader), readerInfo.Context, cancelToken)
                        as GameElement;
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }

                // Check for success
                if (result != null)
                {
                    result.ContentPath = contentPathOrGuid;
                    result.ContentBundle = readerInfo.Context.ContainingBundle;
                }
                else
                {
                    Debug.LogError(LogFilter.Content, "Could not load content: " + contentPathOrGuid);
                    return null;
                }

                // Run load events
                if (result is IContentCallback)
                    ((IContentCallback)result).OnAfterContentLoad();

                // Report loaded
                Debug.Log(LogFilter.Content, "Load content: " + contentPathOrGuid + " - " + timer.ElapsedMilliseconds + "ms");
                timer.Stop();

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
            Task<ContentReaderInfo> streamTask = GetReadContentStream(contentPathOrGuid, bundle, null);

            // Wait for stream to load
            streamTask.Wait();
            importerContext = streamTask.Result;

            // Get the stream
            return importerContext.Stream;
        }

        public async Task<Stream> LoadDataStreamAsync(string contentPathOrGuid, ContentBundle bundle = null)
        {
            // Check for invalid
            if (string.IsNullOrEmpty(contentPathOrGuid) == true)
                throw new ArgumentException("Content path cannot be null or empty");

            // Try to get stream
            ContentReaderInfo importerContext = await GetReadContentStream(contentPathOrGuid, bundle, null);

            // Get the stream
            return importerContext.Stream;
        }

        public ContentBundle LoadBundle(string nameOrPath)
        {
            Debug.Log(LogFilter.Content, "Load bundle: " + nameOrPath);

            // Try to get the bundle stream
            ContentReaderInfo importerInfo;
            Task<ContentReaderInfo> streamTask = GetReadContentStream(nameOrPath, null, typeof(ContentBundle), false);

            // Wait for stream to load
            streamTask.Wait();
            importerInfo = streamTask.Result;

            // Check for error
            if (importerInfo.Stream == null)
                return null;

            // Load the target bundle - keep stream open
            ContentBundle bundle = ContentBundle.LoadBundle(importerInfo.Stream, importerInfo.Context);

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
            ContentReaderInfo importerContext = await GetReadContentStream(nameOrPath, null, typeof(ContentBundle), false);

            // Check for error
            if (importerContext.Stream == null)
                return null;

            // Load the target bundle - keep stream open
            ContentBundle bundle = await Task.Run(() => ContentBundle.LoadBundle(importerContext.Stream, importerContext.Context));

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

        public virtual void Dispose()
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

        protected abstract Task<ContentReaderInfo> GetReadContentStreamFromPath(string contentPath, Type hintType);

        protected abstract Task<ContentWriterInfo> GetWriteContentStreamFromPath(string contentPath, string guid, Type contentType);

        private async Task<ContentReaderInfo> GetReadContentStream(string contentPathOrGuid, ContentBundle bundle, Type hintType, bool allowBundle = true)
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
            ContentReaderInfo result = await GetReadContentStreamFromPath(contentPathOrGuid, hintType);

            // Check for success
            if (result.Stream != null)
                return result;

            // Check all bundles
            foreach(ContentBundle loadedBundle in contentBundles)
            {
                // Try to get content
                result = loadedBundle.GetContentStream(contentPathOrGuid, hintType);

                // Check for success
                if (result.Stream != null)
                    return result;
            }

            return default;
        }

        private async Task<ContentWriterInfo> GetWriteContentStream(string contentPath, object content, Type contentType)
        {
            // Check for guid
            string guid = null;
            if(content is GameElement)
                guid = ((GameElement)content).Guid;

            // Try to get export context
            ContentWriterInfo result = await GetWriteContentStreamFromPath(contentPath, guid, contentType);

            // Check for success
            if (result.Stream != null)
                return result;

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

        private Stream GetRequiredContentStream(Stream stream, IContentWriter contentWriter)
        {
            //if (contentWriter.RequireStreamSeeking == true && stream.CanSeek == false)
            //{
            //    MemoryStream tempStream = new MemoryStream();
            //    stream.CopyTo(tempStream);

            //    // Release source stream
            //    stream.Dispose();

            //    // Return to start of data
            //    tempStream.Position = 0;

            //    return tempStream;
            //}
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

        internal IContentWriter GetContentWriterInstance(Type contentType, bool optimizedWriter)
        {
            // Try to get write type
            Type writerType;
            if (contentWriters.TryGetValue((contentType, optimizedWriter), out writerType) == false)
                return null;

            // Create instance
            return Activator.CreateInstance(writerType) as IContentWriter;
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

        private void LoadContentWriters()
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
                        foreach (ContentWriterAttribute attrib in type.GetCustomAttributes<ContentWriterAttribute>())
                        {
                            // Check for derived type
                            if (typeof(IContentWriter).IsAssignableFrom(type) == false)
                            {
                                Debug.LogError(LogFilter.Content, "Content writer must implement `IContentWriter`: " + type);
                                continue;
                            }

                            // Check for overwrite content reader
                            if (contentWriters.ContainsKey((attrib.ContentType, attrib.OptimizedWriter)) == true)
                            {
                                Debug.LogErrorF(LogFilter.Content, "A content writer already exists for type `{0}`: {1}", type, attrib.ContentType);
                                continue;
                            }

                            // Store reader type
                            contentWriters[(attrib.ContentType, attrib.OptimizedWriter)] = type;
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