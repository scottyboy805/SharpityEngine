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
