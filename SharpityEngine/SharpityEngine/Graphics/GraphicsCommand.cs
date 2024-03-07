using WGPU.NET;

namespace SharpityEngine.Graphics
{
    public struct CommandBuffer : IDisposable
    {
        // Internal
        internal Wgpu.CommandBufferImpl wgpuCommandBuffer;

        // Constructor
        internal CommandBuffer(Wgpu.CommandBufferImpl wgpuCommandBuffer)
        {
            if (wgpuCommandBuffer.Handle == IntPtr.Zero)
                throw new Exception("Failed to create command buffer");

            this.wgpuCommandBuffer = wgpuCommandBuffer;
        }

        // Constructor
        public void Dispose()
        {
            if(wgpuCommandBuffer.Handle != IntPtr.Zero)
            {
                // Release command
                Wgpu.CommandBufferRelease(wgpuCommandBuffer);
                wgpuCommandBuffer = default;
            }
        }
    }

    internal class GraphicsCommand
    {
    }
}
