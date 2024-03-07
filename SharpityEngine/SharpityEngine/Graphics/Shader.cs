using SharpityEngine.Graphics.Pipeline;
using WGPU.NET;

namespace SharpityEngine.Graphics
{
    public enum PrimitiveTopology
    {
        PointList = 0,
        LineList = 1,
        LineStrip = 2,
        TriangleList = 3,
        TriangleStrip = 4,
    }

    public enum IndexFormat
    {
        Undefined = 0,
        Uint16 = 1,
        Uint32 = 2,
    }

    public enum FrontFace
    {
        CCW = 0,
        CW = 1,
    }

    public enum CullMode
    {
        None = 0,
        Front = 1,
        Back = 2,
    }

    public sealed class Shader : DataAsset
    {
        // Private
        private Device device = null;
        private ShaderModule shader = null;
        private BindGroupLayout bindGroupLayout = null;

        private FrontFace frontFace = FrontFace.CCW;
        private CullMode cullMode = CullMode.Back;

        private Pipeline.VertexState vertexState;
        private Pipeline.FragmentState fragmentState;
        private BindingLayout[] bindingLayoutGroup;

        // Properties
        public FrontFace FrontFace
        {
            get { return frontFace; }
        }

        public CullMode CullMode
        {
            get { return cullMode; }
        }

        public Pipeline.VertexState VertexState
        {
            get { return vertexState; }
        }
        
        public Pipeline.FragmentState FragmentState
        {
            get { return fragmentState; }
        }

        public BindingLayout[] BindingLayoutGroup
        {
            get { return bindingLayoutGroup; }
        }

        // Constructor
        internal Shader(Device device, ShaderModule shader)
        {
            this.device = device;
            this.shader = shader;

            // Create binding group
            bindGroupLayout = CreateBindGroup(device);
        }

        // Methods
        protected override void OnDestroy()
        {
            if(shader != null)
            {
                // Release shader
                shader.Dispose();
                shader = null;

                // Release binding group
                bindGroupLayout.Dispose();
                bindGroupLayout = null;
            }
        }

        private BindGroupLayout CreateBindGroup(Device device)
        {
            return device.CreateBindgroupLayout("Layout");
        }
    }
}
