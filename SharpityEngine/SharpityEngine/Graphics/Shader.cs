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
        // Internal
        internal Wgpu.DeviceImpl wgpuDevice;
        internal Wgpu.ShaderModuleImpl wgpuShader;

        // Private
        private FrontFace frontFace = FrontFace.CCW;
        private CullMode cullMode = CullMode.Back;

        private VertexState vertexState;
        private FragmentState fragmentState;
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

        public VertexState VertexState
        {
            get { return vertexState; }
        }

        public FragmentState FragmentState
        {
            get { return fragmentState; }
        }

        public BindingLayout[] BindingLayoutGroup
        {
            get { return bindingLayoutGroup; }
        }

        // Constructor
        internal Shader(Wgpu.DeviceImpl wgpuDevice)
        {
            this.wgpuDevice = wgpuDevice;

            // Create binding group
            //bindGroupLayout = CreateBindGroup(device);
        }

        internal Shader(Wgpu.DeviceImpl wgpuDevice, Wgpu.ShaderModuleImpl wgpuShader, string shaderSource)
            : base(shaderSource)
        {
            this.wgpuDevice = wgpuDevice;
            this.wgpuShader = wgpuShader;
        }

        // Methods
        protected async override void OnLoaded()
        {
            // Call base
            base.OnLoaded();

            // Check for shader asset
            if(wgpuShader.Handle == IntPtr.Zero)
            {
                // Create desc
                Wgpu.ShaderModuleDescriptor wgpuShaderDesc = new Wgpu.ShaderModuleDescriptor
                {
                    label = Name,
                    nextInChain = new WgpuStructChain()
                        .AddShaderModuleWGSLDescriptor(await GetTextAsync())
                        .GetPointer(),
                };

                // Create shader
                Wgpu.ShaderModuleImpl wgpuShader = Wgpu.DeviceCreateShaderModule(wgpuDevice, wgpuShaderDesc);

                // Check for error
                if (wgpuShader.Handle == IntPtr.Zero)
                {
                    Debug.LogError("Failed to create shader!");
                    return;
                }

                // Store shader
                this.wgpuShader = wgpuShader;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (wgpuShader.Handle != IntPtr.Zero)
            {
                // Release shader
                Wgpu.ShaderModuleRelease(wgpuShader);
                wgpuShader = default;

                //// Release binding group
                //bindGroupLayout.Dispose();
                //bindGroupLayout = null;
            }
        }

        //private BindGroupLayout CreateBindGroup(Device device)
        //{
        //    return device.CreateBindgroupLayout("Layout");
        //}
    }
}
