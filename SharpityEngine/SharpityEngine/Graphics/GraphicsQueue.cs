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

        public void WriteTexture<T>(Span<T> data, Texture texture, in TextureDataLayout layout)
        {
            WriteTexture<T>((ReadOnlySpan<T>)data, texture, layout);
        }

        public void WriteTexture<T>(ReadOnlySpan<T> data, Texture texture, in TextureDataLayout layout)
        {
            WriteTexture(data, new TextureCopy(texture), layout);
        }

        public void WriteTexture<T>(Span<T> data, in TextureCopy copy, in TextureDataLayout layout, int width = -1, int height = -1, int depthOrArrayLayers = -1)
        {
            WriteTexture<T>((ReadOnlySpan<T>)data, copy, layout, width, height, depthOrArrayLayers);
        }

        public unsafe void WriteTexture<T>(ReadOnlySpan<T> data, in TextureCopy copy, in TextureDataLayout layout, int width = -1, int height = -1, int depthOrArrayLayers = -1)
        {
            // Create image copy
            Wgpu.ImageCopyTexture copyDesc = new Wgpu.ImageCopyTexture
            {
                texture = copy.Texture.wgpuTexture,
                aspect = (Wgpu.TextureAspect)copy.Aspect,
                origin = new Wgpu.Origin3D { x = (uint)copy.Origin.X, y = (uint)copy.Origin.Y, z = (uint)copy.Origin.Z },
                mipLevel = (uint)copy.MipLevel,
            };

            // Create texture layout
            Wgpu.TextureDataLayout layoutDesc = new Wgpu.TextureDataLayout
            {
                bytesPerRow = (uint)layout.BytesPerRow,
                rowsPerImage = (uint)layout.RowsPerTexture,
                offset = (uint)layout.Offset,
            };

            // Create write size
            Wgpu.Extent3D writeSizeDesc = new Wgpu.Extent3D
            {
                width = (uint)(width == -1 ? copy.Texture.Width : width),
                height = (uint)(height == -1 ? copy.Texture.Height : height),
                depthOrArrayLayers = (uint)(depthOrArrayLayers == -1 ? copy.Texture.DepthOrLayers : depthOrArrayLayers),
            };
            
            // Get size of data element
            ulong tSize = (ulong)sizeof(T);

            // Write texture
            Wgpu.QueueWriteTexture(wgpuQueue, copyDesc,
                (IntPtr)Unsafe.AsPointer(ref MemoryMarshal.GetReference(data)),
                (ulong)data.Length * (ulong)tSize,
                layoutDesc, writeSizeDesc);
        }
    }
}
