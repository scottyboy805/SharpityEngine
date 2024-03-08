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

    public enum TextureAspect : int
    {
        All = 0x00000000,
        StencilOnly = 0x00000001,
        DepthOnly = 0x00000002,
    }

    public enum TextureDimension
    {
        Texture1D = 0,
        Texture2D = 1,
        Texture3D = 2,
    }

    public enum TextureViewDimension : int
    {
        Default = 0x00000000,
        Texture1D = 0x00000001,
        Texture2D = 0x00000002,
        Texture2DArray = 0x00000003,
        Cube = 0x00000004,
        CubeArray = 0x00000005,
        Texture3D = 0x00000006,
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

    public enum WrapMode : int
    {
        Repeat = 0x00000000,
        MirrorRepeat = 0x00000001,
        ClampToEdge = 0x00000002,
    }

    public enum FilterMode : int
    {
        Nearest = 0x00000000,
        Linear = 0x00000001,
    }

    public enum CompareFunction : int
    {
        Default = 0x00000000,
        Never = 0x00000001,
        Less = 0x00000002,
        LessEqual = 0x00000003,
        Greater = 0x00000004,
        GreaterEqual = 0x00000005,
        Equal = 0x00000006,
        NotEqual = 0x00000007,
        Always = 0x00000008,
    }

    public sealed class Sampler : IDisposable
    {
        // Internal
        internal Wgpu.SamplerImpl wgpuSampler;

        // Constructor
        internal Sampler(Wgpu.SamplerImpl wgpuSampler)
        {
            this.wgpuSampler = wgpuSampler;
        }

        // Methods
        public void Dispose()
        {
            if(wgpuSampler.Handle != IntPtr.Zero)
            {
                // Release sampler
                Wgpu.SamplerRelease(wgpuSampler);
                wgpuSampler = default;
            }
        }
    }

    public sealed class TextureView : IDisposable
    {
        // Internal
        internal Wgpu.TextureViewImpl wgpuTextureView;

        // Private
        private Texture texture = null;        

        // Properties
        public Texture Texture
        {
            get { return texture; }
        }

        // Constructor
        internal TextureView(Wgpu.TextureViewImpl wgpuTextureView, Texture texture)
        {
            this.wgpuTextureView = wgpuTextureView;
            this.texture = texture;
        }

        // Methods
        public void Dispose()
        {
            if(wgpuTextureView.Handle != IntPtr.Zero)
            {
                // Unregister view
                texture?.ReleaseView(this);

                // Release view
                Wgpu.TextureViewRelease(wgpuTextureView);
                wgpuTextureView = default;
            }
        }
    }

    public sealed class Texture : GameAsset
    {
        // Internal
        internal Wgpu.TextureImpl wgpuTexture;
        internal Wgpu.TextureDescriptor wgpuTextureDesc;

        // Private
        private HashSet<TextureView> createdViews = null;

        // Properties
        public TextureFormat Format
        {
            get { return (TextureFormat)wgpuTextureDesc.format; }
        }

        public TextureDimension Dimension
        {
            get { return (TextureDimension)wgpuTextureDesc.dimension; }
        }

        public TextureUsage Usage
        {
            get { return (TextureUsage)wgpuTextureDesc.usage; }
        }

        public int Width
        {
            get { return (int)wgpuTextureDesc.size.width; }
        }

        public int Height
        {
            get { return (int)wgpuTextureDesc.size.height; }
        }

        public int DepthOrLayers
        {
            get { return (int)wgpuTextureDesc.size.depthOrArrayLayers; }
        }

        public int MipLevelCount
        {
            get { return (int)wgpuTextureDesc.mipLevelCount; }
        }

        public int SampleCount
        {
            get { return (int)wgpuTextureDesc.sampleCount; }
        }

        // Constructor
        internal Texture(Wgpu.TextureImpl wgpuTexture, in Wgpu.TextureDescriptor wgpuTextureDesc)
        {
            this.wgpuTexture = wgpuTexture;
            this.wgpuTextureDesc = wgpuTextureDesc;
        }

        // Methods
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(wgpuTexture.Handle != IntPtr.Zero)
            {
                // Release any created views
                if (createdViews != null)
                {
                    foreach (TextureView view in createdViews)
                    {
                        // Release view
                        Wgpu.TextureViewRelease(view.wgpuTextureView);
                        view.wgpuTextureView = default;
                    }
                    createdViews.Clear();
                    createdViews = null;
                }

                // Release texture
                Wgpu.TextureDestroy(wgpuTexture);
                Wgpu.TextureRelease(wgpuTexture);
                wgpuTexture = default;

                // Zero desc
                wgpuTextureDesc = default;
            }
        }

        public TextureView CreateView(TextureAspect aspect = TextureAspect.All)
        {
            // Create desc
            Wgpu.TextureViewDescriptor wgpuTextureViewDesc = new Wgpu.TextureViewDescriptor
            {
                label = "View",
                format = wgpuTextureDesc.format,
                dimension = wgpuTextureDesc.dimension switch
                { 
                    Wgpu.TextureDimension.OneDimension => Wgpu.TextureViewDimension.OneDimension,
                    Wgpu.TextureDimension.TwoDimensions => Wgpu.TextureViewDimension.TwoDimensions,
                    Wgpu.TextureDimension.ThreeDimensions => Wgpu.TextureViewDimension.ThreeDimensions,
                    _ => throw new NotSupportedException("Texture View Format: " + (TextureDimension)wgpuTextureDesc.dimension),
                },
                baseMipLevel = 0,
                mipLevelCount = wgpuTextureDesc.mipLevelCount,
                baseArrayLayer = 0,
                arrayLayerCount = wgpuTextureDesc.size.depthOrArrayLayers,
                aspect = (Wgpu.TextureAspect)aspect,
            };

            // Create view
            Wgpu.TextureViewImpl wgpuTextureView = Wgpu.TextureCreateView(wgpuTexture, wgpuTextureViewDesc);

            // Check for error
            if (wgpuTextureView.Handle == IntPtr.Zero)
                return null;

            // Create view result
            TextureView textureViewResult = new TextureView(wgpuTextureView, this);

            // Register view
            if (createdViews == null) createdViews = new HashSet<TextureView>();
            createdViews.Add(textureViewResult);

            // Create view
            return textureViewResult;
        }

        public TextureView CreateView(TextureFormat format, TextureViewDimension dimension, TextureAspect aspect = TextureAspect.All, int baseMipLevel = 0, int mipLevelCount = 1, int baseArrayLayer = 0, int arrayLayerCount = 0)
        {
            // Create desc
            Wgpu.TextureViewDescriptor wgpuTextureViewDesc = new Wgpu.TextureViewDescriptor
            {
                label = "View",
                format = (Wgpu.TextureFormat)format,
                dimension = (Wgpu.TextureViewDimension)dimension,
                baseMipLevel = (uint)baseMipLevel,
                mipLevelCount = (uint)mipLevelCount,
                baseArrayLayer = (uint)baseArrayLayer,
                arrayLayerCount = (uint)arrayLayerCount,
                aspect = (Wgpu.TextureAspect)aspect,
            };

            // Create view
            Wgpu.TextureViewImpl wgpuTextureView = Wgpu.TextureCreateView(wgpuTexture, wgpuTextureViewDesc);

            // Check for error
            if (wgpuTextureView.Handle == IntPtr.Zero)
                return null;

            // Create view result
            TextureView textureViewResult = new TextureView(wgpuTextureView, this);

            // Register view
            if (createdViews == null) createdViews = new HashSet<TextureView>();
            createdViews.Add(textureViewResult);

            // Create view
            return textureViewResult;
        }

        internal void ReleaseView(TextureView view)
        {
            if(createdViews != null && createdViews.Contains(view) == true)
                createdViews.Remove(view);
        }
    }
}
