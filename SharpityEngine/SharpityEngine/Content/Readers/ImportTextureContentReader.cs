using SharpityEngine.Graphics;
using StbImageSharp;

namespace SharpityEngine.Content.Readers
{
    [ContentReader(".bmp")]
    [ContentReader(".jpg")]
    [ContentReader(".jpeg")]
    [ContentReader(".png")]
    internal sealed class ImportTextureContentReader : IContentReader
    {
        // Properties
        public bool RequireStreamSeeking => true;

        // Methods
        public Task<object> ReadContentAsync(Stream readStream, IContentReader.ContentReadContext context, CancellationToken cancelToken)
        {
            // Try to read image
            ImageResult result = ImageResult.FromStream(readStream, ColorComponents.RedGreenBlueAlpha);

            // Check for loaded
            if (result == null)
                return Task.FromResult<object>(null);

            // Create texture
            TextureFormat format = result.Comp switch
            {
                ColorComponents.Grey => TextureFormat.R8Unorm,
                ColorComponents.RedGreenBlueAlpha => TextureFormat.RGBA8Unorm,
            };

            // Get pixel size
            int pixelSpan = result.Comp switch
            {
                ColorComponents.Grey => 1,
                ColorComponents.RedGreenBlueAlpha => 4,
            };

            // Create texture
            Texture texture = context.GraphicsDevice.CreateTexture2D(result.Width, result.Height, format);

            // Create spawn from pixels
            ReadOnlySpan<byte> pixels = result.Data;
            {
                // Upload data
                context.GraphicsDevice.Queue.WriteTexture(pixels, texture, new TextureDataLayout(result.Width * pixelSpan, result.Height));
            }

            // Get result
            return Task.FromResult<object>(texture);
        }
    }
}
