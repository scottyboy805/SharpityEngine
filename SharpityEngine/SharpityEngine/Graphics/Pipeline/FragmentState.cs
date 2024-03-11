
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using WGPU.NET;

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
            get { return srcFactor; }
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
        // Public
        public TextureFormat Format;
        public ColorWriteMask WriteMask;
        public BlendState? BlendState;

        // Constructor
        public ColorTargetState(TextureFormat format, ColorWriteMask writeMask, BlendState? blendState = null)
        {
            this.Format = format;
            this.WriteMask = writeMask;
            this.BlendState = blendState;
        }

        // Methods
        internal unsafe Wgpu.ColorTargetState GetColorState()
        {
            IntPtr blendPtr = IntPtr.Zero;

            // Blend state
            if(BlendState != null)
            {
                // Create blend state
                Wgpu.BlendState wgpuBlendState = new Wgpu.BlendState
                {
                    color = new Wgpu.BlendComponent
                    {
                        operation = (Wgpu.BlendOperation)BlendState?.Color.BlendOperation,
                        srcFactor = (Wgpu.BlendFactor)BlendState?.Color.SrcBlendFactor,
                        dstFactor = (Wgpu.BlendFactor)BlendState?.Color.DstBlendFactor,
                    },
                    alpha = new Wgpu.BlendComponent
                    {
                        operation = (Wgpu.BlendOperation)BlendState?.Alpha.BlendOperation,
                        srcFactor = (Wgpu.BlendFactor)BlendState?.Alpha.SrcBlendFactor,
                        dstFactor = (Wgpu.BlendFactor)BlendState?.Alpha.DstBlendFactor,
                    },
                };

                // Update ptr
                blendPtr = new IntPtr(&wgpuBlendState);
            }

            // Create desc
            return new Wgpu.ColorTargetState
            {
                format = (Wgpu.TextureFormat)Format,
                writeMask = (uint)WriteMask,
                blend = blendPtr,
            };
        }
    }

    public struct FragmentState
    {
        // Private
        private string entryPoint;
        private ColorTargetState[] colorTargets;

        // Public
        public const string DefaultEntryPoint = "fs_main";

        // Properties
        public string EntryPoint
        {
            get { return entryPoint; }
        }

        public ColorTargetState[] ColorTargets
        {
            get { return colorTargets; }
        }

        // Constructor
        public FragmentState(params ColorTargetState[] colorTargets)
        {
            this.entryPoint = DefaultEntryPoint;
            this.colorTargets = colorTargets;
        }

        public FragmentState(string entryPoint, params ColorTargetState[] colorTargets)
        {
            this.entryPoint = entryPoint;
            this.colorTargets = colorTargets;
        }

        // Methods
        internal unsafe Wgpu.FragmentState GetFragmentState(Shader shader)
        {
            // Create array
            Span<Wgpu.ColorTargetState> wgpuColorTargetStates = stackalloc Wgpu.ColorTargetState[colorTargets.Length];

            // Fill data
            for(int i = 0; i < colorTargets.Length; i++)
                wgpuColorTargetStates[i] = colorTargets[i].GetColorState();

            // Create desc
            return new Wgpu.FragmentState
            {
                module = shader.wgpuShader,
                entryPoint = entryPoint,
                targets = new IntPtr(Unsafe.AsPointer(ref wgpuColorTargetStates.GetPinnableReference())),
                targetCount = (uint)colorTargets.Length,
            };
        }
    }
}
