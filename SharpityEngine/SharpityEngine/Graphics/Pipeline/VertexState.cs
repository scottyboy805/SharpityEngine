
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
        private ulong offset;
        private uint shaderLocation;

        // Properties
        public VertexFormat Format
        {
            get { return format; }
        }

        public ulong Offset
        {
            get { return offset; }
        }

        public uint ShaderLocation
        {
            get { return shaderLocation; }
        }

        // Constructor
        public VertexAttribute(VertexFormat format, ulong offset, uint shaderLocation)
        {
            this.format = format;
            this.offset = offset;
            this.shaderLocation = shaderLocation;
        }
    }

    public struct VertexBufferLayout
    {
        // Private
        private ulong arrayStride;
        private VertexStepMode stepMode;
        private VertexAttribute[] attributes;

        // Properties
        public ulong ArrayStride
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
        public VertexBufferLayout(ulong arrayStride, params VertexAttribute[] attributes)
        {
            this.arrayStride = arrayStride;
            this.stepMode = VertexStepMode.Vertex;
            this.attributes = attributes;
        }

        public VertexBufferLayout(ulong arrayStride, VertexStepMode stepMode, params VertexAttribute[] attributes)
        {
            this.arrayStride = arrayStride;
            this.stepMode = stepMode;
            this.attributes = attributes;
        }
    }

    public struct VertexState
    {
        // Private
        private Shader shader;
        private string entryPoint;
        private VertexBufferLayout[] bufferLayouts;

        // Public
        public const string DefaultEntryPoint = "vs_main";

        // Properties
        public Shader Shader
        {
            get { return shader; }
        }

        public string EntryPoint
        {
            get { return entryPoint; }
        }

        public VertexBufferLayout[] BufferLayouts
        {
            get { return bufferLayouts; }
        }

        // Constructor
        public VertexState(Shader shader, params VertexBufferLayout[] bufferLayouts)
        {
            this.shader = shader;
            this.entryPoint = DefaultEntryPoint;
            this.bufferLayouts = bufferLayouts;
        }

        public VertexState(Shader shader, string entryPoint, params VertexBufferLayout[] bufferLayouts)
        {
            this.shader = shader;
            this.entryPoint = entryPoint;
            this.bufferLayouts = bufferLayouts;
        }
    }
}
