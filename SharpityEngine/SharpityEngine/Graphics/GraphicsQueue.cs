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

        //public unsafe void WriteTexture<T>(Texture texture, ReadOnlySpan<T> data, int bytesPerRow, int rowsPerImage, int offset = 0, TextureAspect aspect = TextureAspect.All, Point3 origin = default, int mipLevel = 0)
        //{
        //    // Create image copy
        //    Wgpu.ImageCopyTexture copyDesc = new Wgpu.ImageCopyTexture
        //    {
        //        texture = texture.wgpuTexture,
        //        aspect = (Wgpu.TextureAspect)aspect,
        //        origin = new Wgpu.Origin3D { x = (uint)origin.X, y = (uint)origin.Y, z = (uint)origin.Z },
        //        mipLevel = (uint)mipLevel,
        //    };

        //    // Create texture layout
        //    Wgpu.TextureDataLayout layoutDesc = new Wgpu.TextureDataLayout
        //    {
        //        bytesPerRow = (uint)bytesPerRow,
        //        rowsPerImage = (uint)rowsPerImage,
        //        offset = (uint)offset,
        //    };

        //    ulong tSize = (ulong)sizeof(T);

        //    // Write texture
        //    Wgpu.QueueWriteTexture(wgpuQueue, copyDesc,
        //        (IntPtr)Unsafe.AsPointer(ref MemoryMarshal.GetReference(data)),
        //        (ulong)data.Length * (ulong)tSize,
        //        layoutDesc, )
        //}
    }
}
