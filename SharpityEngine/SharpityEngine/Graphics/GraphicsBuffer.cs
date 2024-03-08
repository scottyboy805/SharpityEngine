﻿using System.Runtime.InteropServices;
using WGPU.NET;

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
        // Internal
        internal Wgpu.DeviceImpl wgpuDevice;
        internal Wgpu.BufferImpl wgpuBuffer;
        internal Wgpu.BufferDescriptor wgpuBufferDesc;

        // Properties
        public ulong SizeInBytes
        {
            get { return wgpuBufferDesc.size; }
        }

        public GraphicsBufferUsage Usage
        {
            get { return (GraphicsBufferUsage)wgpuBufferDesc.usage; }
        }

        // Constructor
        internal GraphicsBuffer(Wgpu.DeviceImpl wgpuDevice, Wgpu.BufferImpl wgpuBuffer, Wgpu.BufferDescriptor wgpuBufferDesc)
        {
            this.wgpuDevice = wgpuDevice;
            this.wgpuBuffer = wgpuBuffer;
            this.wgpuBufferDesc = wgpuBufferDesc;
        }

        // Methods
        public void Dispose()
        {
            if (wgpuBuffer.Handle != IntPtr.Zero)
            {
                // Release buffer
                Wgpu.BufferDestroy(wgpuBuffer);
                Wgpu.BufferRelease(wgpuBuffer);
                wgpuBuffer = default;

                // Zero desc
                wgpuBufferDesc = default;
            }
        }

        public unsafe Span<T> MapConstRange<T>(ulong offset, int size)
            where T : unmanaged
        {
            var structSize = (ulong)Marshal.SizeOf<T>();

            void* ptr = (void*)Wgpu.BufferGetConstMappedRange(wgpuBuffer,
                offset * structSize, (ulong)size * structSize);

            return new Span<T>(ptr, size);
        }

        public unsafe Span<T> MapRange<T>(ulong offset, int size)
            where T : unmanaged
        {
            var structSize = (ulong)Marshal.SizeOf<T>();

            void* ptr = (void*)Wgpu.BufferGetMappedRange(wgpuBuffer,
                offset * structSize, (ulong)size * structSize);

            return new Span<T>(ptr, size);
        }

        public void Unmap()
        {
            Wgpu.BufferUnmap(wgpuBuffer);
        }
    }
}