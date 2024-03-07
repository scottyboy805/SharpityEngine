using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WGPU.NET;

namespace SharpityEngine.Graphics
{
    public enum QueueWorkDoneStatus : int
    {
        Success = 0x00000000,
        Error = 0x00000001,
        Unknown = 0x00000002,
        DeviceLost = 0x00000003,
    }

    public sealed class GraphicsQueue : IDisposable
    {
        // Internal
        internal Wgpu.QueueImpl wgpuQueue;

        // Constructor
        internal GraphicsQueue(Wgpu.QueueImpl wgpuQueue)
        {
            this.wgpuQueue = wgpuQueue;
        }

        // Methods
        public void Dispose()
        {
            if(wgpuQueue.Handle != IntPtr.Zero)
            {
                // Release queue
                Wgpu.QueueRelease(wgpuQueue);
                wgpuQueue = default;
            }
        }

        public void OnSubmittedWorkDone(Action<QueueWorkDoneStatus> callback)
        {
            Wgpu.QueueOnSubmittedWorkDone(wgpuQueue, 
                (status, _) => callback((QueueWorkDoneStatus)status), 
                IntPtr.Zero);
        }

        public unsafe void Submit(CommandBuffer[] commands)
        {
            // Get start of array
            fixed(Wgpu.CommandBufferImpl* cmd = &commands[0].wgpuCommandBuffer)
            {
                Wgpu.QueueSubmit(wgpuQueue, (uint)commands.Length, ref *cmd);
            }            
        }

        public unsafe void WriteBuffer<T>(GraphicsBuffer buffer, ulong offset, ReadOnlySpan<T> data) where T : unmanaged
        {
            ulong tSize = (ulong)sizeof(T);

            // Write buffer
            Wgpu.QueueWriteBuffer(wgpuQueue, buffer.wgpuBuffer, offset, 
                (IntPtr)Unsafe.AsPointer(ref MemoryMarshal.GetReference(data)),
                (ulong)data.Length * tSize);
        }
    }
}
