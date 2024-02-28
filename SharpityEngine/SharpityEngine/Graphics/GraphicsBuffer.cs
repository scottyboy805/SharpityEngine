using WGPU.NET;
using Buffer = WGPU.NET.Buffer;

namespace SharpityEngine.Graphics
{
    public enum GraphicsBufferUsage
    {
        None = 0,
        MapRead = 1,
        MapWrite = 2,
        CopySrc = 4,
        CopyDst = 8,
        Index = 0x10,
        Vertex = 0x20,
        Uniform = 0x40,
        Storage = 0x80,
        Indirect = 0x100,
        QueryResolve = 0x200,
    }

    public sealed class GraphicsBuffer : IDisposable
    {
        // Private
        private Device device = null;
        private Buffer buffer = null;

        // Properties
        public ulong SizeInBytes
        {
            get { return buffer.SizeInBytes; }
        }

        // Constructor
        internal GraphicsBuffer(Device device, Buffer buffer)
        {
            this.device = device;
            this.buffer = buffer;
        }

        // Methods
        public void Dispose()
        {
            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
        }

        public void Write<T>(ReadOnlySpan<T> data, long bufferOffset = 0) where T : unmanaged
        {
            // Write buffer
            device.Queue.WriteBuffer(buffer, (ulong)bufferOffset, data);
        }

        private void CheckDisposed()
        {
            if (buffer == null)
                throw new ObjectDisposedException(nameof(GraphicsBuffer));
        }
    }
}
