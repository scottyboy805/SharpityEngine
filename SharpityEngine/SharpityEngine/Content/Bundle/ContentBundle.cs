using Newtonsoft.Json;
using SharpityEngine.Scene;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;

namespace SharpityEngine.Content
{
    public sealed class ContentBundle
    {
        // Private
        private ContentProvider contentProvider = null;
        private string contentPath = "";
        private string bundleName = "";

        // Internal
        internal static readonly int bundleIdentifier = BitConverter.ToInt32(Encoding.UTF8.GetBytes("SHFS"), 0); // Sharpity file system
        internal static string bundleExtension = "pksh";    // Sharpity package

        internal bool isLoaded = true;
        internal ZipArchive archive = null;
        internal ContentBundleAssetIndex assetIndex = null;
        internal ConcurrentDictionary<string, ContentBundleAssetEntry> guidAssetLookup = new ConcurrentDictionary<string, ContentBundleAssetEntry>();
        internal ConcurrentDictionary<string, ContentBundleAssetEntry> pathAssetLookup = new ConcurrentDictionary<string, ContentBundleAssetEntry>(StringComparer.OrdinalIgnoreCase);

        // Properties
        public bool IsLoaded
        {
            get { return isLoaded; }
        }

        public string ContentPath
        {
            get { return contentPath; }
        }

        public string BundleName
        {
            get { return bundleName; }
        }

        // Constructor
        public ContentBundle(ContentProvider provider, string contentPath)
        {
            this.contentProvider = provider;
            this.contentPath = contentPath;
            this.bundleName = Path.GetFileNameWithoutExtension(contentPath);
        }

        // Methods
        public GameScene LoadScene(string contentPathOrGuid, bool autoActivate)
        {
            return contentProvider.LoadScene(contentPathOrGuid, autoActivate, this);
        }

        public Task<GameScene> LoadSceneAsync(string contentPathOrGuid, bool autoActivate)
        {
            return contentProvider.LoadSceneAsync(contentPathOrGuid, autoActivate, this);
        }

        public T Load<T>(string contentPathOrGuid) where T : class
        {
            return contentProvider.Load<T>(contentPathOrGuid, this);
        }

        public Task<T> LoadAsync<T>(string contentPathOrGuid) where T : class
        {
            return contentProvider.LoadAsync<T>(contentPathOrGuid, this);
        }

        public GameElement Load(string contentPathOrGuid)
        {
            return contentProvider.Load(contentPathOrGuid, this);
        }

        public Task<GameElement> LoadAsync(string contentPathOrGuid)
        {
            return contentProvider.LoadAsync(contentPathOrGuid, this);
        }

        public string LoadDataText(string contentPathOrGuid)
        {
            return contentProvider.LoadDataText(contentPathOrGuid, this);
        }

        public Task<string> LoadDataTextAsync(string contentPathOrGuid)
        {
            return contentProvider.LoadDataTextAsync(contentPathOrGuid, this);
        }

        public byte[] LoadDataBytes(string contentPathOrGuid)
        {
            return contentProvider.LoadDataBytes(contentPathOrGuid, this);
        }

        public Task<byte[]> LoadDataBytesAsync(string contentPathOrGuid)
        {
            return contentProvider.LoadDataBytesAsync(contentPathOrGuid, this);
        }

        public Stream LoadDataStream(string contentPathOrGuid)
        {
            return contentProvider.LoadDataStream(contentPathOrGuid, this);
        }

        public Task<Stream> LoadDataStreamAsync(string contentPathOrGuid)
        {
            return contentProvider.LoadDataStreamAsync(contentPathOrGuid, this);
        }

        public void Unload(bool unloadAssets)
        {
            // Check for already unloaded
            if (isLoaded == false)
                throw new InvalidOperationException("Bundle has already been unloaded");

            // Check for unload assets associated with this bundle
            if(unloadAssets == true)
            {
                // Process all guids
                foreach(ContentBundleAssetEntry entry in assetIndex.assetEntries)
                {
                    // Request unload
                    contentProvider.Unload(entry.guid);
                }
            }

            // Unload the bundle
            isLoaded = false;

            // Remove cached content from provider
            foreach (ContentBundleAssetEntry entry in assetIndex.assetEntries)
            {
                // Remove cache for content provider
                if(contentProvider.contentBundleGuidAssetLookup.ContainsKey(entry.guid) == true)
                    contentProvider.contentBundleGuidAssetLookup.Remove(entry.guid, out _);
            }

            // Remove from bundles
            contentProvider.contentBundles.Remove(this);

            // Release archive
            archive.Dispose();
            archive = null;
            assetIndex = null;

            // Clear cache
            guidAssetLookup.Clear();
            guidAssetLookup = null;
            pathAssetLookup.Clear();
            pathAssetLookup = null;
    }

        internal ContentProvider.ContentReaderInfo GetContentStream(string pathOrGuid, Type hintType)
        {
            // Check for loaded
            if (isLoaded == false)
                throw new InvalidOperationException("Content bundle has been unloaded: " + bundleName);

            ContentBundleAssetEntry assetEntry = null;

            // Check for guid
            if(ContentProvider.IsGuid(pathOrGuid) == true)
            {
                // Check for content
                guidAssetLookup.TryGetValue(pathOrGuid, out assetEntry);
            }
            else
            {
                pathAssetLookup.TryGetValue(pathOrGuid, out assetEntry);
            }

            // Check for header
            if (assetEntry == null)
                return default;

            // Get the content stream for the asset
            return GetContentStreamForAsset(assetEntry, hintType);
        }

        private ContentProvider.ContentReaderInfo GetContentStreamForAsset(ContentBundleAssetEntry assetEntry, Type hintType)
        {
            // Try to get archive entry
            ZipArchiveEntry entry = archive.GetEntry(assetEntry.path);

            // Check for error
            if (entry == null)
                return default;

            // Create the import context
            return new ContentProvider.ContentReaderInfo
            {
                // Get stream
                Stream = entry.Open(),

                // Create reader
                Reader = contentProvider.GetContentReaderInstance(assetEntry.extension),

                // Create context
                Context = new IContentReader.ContentReadContext(hintType, contentProvider, this,
                    assetEntry.guid, assetEntry.path, assetEntry.dataSize),
            };
        }

        private void CacheBundleContent()
        {
            foreach(ContentBundleAssetEntry entry in assetIndex.assetEntries)
            {
                // Cache asset name with and without extension
                pathAssetLookup[entry.path] = entry;
                pathAssetLookup[Path.ChangeExtension(entry.path, null)] = entry;

                // Cache asset guid
                guidAssetLookup[entry.guid] = entry;

                // Cache for content provider
                contentProvider.contentBundleGuidAssetLookup[entry.guid] = this;
            }
        }

        internal static ContentBundle LoadBundle(Stream stream, IContentReader.ContentReadContext context)
        {
            // Check for null
            if (stream == null)
                throw new ArgumentNullException("Stream");


            // Temp buffer
            byte[] buffer = new byte[sizeof(int)];

            // Check for bundle file
            stream.Read(buffer, 0, sizeof(int));

            // Validate bundle
            if (BitConverter.ToInt32(buffer, 0) != ContentBundle.bundleIdentifier)
                throw new BadImageFormatException("Not a valid content bundle");

            // Create the bundle
            ContentBundle bundle = new ContentBundle(context.ContentProvider, context.ContentPath);

            // Open zip
            ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);

            // Read index
            ZipArchiveEntry indexEntry = archive.GetEntry("Assets.index");

            // Check for error
            if (indexEntry == null)
                throw new InvalidDataException("Content bundle is invalid");

            // Create asset index
            ContentBundleAssetIndex index = null;
            
            // Create loader
            JsonDeserializeFormatter formatter = new JsonDeserializeFormatter(new IContentReader.ContentReadContext(
                null, context.ContentProvider, bundle, null, "Assets.index"));

            // Try to load asset index
            using (Stream indexStream = indexEntry.Open())
            {
                using (StreamReader indexReader = new StreamReader(indexStream))
                {
                    using (JsonTextReader textReader = new JsonTextReader(indexReader))
                    {
                        // Read index
                        index = formatter.DeserializeObject(textReader).Result as ContentBundleAssetIndex;
                    }
                }
            }

            // Check for no index
            if (index == null)
                throw new InvalidDataException("Content bundle is invalid");

            // Store index and archive
            bundle.archive = archive;
            bundle.assetIndex = index;

            // Cache content from asset index
            bundle.CacheBundleContent();

            return bundle;
        }
    }
}