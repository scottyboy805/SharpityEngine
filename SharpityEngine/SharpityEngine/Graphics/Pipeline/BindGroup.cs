using WGPU.NET;

namespace SharpityEngine.Graphics.Pipeline
{
    public sealed class BufferBindData : BindData
    {
        // Private
        private GraphicsBuffer buffer = null;

        // Properties
        public GraphicsBuffer Buffer
        {
            get { return buffer; }
        }

        // Constructor
        public BufferBindData(GraphicsBuffer buffer, int bindSlot, long offset, long size)
            : base(bindSlot, offset, size)
        {
            this.buffer = buffer;
        }

        // Methods
        internal override Wgpu.BindGroupEntry GetEntry()
        {
            return new Wgpu.BindGroupEntry
            {
                binding = (uint)BindingSlot,
                offset = (ulong)Offset,
                size = (ulong)Size,
                buffer = buffer.wgpuBuffer,
            };
        }
    }

    public sealed class SamplerBindData : BindData
    {
        // Private
        private Sampler sampler = null;

        // Properties
        public Sampler Sampler
        {
            get { return sampler; }
        }

        // Constructor
        public SamplerBindData(Sampler sampler, int bindSlot, long offset, long size)
            : base(bindSlot, offset, size)
        {
            this.sampler = sampler;
        }

        // Methods
        internal override Wgpu.BindGroupEntry GetEntry()
        {
            return new Wgpu.BindGroupEntry
            {
                binding = (uint)BindingSlot,
                offset = (ulong)Offset,
                size = (ulong)Size,
                sampler = sampler.wgpuSampler,
            };
        }
    }

    public sealed class TextureBindData : BindData
    {
        // Private
        private TextureView textureView = null;

        // Properties
        public TextureView TextureView
        {
            get { return textureView; }
        }

        // Constructor
        public TextureBindData(TextureView textureView,  int bindSlot, long offset, long size)
            : base(bindSlot, offset, size)
        {
            this.textureView = textureView;
        }

        // Methods
        internal override Wgpu.BindGroupEntry GetEntry()
        {
            return new Wgpu.BindGroupEntry
            {
                binding = (uint)BindingSlot,
                offset = (ulong)Offset,
                size = (ulong)Size,
                textureView = textureView.wgpuTextureView,
            };
        }
    }

    public abstract class BindData
    {
        // Private
        private int bindingSlot = 0;
        private long offset = 0;
        private long size = 0;

        // Properties
        public int BindingSlot
        {
            get { return bindingSlot; }
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
        protected BindData() { }

        protected BindData(int bindingSlot, long offset, long size)
        {
            this.bindingSlot = bindingSlot;
            this.offset = offset;
            this.size = size;
        }

        // Methods
        internal abstract Wgpu.BindGroupEntry GetEntry();
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
