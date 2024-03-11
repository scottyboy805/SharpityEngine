using System.Runtime.Serialization;
using WGPU.NET;

namespace SharpityEngine.Graphics.Pipeline
{
    public abstract class BindLayoutData
    {
        // Type
        internal sealed class BufferBindLayoutData : BindLayoutData
        {
            // Private
            [DataMember(Name = "BufferType")]
            private BufferBindingType bufferType = BufferBindingType.Uniform;
            [DataMember(Name = "MinBindingSize")]
            private long minBindingSize = 0;

            // Properties
            public BufferBindingType BufferType
            {
                get { return bufferType; }
            }

            public long MinBindingSize
            {
                get { return minBindingSize; }
            }

            // Constructor
            public BufferBindLayoutData() { }

            public BufferBindLayoutData(BufferBindingType bufferType, long minBindingSize, int bindSlot, ShaderStage shaderStage)
                : base(bindSlot, shaderStage)
            {
                this.bufferType = bufferType;
                this.minBindingSize = minBindingSize;
            }

            // Methods
            internal override Wgpu.BindGroupLayoutEntry GetLayoutEntry()
            {
                return new Wgpu.BindGroupLayoutEntry
                {
                    binding = (uint)BindingSlot,
                    visibility = (uint)ShaderStage,
                    buffer = new Wgpu.BufferBindingLayout
                    {
                        type = (Wgpu.BufferBindingType)bufferType,
                        hasDynamicOffset = 0,
                        minBindingSize = (ulong)minBindingSize,
                    },
                };
            }
        }

        internal sealed class SamplerBindLayoutData : BindLayoutData
        {
            // Private
            [DataMember(Name = "SamplerType")]
            private SamplerBindingType samplerType = SamplerBindingType.Filtering;

            // Properties
            public SamplerBindingType SamplerType
            {
                get { return samplerType; }
            }

            // Constructor
            public SamplerBindLayoutData() { }

            public SamplerBindLayoutData(SamplerBindingType samplerType, int bindSlot, ShaderStage shaderStage)
                : base(bindSlot, shaderStage)
            {
                this.samplerType = samplerType;
            }

            // Methods
            internal override Wgpu.BindGroupLayoutEntry GetLayoutEntry()
            {
                return new Wgpu.BindGroupLayoutEntry
                {
                    binding = (uint)BindingSlot,
                    visibility = (uint)ShaderStage,
                    sampler = new Wgpu.SamplerBindingLayout
                    {
                        type = (Wgpu.SamplerBindingType)samplerType,
                    },
                };
            }
        }

        internal sealed class TextureBindLayoutData : BindLayoutData
        {
            // Private
            [DataMember(Name = "SampleType")]
            private TextureSampleType sampleType = TextureSampleType.Float;
            [DataMember(Name = "ViewDimension")]
            private TextureViewDimension viewDimension = TextureViewDimension.Texture2D;
            [DataMember(Name = "Multisampled")]
            private bool multisampled = true;

            // Properties
            public TextureSampleType SampleType
            {
                get { return sampleType; }
            }

            public TextureViewDimension ViewDimension
            {
                get { return viewDimension; }
            }

            public bool Multisampled
            {
                get { return multisampled; }
            }

            // Constructor
            public TextureBindLayoutData() { }

            public TextureBindLayoutData(TextureSampleType sampleType, TextureViewDimension viewDimension, bool multisampled, int bindSlot, ShaderStage shaderStage)
                : base(bindSlot, shaderStage)
            {
                this.sampleType = sampleType;
                this.viewDimension = viewDimension;
                this.multisampled = multisampled;
            }

            // Methods
            internal override Wgpu.BindGroupLayoutEntry GetLayoutEntry()
            {
                return new Wgpu.BindGroupLayoutEntry
                {
                    binding = (uint)BindingSlot,
                    visibility = (uint)ShaderStage,
                    texture = new Wgpu.TextureBindingLayout
                    {
                        sampleType = (Wgpu.TextureSampleType)sampleType,
                        viewDimension = (Wgpu.TextureViewDimension)viewDimension,
                        multisampled = multisampled ? 1u : 0u,
                    },
                };
            }
        }

        internal sealed class StorageTextureBindLayoutData : BindLayoutData
        {
            // Private
            [DataMember(Name = "Format")]
            private TextureFormat format = TextureFormat.RGBA32Float;
            [DataMember(Name = "ViewDimension")]
            private TextureViewDimension viewDimension = TextureViewDimension.Texture2D;

            // Properties
            public TextureFormat Format
            {
                get { return format; }
            }

            public TextureViewDimension ViewDimension
            {
                get { return viewDimension; }
            }

            // Constructor
            public StorageTextureBindLayoutData() { }

            public StorageTextureBindLayoutData(TextureFormat format, TextureViewDimension viewDimension, int bindSlot, ShaderStage shaderStage)
                : base(bindSlot, shaderStage)
            {
                this.format = format;
                this.viewDimension = viewDimension;
            }

            // Methods
            internal override Wgpu.BindGroupLayoutEntry GetLayoutEntry()
            {
                return new Wgpu.BindGroupLayoutEntry
                {
                    binding = (uint)BindingSlot,
                    visibility = (uint)ShaderStage,
                    storageTexture = new Wgpu.StorageTextureBindingLayout
                    {
                        format = (Wgpu.TextureFormat)format,
                        viewDimension = (Wgpu.TextureViewDimension)viewDimension,
                        access = Wgpu.StorageTextureAccess.WriteOnly,
                    },
                };
            }
        }

        // Private
        [DataMember(Name = "BindingSlot")]
        private int bindingSlot = 0;
        [DataMember(Name = "ShaderStage")]
        private ShaderStage shaderStage = ShaderStage.Vertex;

        // Properties
        public int BindingSlot
        {
            get { return bindingSlot; }
        }

        public ShaderStage ShaderStage
        {
            get { return shaderStage; }
        }

        // Constructor
        protected BindLayoutData() { }

        protected BindLayoutData(int bindSlot, ShaderStage shaderStage)
        {
            this.bindingSlot = bindSlot;
            this.shaderStage = shaderStage;
        }

        // Methods
        internal abstract Wgpu.BindGroupLayoutEntry GetLayoutEntry();

        public static BindLayoutData Buffer(BufferBindingType bufferType, long minBindingSize, int bindSlot, ShaderStage shaderStage)
        {
            return new BufferBindLayoutData(bufferType, minBindingSize, bindSlot, shaderStage);
        }

        public static BindLayoutData Sampler(SamplerBindingType samplerType, int bindSlot, ShaderStage shaderStage)
        {
            return new SamplerBindLayoutData(samplerType, bindSlot, shaderStage);
        }

        public static BindLayoutData Texture(TextureSampleType sampleType, TextureViewDimension viewDimension, int bindSlot, ShaderStage shaderStage, bool multisampled = false)
        {
            return new TextureBindLayoutData(sampleType, viewDimension, multisampled, bindSlot, shaderStage);
        }

        public static BindLayoutData StorageTexture(TextureFormat format, TextureViewDimension viewDimension, int bindSlot, ShaderStage shaderStage)
        {
            return new StorageTextureBindLayoutData(format, viewDimension, bindSlot, shaderStage);
        }
    }

    public sealed class BindGroupLayout : IDisposable
    {
        // Internal
        internal Wgpu.BindGroupLayoutImpl wgpuBindGroupLayout;

        // Constructor
        internal BindGroupLayout(Wgpu.BindGroupLayoutImpl wgpuBindGroupLayout)
        {
            this.wgpuBindGroupLayout = wgpuBindGroupLayout;
        }

        // Methods
        public void Dispose()
        {
            if(wgpuBindGroupLayout.Handle != IntPtr.Zero)
            {
                // Release bind group
                Wgpu.BindGroupLayoutRelease(wgpuBindGroupLayout);
                wgpuBindGroupLayout = default;
            }
        }
    }
}
