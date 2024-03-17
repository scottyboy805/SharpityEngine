using Newtonsoft.Json;

namespace SharpityEngine.Content.Readers
{
    [ContentReader(".json")]
    internal sealed class JsonContentReader : IContentReader
    {
        // Properties
        public bool RequireStreamSeeking
        {
            get { return false; }
        }

        // Methods
        public async Task<object> ReadContentAsync(Stream readStream, IContentReader.ContentReadContext context, CancellationToken cancelToken)
        {
            // Read the json asset
            using (JsonReader reader = new JsonTextReader(new StreamReader(readStream)))
            {
                // Create formatter
                JsonDeserializeFormatter formatter = new JsonDeserializeFormatter(context);

                // Read the object
                object instance = await formatter.DeserializeObject(reader);

                // Get as element
                return instance;
            }
        }
    }
}
