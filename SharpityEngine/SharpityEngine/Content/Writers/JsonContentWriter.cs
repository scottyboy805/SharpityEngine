using Newtonsoft.Json;

namespace SharpityEngine.Content.Writers
{
    [ContentWriter(typeof(GameObject), false)]
    internal sealed class JsonContentWriter : IContentWriter
    {
        // Properties
        public bool RequireStreamSeeking => false;

        // Methods
        public async Task<bool> WriteContentAsync(object content, Stream writeStream, IContentWriter.ContentWriteContext context, CancellationToken cancelToken)
        {
            JsonSerializeFormatter formatter = new JsonSerializeFormatter(context);

            using (JsonWriter writer = new JsonTextWriter(new StreamWriter(writeStream)))
            {
                // Before save
                if (content is IContentCallback)
                    ((IContentCallback)content).OnBeforeContentSave();

                writer.Formatting = Formatting.Indented;
                formatter.SerializeObject(writer, content);
            }

            return true;
        }
    }
}
