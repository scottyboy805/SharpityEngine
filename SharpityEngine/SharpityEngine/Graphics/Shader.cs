using SharpityEngine.Graphics.Pipeline;
using System.Runtime.Serialization;
using WGPU.NET;

namespace SharpityEngine.Graphics
{
    public enum ShaderStage
    {
        None = 0,
        Vertex = 1,
        Fragment = 2,
        Compute = 4,
    }

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
        // Internal
        internal Wgpu.DeviceImpl wgpuDevice;
        internal Wgpu.ShaderModuleImpl wgpuShader;
        internal BindGroupLayout bindGroupLayout = null;
        internal RenderPipelineLayout renderPipelineLayout = null;
        internal RenderPipeline renderPipeline = null;

        // Private
        [DataMember(Name = "Topology")]
        private PrimitiveTopology topology = PrimitiveTopology.TriangleList;
        [DataMember(Name = "FrontFace")]
        private FrontFace frontFace = FrontFace.CCW;
        [DataMember(Name = "CullMode")]
        private CullMode cullMode = CullMode.Back;
        [DataMember(Name = "VertexState")]
        private VertexState vertexState = new VertexState(Mesh.MeshBufferLayout);
        [DataMember(Name = "FragmentState")]
        private FragmentState fragmentState = new FragmentState(
                new ColorTargetState(TextureFormat.BGRA8Unorm, ColorWriteMask.All,
                    new BlendState(
                        new BlendComponent(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero),
                        new BlendComponent(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero))));
        [DataMember(Name = "BindingLayoutData")]
        private unsafe BindLayoutData[] bindingLayoutData = 
        {
            BindLayoutData.Buffer(BufferBindingType.Uniform, sizeof(Matrix4), 0, ShaderStage.Vertex),
            BindLayoutData.Sampler(SamplerBindingType.Filtering, 1, ShaderStage.Fragment),
            BindLayoutData.Texture(TextureSampleType.Float, TextureViewDimension.Texture2D, 2, ShaderStage.Fragment)
        };
        [DataMember(Name = "MultisampleState")]
        private MultisampleState multisampleState = new MultisampleState(1);
        [DataMember(Name = "DepthStenclState")]
        private DepthStencilState depthStencilState = new DepthStencilState(TextureFormat.Depth32Float, CompareFunction.Always, new StencilFaceState(CompareFunction.Always));
        [DataMember(Name = "RenderQueue")]
        private int renderQueue = 1000;

        // Properties
        public PrimitiveTopology Topology
        {
            get { return topology; }
        }

        public FrontFace FrontFace
        {
            get { return frontFace; }
        }

        public CullMode CullMode
        {
            get { return cullMode; }
        }

        public VertexState VertexState
        {
            get { return vertexState; }
        }

        public FragmentState FragmentState
        {
            get { return fragmentState; }
        }

        public int RenderQueue
        {
            get { return renderQueue; }
            set { renderQueue = value; }
        }

        // Constructor
        internal Shader(Wgpu.DeviceImpl wgpuDevice, string name = null)
            : base(name)
        {
            this.wgpuDevice = wgpuDevice;
        }

        internal Shader(Wgpu.DeviceImpl wgpuDevice, Wgpu.ShaderModuleImpl wgpuShader, string shaderSource, string name = null)
            : base(shaderSource, name)
        {
            this.wgpuDevice = wgpuDevice;
            this.wgpuShader = wgpuShader;

            // Trigger loaded
            OnAfterLoaded();
        }

        // Methods
        protected override void OnLoaded()
        {
            // Call base
            base.OnLoaded();

            // Create pipeline
            CreateRenderPipeline();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (wgpuShader.Handle != IntPtr.Zero)
            {
                // Release render pipeline
                renderPipeline.Dispose();
                renderPipeline = null;

                // Release pipeline layout
                renderPipelineLayout.Dispose();
                renderPipelineLayout = null;

                // Release bind group layout
                bindGroupLayout.Dispose();
                bindGroupLayout = null;

                // Release shader
                Wgpu.ShaderModuleRelease(wgpuShader);
                wgpuShader = default;
            }
        }

        private void CreateRenderPipeline()
        {
            // Get surface format
            TextureFormat surfaceFormat = Game.GraphicsSurface.GetPreferredFormat(Game.GraphicsAdapter);

            // Update fragment state
            for (int i = 0; i < fragmentState.ColorTargets.Length; i++)
                fragmentState.ColorTargets[i].Format = surfaceFormat;

            // Create bind group layout
            bindGroupLayout = Game.GraphicsDevice.CreateBindGroupLayout(bindingLayoutData, Name + " Binding Group Layout");

            // Create pipeline layout
            renderPipelineLayout = Game.GraphicsDevice.CreateRenderPipelineLayout(new[] { bindGroupLayout }, Name + " Render Pipeline Layout");

            // Create render pipeline
            renderPipeline = Game.GraphicsDevice.CreateRenderPipeline(renderPipelineLayout, this, 
                vertexState,
                new PrimitiveState(topology, IndexFormat.Undefined, frontFace, cullMode), 
                multisampleState,
                fragmentState, 
                depthStencilState,
                Name + " Render Pipeline");
        }
    }
}
