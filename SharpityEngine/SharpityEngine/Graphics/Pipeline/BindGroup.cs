using System.Runtime.Serialization;
using WGPU.NET;

namespace SharpityEngine.Graphics.Pipeline
{
    [DataContract]
    public abstract class BindData
    {
        // Type
        [DataContract]
        internal sealed class BufferBindData : BindData
        {
            // Private            
            [DataMember(Name = "Usage")]
            private BufferUsage usage = BufferUsage.Uniform | BufferUsage.CopyDst;
            [DataMember(Name = "Offset")]
            private long offset = 0;
            [DataMember(Name = "Size")]
            private long size = 0;

            private GraphicsBuffer buffer = null;

            // Properties
            public BufferUsage Usage
            {
                get { return usage; }
            }

            public long Offset
            {
                get { return offset; }
            }

            public long Size
            {
                get { return size; }
            }

            public new GraphicsBuffer Buffer
            {
                get 
                {
                    // Create buffer on demand
                    if (buffer == null && Device != null)
                        buffer = Device.CreateBuffer(size, usage);

                    return buffer; 
                }
            }

            // Constructor
            private BufferBindData() { }

            public BufferBindData(int bindSlot, long offset = 0, long size = -1, BufferUsage usage = BufferUsage.Uniform | BufferUsage.CopyDst)
                : base(bindSlot)
            {
                this.usage = usage;
                this.offset = offset;
                this.size = size < 0 ? buffer.SizeInBytes : size;
            }

            public BufferBindData(GraphicsBuffer buffer, int bindSlot, long offset = 0, long size = -1)
                : base(bindSlot)
            {                
                this.buffer = buffer;
                this.usage = buffer.Usage;
                this.offset = offset;
                this.size = size < 0 ? buffer.SizeInBytes : size;
            }

            // Methods
            internal override Wgpu.BindGroupEntry GetEntry()
            {
                return new Wgpu.BindGroupEntry
                {
                    binding = (uint)BindingSlot,
                    offset = (ulong)Offset,
                    size = (ulong)Size,
                    buffer = Buffer != null ? Buffer.wgpuBuffer : default,
                };
            }
        }

        [DataContract]
        internal sealed class SamplerBindData : BindData
        {
            // Private
            [DataMember(Name = "Wrap")]
            private WrapMode wrapMode = WrapMode.ClampToEdge;
            [DataMember(Name = "Filter")]
            private FilterMode filterMode = FilterMode.Linear;

            private Sampler sampler = null;

            // Properties
            public WrapMode WrapMode
            {
                get { return wrapMode; }
            }

            public FilterMode FilterMode
            {
                get { return filterMode; }
            }

            public new Sampler Sampler
            {
                get 
                {
                    // Create sample on demand
                    if (sampler == null && Device != null)
                        sampler = Device.CreateSampler(wrapMode, filterMode);

                    return sampler; 
                }
            }

            // Constructor
            private SamplerBindData() { }

            public SamplerBindData(int bindSlot)
                : base(bindSlot)
            {
            }

            public SamplerBindData(int bindSlot, WrapMode wrapMode, FilterMode filterMode)
                : base(bindSlot)
            {
                this.wrapMode = wrapMode;
                this.filterMode = filterMode;
            }

            public SamplerBindData(Sampler sampler, int bindSlot)
                : base(bindSlot)
            {
                this.sampler = sampler;
            }

            // Methods
            internal override Wgpu.BindGroupEntry GetEntry()
            {
                return new Wgpu.BindGroupEntry
                {
                    binding = (uint)BindingSlot,
                    sampler = Sampler != null ? Sampler.wgpuSampler : default,
                };
            }
        }

        internal sealed class TextureBindData : BindData
        {
            // Private
            [DataMember(Name = "Texture")]
            private Texture texture = null;
            [DataMember(Name = "Aspect")]
            private TextureAspect aspect = TextureAspect.All;

            private TextureView textureView = null;

            // Properties
            public new Texture Texture
            {
                get { return texture; }
            }

            public TextureAspect Aspect
            {
                get { return aspect; }
            }

            public TextureView TextureView
            {
                get 
                {
                    // Create view on demand
                    if (textureView == null && texture != null)
                        textureView = texture.CreateView(aspect);

                    return textureView; 
                }
            }

            // Constructor
            private TextureBindData() { }

            public TextureBindData(int bindSlot, TextureAspect aspect = TextureAspect.All)
                : base(bindSlot)
            {
                this.aspect = aspect;
            }

            public TextureBindData(Texture texture, int bindSlot, TextureAspect aspect = TextureAspect.All)
                : base(bindSlot)
            {
                this.texture = texture;
                this.aspect = aspect;
            }

            // Methods
            internal override Wgpu.BindGroupEntry GetEntry()
            {
                return new Wgpu.BindGroupEntry
                {
                    binding = (uint)BindingSlot,
                    textureView = TextureView != null ? TextureView.wgpuTextureView : default,
                };
            }
        }

        // Private
        [DataMember(Name = "Slot")]
        private int bindingSlot = 0;        

        // Properties
        public int BindingSlot
        {
            get { return bindingSlot; }
        }

        protected GraphicsDevice Device
        {
            get { return Game.Current != null ? Game.Current.GraphicsDevice : null; }
        }

        // Constructor
        protected BindData() { }

        protected BindData(int bindingSlot)
        {
            this.bindingSlot = bindingSlot;
        }

        // Methods
        internal abstract Wgpu.BindGroupEntry GetEntry();

        public static BindData Buffer(int bindSlot, long offset, long size, BufferUsage usage = BufferUsage.Uniform | BufferUsage.CopyDst)
        {
            return new BufferBindData(bindSlot, offset, size, usage);
        }

        public static BindData Buffer(GraphicsBuffer buffer, int bindSlot, long offset, long size)
        {
            return new BufferBindData(buffer, bindSlot, offset, size);
        }

        public static BindData Sampler(int bindSlot, WrapMode wrapMode = WrapMode.ClampToEdge, FilterMode filterMode = FilterMode.Linear)
        {
            return new SamplerBindData(bindSlot, wrapMode, filterMode);
        }

        public static BindData Sampler(Sampler sampler, int bindSlot)
        {
            return new SamplerBindData(sampler, bindSlot);
        }

        public static BindData Texture(Texture texture, int bindSlot, TextureAspect aspect = TextureAspect.All)
        {
            return new TextureBindData(texture, bindSlot, aspect);
        }
    }

    public sealed class BindGroup : IDisposable
    {
        // Internal
        internal Wgpu.BindGroupImpl wgpuBindGroup;

        // Constructor
        internal BindGroup(Wgpu.BindGroupImpl wgpuBindGroup)
        {
            this.wgpuBindGroup = wgpuBindGroup;
        }

        // Methods
        public void Dispose()
        {
            if(wgpuBindGroup.Handle != IntPtr.Zero)
            {
                // Release bind group
                Wgpu.BindGroupRelease(wgpuBindGroup);
                wgpuBindGroup = default;
            }
        }
    }
}
