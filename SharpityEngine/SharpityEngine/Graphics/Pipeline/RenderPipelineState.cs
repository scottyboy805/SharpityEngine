using System.Runtime.InteropServices;
using WGPU.NET;
using static WGPU.NET.Wgpu;

namespace SharpityEngine.Graphics.Pipeline
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PrimitiveState
    {
        // Public
        public PrimitiveTopology Topology;
        public IndexFormat StripIndexFormat;
        public FrontFace FrontFace;
        public CullMode CullMode;

        // Constructor
        public PrimitiveState(PrimitiveTopology topology, IndexFormat stripIndexFormat, FrontFace frontFace = FrontFace.CCW, CullMode cullMode = CullMode.Back)
        {
            this.Topology = topology;
            this.StripIndexFormat = stripIndexFormat;
            this.FrontFace = frontFace;
            this.CullMode = cullMode;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MultisampleState
    {
        // Public
        public int Count;
        public uint Mask;
        public bool AlphaToCoverageEnabled;

        // Constructor
        public MultisampleState(int count, uint max = uint.MaxValue, bool alphaToCoverageEnabled = false) 
        { 
            this.Count = count;
            this.Mask = max;
            this.AlphaToCoverageEnabled = alphaToCoverageEnabled;
        }
    }

    public struct StencilFaceState
    {
        // Public
        public CompareFunction Compare;
        public StencilOperation FailOp;
        public StencilOperation PassOp;
        public StencilOperation DepthFailOp;

        // Constructor
        public StencilFaceState(CompareFunction compare, StencilOperation failOp = StencilOperation.Keep, StencilOperation passOp = StencilOperation.Keep, StencilOperation depthFailOp = StencilOperation.Keep) 
        {
            this.Compare = compare;
            this.FailOp = failOp;
            this.PassOp = passOp;
            this.DepthFailOp = depthFailOp;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DepthStencilState
    {
        // Private
        private IntPtr unused;

        // Public
        public TextureFormat Format;
        public bool DepthWriteEnabled;
        public CompareFunction DepthCompare;
        public StencilFaceState StencilFront;
        public StencilFaceState StencilBack;
        public uint StencilReadMask;
        public uint StencilWriteMask;
        public int DepthBias;
        public float DepthBiasSlopeScale;
        public float DepthBiasClamp;

        // Constructor
        public DepthStencilState(TextureFormat format, CompareFunction depthCompare, StencilFaceState stencilFront)
        {
            this.Format = format;
            this.DepthCompare = depthCompare;
            this.StencilFront = stencilFront;
            this.StencilBack = stencilFront;
        }

        public DepthStencilState(TextureFormat format, CompareFunction depthCompare, StencilFaceState stencilFront, StencilFaceState stencilBack)
        {
            this.Format = format;
            this.DepthCompare = depthCompare;
            this.StencilFront = stencilFront;
            this.StencilBack = stencilBack;
        }

        // Methods
        internal Wgpu.DepthStencilState GetDepthStencilState()
        {
            return new Wgpu.DepthStencilState
            {
                format = (Wgpu.TextureFormat)Format,
                depthWriteEnabled = DepthWriteEnabled ? 1u : 0u,
                depthCompare = (Wgpu.CompareFunction)DepthCompare,
                stencilFront = new Wgpu.StencilFaceState
                {
                    compare = (Wgpu.CompareFunction)StencilFront.Compare,
                    failOp = (Wgpu.StencilOperation)StencilFront.FailOp,
                    passOp = (Wgpu.StencilOperation)StencilFront.PassOp,
                    depthFailOp = (Wgpu.StencilOperation)StencilFront.DepthFailOp,
                },
                stencilBack = new Wgpu.StencilFaceState
                {
                    compare = (Wgpu.CompareFunction)StencilBack.Compare,
                    failOp = (Wgpu.StencilOperation)StencilBack.FailOp,
                    passOp = (Wgpu.StencilOperation)StencilBack.PassOp,
                    depthFailOp = (Wgpu.StencilOperation)StencilBack.DepthFailOp,
                },
                stencilReadMask = StencilReadMask,
                stencilWriteMask = StencilWriteMask,
                depthBias = DepthBias,
                depthBiasSlopeScale = DepthBiasSlopeScale,
                depthBiasClamp = DepthBiasClamp,
            };
        }
    }

    public struct RenderPipelineState
    {
        // Public
        public VertexState VertexState;
        public PrimitiveState PrimitiveState;
        public MultisampleState MultisampleState;
        public FragmentState? FragmentState;
        public DepthStencilState? DepthStencilState;

        // Constructor
        public RenderPipelineState(in VertexState vertexState, in PrimitiveState primitiveState, MultisampleState multisampleState, DepthStencilState? depthStencilState = null)
        {
            this.VertexState = vertexState;
            this.PrimitiveState = primitiveState;
            this.MultisampleState = multisampleState;
            this.DepthStencilState = depthStencilState;
        }

        public RenderPipelineState(in VertexState vertexState, in FragmentState fragmentState, in PrimitiveState primitiveState, MultisampleState multisampleState, DepthStencilState? depthStencilState = null)
        {
            this.VertexState = vertexState;
            this.PrimitiveState = primitiveState;
            this.FragmentState = fragmentState;
            this.MultisampleState = multisampleState;
            this.DepthStencilState = depthStencilState;
        }
    }
}
