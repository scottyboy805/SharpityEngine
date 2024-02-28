
namespace SharpityEngine.Graphics.Pipeline
{
    public enum BlendFactor
    {
        Zero = 0,
        One = 1,
        Src = 2,
        OneMinusSrc = 3,
        SrcAlpha = 4,
        OneMinusSrcAlpha = 5,
        Dst = 6,
        OneMinusDst = 7,
        DstAlpha = 8,
        OneMinusDstAlpha = 9,
        SrcAlphaSaturated = 10,
        Constant = 11,
        OneMinusConstant = 12,
    }

    public enum BlendOperation
    {
        Add = 0,
        Subtract = 1,
        ReverseSubtract = 2,
        Min = 3,
        Max = 4,
    }

    [Flags]
    public enum ColorWriteMask
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 4,
        Alpha = 8,
        All = 0xF,
    }

    public struct BlendComponent
    {
        // Private
        private BlendOperation operation;
        private BlendFactor srcFactor;
        private BlendFactor dstFactor;

        // Properties
        public BlendOperation BlendOperation
        {
            get { return operation; }
        }

        public BlendFactor SrcBlendFactor
        {
            get { return SrcBlendFactor; }
        }

        public BlendFactor DstBlendFactor
        {
            get { return dstFactor; }
        }

        // Constructor
        public BlendComponent(BlendOperation blendOperation, BlendFactor srcFactor, BlendFactor dstFactor)
        {
            this.operation = blendOperation;
            this.srcFactor = srcFactor;
            this.dstFactor = dstFactor;
        }
    }

    public struct BlendState
    {
        // Private
        private BlendComponent color;
        private BlendComponent alpha;

        // Properties
        public BlendComponent Color
        {
            get { return color; }
        }

        public BlendComponent Alpha
        {
            get { return alpha; }
        }

        // Constructor
        public BlendState(BlendComponent color, BlendComponent alpha)
        {
            this.color = color;
            this.alpha = alpha;
        }
    }

    public struct ColorTargetState
    {
        // Private
        private TextureFormat format;
        private BlendState blendState;
        private bool hasBlendState;
        private ColorWriteMask writeMask;

        // Properties
        public TextureFormat Format
        {
            get { return format; }
        }

        public BlendState BlendState
        {
            get { return blendState; }
        }

        public bool HasBlendState
        {
            get { return hasBlendState; }
        }

        public ColorWriteMask WriteMask
        {
            get { return writeMask; }
        }

        // Constructor
        public ColorTargetState(TextureFormat format, ColorWriteMask writeMask)
        {
            this.format = format;
            this.blendState = default;
            this.hasBlendState = false;
            this.writeMask = writeMask;
        }

        public ColorTargetState(TextureFormat format, BlendState blendState, ColorWriteMask writeMask)
        {
            this.format = format;
            this.blendState = blendState;
            this.hasBlendState = true;
            this.writeMask = writeMask;
        }
    }

    public struct FragmentState
    {
        // Private
        private Shader shader;
        private string entryPoint;
        private ColorTargetState[] colorTargets;

        // Public
        public const string DefaultEntryPoint = "fs_main";

        // Properties
        public Shader Shader
        {
            get { return shader; }
        }

        public string EntryPoint
        {
            get { return entryPoint; }
        }

        public ColorTargetState[] ColorTargets
        {
            get { return colorTargets; }
        }

        // Constructor
        public FragmentState(Shader shader, params ColorTargetState[] colorTargets)
        {
            this.shader = shader;
            this.entryPoint = DefaultEntryPoint;
            this.colorTargets = colorTargets;
        }

        public FragmentState(Shader shader, string entryPoint, params ColorTargetState[] colorTargets)
        {
            this.shader = shader;
            this.entryPoint = entryPoint;
            this.colorTargets = colorTargets;
        }
    }
}
