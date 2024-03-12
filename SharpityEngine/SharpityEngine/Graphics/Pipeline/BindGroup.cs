using WGPU.NET;

namespace SharpityEngine.Graphics.Pipeline
{
    public abstract class BindData
    {
        // Type
        internal sealed class BufferBindData : BindData
        {
            // Private
            private GraphicsBuffer buffer = null;
            private long offset = 0;
            private long size = 0;

            // Properties
            public GraphicsBuffer Buffer
            {
                get { return buffer; }
            }

            public long Offset
            {
                get { return offset; }
            }

            public long Size
            {
                get { return size; }
            }

            // Constructor
            public BufferBindData(GraphicsBuffer buffer, int bindSlot, long offset, long size)
                : base(bindSlot)
            {
                this.buffer = buffer;
                this.offset = offset;
                this.size = size;
            }

            // Methods
            internal override Wgpu.BindGroupEntry GetEntry()
            {
                return new Wgpu.BindGroupEntry
                {
                    binding = (uint)BindingSlot,
                    offset = (ulong)Offset,
                    size = (ulong)Size,
                    buffer = buffer != null ? buffer.wgpuBuffer : default,
                };
            }
        }

        internal sealed class SamplerBindData : BindData
        {
            // Private
            private Sampler sampler = null;

            // Properties
            public Sampler Sampler
            {
                get { return sampler; }
            }

            // Constructor
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
                    sampler = sampler != null ? sampler.wgpuSampler : default,
                };
            }
        }

        internal sealed class TextureBindData : BindData
        {
            // Private
            private TextureView textureView = null;

            // Properties
            public TextureView TextureView
            {
                get { return textureView; }
            }

            // Constructor
            public TextureBindData(TextureView textureView, int bindSlot)
                : base(bindSlot)
            {
                this.textureView = textureView;
            }

            // Methods
            internal override Wgpu.BindGroupEntry GetEntry()
            {
                return new Wgpu.BindGroupEntry
                {
                    binding = (uint)BindingSlot,
                    textureView = textureView != null ? textureView.wgpuTextureView : default,
                };
            }
        }

        // Private
        private int bindingSlot = 0;        

        // Properties
        public int BindingSlot
        {
            get { return bindingSlot; }
        }

        // Constructor
        protected BindData() { }

        protected BindData(int bindingSlot)
        {
            this.bindingSlot = bindingSlot;
        }

        // Methods
        internal abstract Wgpu.BindGroupEntry GetEntry();

        public static BindData Buffer(GraphicsBuffer buffer, int bindSlot, long offset, long size)
        {
            return new BufferBindData(buffer, bindSlot, offset, size);
        }

        public static BindData Sampler(Sampler sampler, int bindSlot)
        {
            return new SamplerBindData(sampler, bindSlot);
        }

        public static BindData Texture(TextureView textureView, int bindSlot)
        {
            return new TextureBindData(textureView, bindSlot);
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
