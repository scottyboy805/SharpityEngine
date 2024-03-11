
using System.Runtime.CompilerServices;
using WGPU.NET;

namespace SharpityEngine.Graphics.Pipeline
{
    public enum VertexStepMode
    {
        Vertex = 0,
        Instance = 1,
        VertexBufferNotUsed = 2,
    }

    public enum VertexFormat
    {
        Uint8x2 = 1,
        Uint8x4 = 2,
        Sint8x2 = 3,
        Sint8x4 = 4,
        Unorm8x2 = 5,
        Unorm8x4 = 6,
        Snorm8x2 = 7,
        Snorm8x4 = 8,
        Uint16x2 = 9,
        Uint16x4 = 10,
        Sint16x2 = 11,
        Sint16x4 = 12,
        Unorm16x2 = 13,
        Unorm16x4 = 14,
        Snorm16x2 = 0xF,
        Snorm16x4 = 0x10,
        Float16x2 = 17,
        Float16x4 = 18,
        Float32 = 19,
        Float32x2 = 20,
        Float32x3 = 21,
        Float32x4 = 22,
        Uint32 = 23,
        Uint32x2 = 24,
        Uint32x3 = 25,
        Uint32x4 = 26,
        Sint32 = 27,
        Sint32x2 = 28,
        Sint32x3 = 29,
        Sint32x4 = 30,
    }

    public struct VertexAttribute
    {
        // Private
        private VertexFormat format;
        private long offset;
        private int shaderLocation;

        // Properties
        public VertexFormat Format
        {
            get { return format; }
        }

        public long Offset
        {
            get { return offset; }
        }

        public int ShaderLocation
        {
            get { return shaderLocation; }
        }

        // Constructor
        public VertexAttribute(VertexFormat format, long offset, int shaderLocation)
        {
            this.format = format;
            this.offset = offset;
            this.shaderLocation = shaderLocation;
        }
    }

    public struct VertexBufferLayout
    {
        // Private
        private long arrayStride;
        private VertexStepMode stepMode;
        private VertexAttribute[] attributes;

        // Properties
        public long ArrayStride
        {
            get { return arrayStride; }
        }

        public VertexStepMode StepMode
        {
            get { return stepMode; }
        }

        public VertexAttribute[] Attributes
        {
            get { return attributes; }
        }
        
        // Constructor
        public VertexBufferLayout(long arrayStride, params VertexAttribute[] attributes)
        {
            this.arrayStride = arrayStride;
            this.stepMode = VertexStepMode.Vertex;
            this.attributes = attributes;
        }

        public VertexBufferLayout(long arrayStride, VertexStepMode stepMode, params VertexAttribute[] attributes)
        {
            this.arrayStride = arrayStride;
            this.stepMode = stepMode;
            this.attributes = attributes;
        }

        // Methods
        internal unsafe Wgpu.VertexBufferLayout GetBufferLayout()
        {
            // Create array
            Span<Wgpu.VertexAttribute> wgpuVertexAttributes = stackalloc Wgpu.VertexAttribute[attributes.Length];

            // Fill data
            for (int i = 0; i < attributes.Length; i++)
            {
                wgpuVertexAttributes[i] = new Wgpu.VertexAttribute
                {
                    format = (Wgpu.VertexFormat)attributes[i].Format,
                    offset = (ulong)attributes[i].Offset,
                    shaderLocation = (uint)attributes[i].ShaderLocation,
                };
            }

            // Create desc
            return new Wgpu.VertexBufferLayout
            {
                arrayStride = (ulong)arrayStride,
                stepMode = (Wgpu.VertexStepMode)stepMode,
                attributes = new IntPtr(Unsafe.AsPointer(ref wgpuVertexAttributes.GetPinnableReference())),
                attributeCount = (uint)attributes.Length
            };
        }
    }

    public struct VertexState
    {
        // Private
        private string entryPoint;
        private VertexBufferLayout[] bufferLayouts;

        // Public
        public const string DefaultEntryPoint = "vs_main";

        // Properties
        public string EntryPoint
        {
            get { return entryPoint; }
        }

        public VertexBufferLayout[] BufferLayouts
        {
            get { return bufferLayouts; }
        }

        // Constructor
        public VertexState(params VertexBufferLayout[] bufferLayouts)
        {
            this.entryPoint = DefaultEntryPoint;
            this.bufferLayouts = bufferLayouts;
        }

        public VertexState(string entryPoint, params VertexBufferLayout[] bufferLayouts)
        {
            this.entryPoint = entryPoint;
            this.bufferLayouts = bufferLayouts;
        }

        // Methods
        internal unsafe Wgpu.VertexState GetVertexState(Shader shader)
        {
            // Create array
            Span<Wgpu.VertexBufferLayout> wgpuBufferLayouts = stackalloc Wgpu.VertexBufferLayout[bufferLayouts.Length];

            // Fill data
            for (int i = 0; i < bufferLayouts.Length; i++)
                wgpuBufferLayouts[i] = bufferLayouts[i].GetBufferLayout();

            // Create state
            return new Wgpu.VertexState
            {
                module = shader.wgpuShader, 
                entryPoint = entryPoint,
                buffers = new IntPtr(Unsafe.AsPointer(ref wgpuBufferLayouts.GetPinnableReference())),
                bufferCount = (uint)bufferLayouts.Length,
            };
        }
    }
}
