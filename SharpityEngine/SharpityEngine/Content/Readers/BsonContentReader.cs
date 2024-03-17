using Newtonsoft.Json.Bson;
using Newtonsoft.Json;

namespace SharpityEngine.Content.Readers
{
    [ContentReader(".bson")]
    internal sealed class BsonContentReader : IContentReader
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
            using (JsonReader reader = new BsonReader(readStream))
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
