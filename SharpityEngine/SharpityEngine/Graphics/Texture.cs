using System.Runtime.InteropServices;
using WGPU.NET;

namespace SharpityEngine.Graphics
{
    public enum TextureUsage
    {
        None = 0,
        CopySrc = 1,
        CopyDst = 2,
        TextureBinding = 4,
        StorageBinding = 8,
        RenderAttachment = 0x10,
    }

    public enum TextureDimension
    {
        Texture1D = 0,
        Texture2D = 1,
        Texture3D = 2,
        Force32 = int.MaxValue
    }

    public enum TextureFormat
    {
        R8Unorm = 1,
        R8Snorm = 2,
        R8Uint = 3,
        R8Sint = 4,
        R16Uint = 5,
        R16Sint = 6,
        R16Float = 7,
        RG8Unorm = 8,
        RG8Snorm = 9,
        RG8Uint = 10,
        RG8Sint = 11,
        R32Float = 12,
        R32Uint = 13,
        R32Sint = 14,
        RG16Uint = 0xF,
        RG16Sint = 0x10,
        RG16Float = 17,
        RGBA8Unorm = 18,
        RGBA8UnormSrgb = 19,
        RGBA8Snorm = 20,
        RGBA8Uint = 21,
        RGBA8Sint = 22,
        BGRA8Unorm = 23,
        BGRA8UnormSrgb = 24,
        RGB10A2Unorm = 25,
        RG11B10Ufloat = 26,
        RGB9E5Ufloat = 27,
        RG32Float = 28,
        RG32Uint = 29,
        RG32Sint = 30,
        RGBA16Uint = 0x1F,
        RGBA16Sint = 0x20,
        RGBA16Float = 33,
        RGBA32Float = 34,
        RGBA32Uint = 35,
        RGBA32Sint = 36,
        Stencil8 = 37,
        Depth16Unorm = 38,
        Depth24Plus = 39,
        Depth24PlusStencil8 = 40,
        Depth32Float = 41,
        Depth32FloatStencil8 = 42,
        BC1RGBAUnorm = 43,
        BC1RGBAUnormSrgb = 44,
        BC2RGBAUnorm = 45,
        BC2RGBAUnormSrgb = 46,
        BC3RGBAUnorm = 47,
        BC3RGBAUnormSrgb = 48,
        BC4RUnorm = 49,
        BC4RSnorm = 50,
        BC5RGUnorm = 51,
        BC5RGSnorm = 52,
        BC6HRGBUfloat = 53,
        BC6HRGBFloat = 54,
        BC7RGBAUnorm = 55,
        BC7RGBAUnormSrgb = 56,
        ETC2RGB8Unorm = 57,
        ETC2RGB8UnormSrgb = 58,
        ETC2RGB8A1Unorm = 59,
        ETC2RGB8A1UnormSrgb = 60,
        ETC2RGBA8Unorm = 61,
        ETC2RGBA8UnormSrgb = 62,
        EACR11Unorm = 0x3F,
        EACR11Snorm = 0x40,
        EACRG11Unorm = 65,
        EACRG11Snorm = 66,
        ASTC4x4Unorm = 67,
        ASTC4x4UnormSrgb = 68,
        ASTC5x4Unorm = 69,
        ASTC5x4UnormSrgb = 70,
        ASTC5x5Unorm = 71,
        ASTC5x5UnormSrgb = 72,
        ASTC6x5Unorm = 73,
        ASTC6x5UnormSrgb = 74,
        ASTC6x6Unorm = 75,
        ASTC6x6UnormSrgb = 76,
        ASTC8x5Unorm = 77,
        ASTC8x5UnormSrgb = 78,
        ASTC8x6Unorm = 79,
        ASTC8x6UnormSrgb = 80,
        ASTC8x8Unorm = 81,
        ASTC8x8UnormSrgb = 82,
        ASTC10x5Unorm = 83,
        ASTC10x5UnormSrgb = 84,
        ASTC10x6Unorm = 85,
        ASTC10x6UnormSrgb = 86,
        ASTC10x8Unorm = 87,
        ASTC10x8UnormSrgb = 88,
        ASTC10x10Unorm = 89,
        ASTC10x10UnormSrgb = 90,
        ASTC12x10Unorm = 91,
        ASTC12x10UnormSrgb = 92,
        ASTC12x12Unorm = 93,
        ASTC12x12UnormSrgb = 94,
    }

    public class Texture : GameAsset
    {
        // Private 
        private Device device = null;
        private WGPU.NET.Texture texture = null;
        private TextureFormat format = 0;
        private TextureDimension dimension = 0;
        private int mipLevelCount = 1;
        private int sampleCount = 1;

        // Properties
        public TextureFormat Format
        {
            get { return format; }
        }

        public TextureDimension Dimension
        {
            get { return dimension; }
        }

        public int Width
        {
            get { return (int)texture.Size.width; }
        }

        public int Height
        {
            get { return (int)texture.Size.height; }
        }

        public int DepthOrLayers
        {
            get { return (int)texture.Size.depthOrArrayLayers; }
        }

        public int MipLevelCount
        {
            get { return mipLevelCount; }
        }

        public int SampleCount
        {
            get { return sampleCount; }
        }

        // Constructor
        internal Texture(Device device, WGPU.NET.Texture texture, TextureFormat format, TextureDimension dimension, int mipLevel, int sampleCount)
        {
            this.device = device;
            this.texture = texture;
            this.format = format;
            this.dimension = dimension;
            this.mipLevelCount = mipLevel;
            this.sampleCount = sampleCount;
        }

        // Methods
        protected override void OnDestroy()
        {
            base.OnDestroy();
            texture.Dispose();
        }

        public void Write<T>(ReadOnlySpan<T> data, int mipLevel = 0) where T : unmanaged
        {
            // Create copy instruction
            ImageCopyTexture copy = new ImageCopyTexture
            {
                Aspect = Wgpu.TextureAspect.All,
                MipLevel = (uint)mipLevel,
                Origin = default,
                Texture = texture,
            };

            // Create data layout
            Wgpu.TextureDataLayout layout = new Wgpu.TextureDataLayout
            {
                offset = 0,
                bytesPerRow = (uint)(Marshal.SizeOf<T>() * Width),
                rowsPerImage = (uint)Height,
            };

            // Add to queue
            device.Queue.WriteTexture<T>(copy, data, layout, texture.Size);
        }
    }
}
