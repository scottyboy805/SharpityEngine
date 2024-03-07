using SharpityEngine.Graphics.Pipeline;
using System.Runtime.InteropServices;
using WGPU.NET;

namespace SharpityEngine.Graphics
{
    public enum GraphicsPowerMode
    {
        LowPower = 1,
        HighPower = 2,
    }

    public enum GraphicsBackend
    {
        Default = 0,
        Null = 1,
        WebGPU = 2,
        D3D11 = 3,
        D3D12 = 4,
        Metal = 5,
        Vulkan = 6,
        OpenGL = 7,
        OpenGLES = 8,
    }

    public sealed class GraphicsDevice : IDisposable
    {
        // Internal
        internal Wgpu.InstanceImpl wgpuInstance;
        internal Wgpu.DeviceImpl wgpuDevice;

        // Private
        private GraphicsAdapter adapter = null;

        // Properties
        public GraphicsAdapter Adapter
        {
            get { return adapter; }
        }

        // Constructor
        internal GraphicsDevice(Wgpu.InstanceImpl wgpuInstance, Wgpu.DeviceImpl wgpuDevice, GraphicsAdapter adapter)
        {
            this.wgpuInstance = wgpuInstance;
            this.wgpuDevice = wgpuDevice;
            this.adapter = adapter;

            // Set error callback
            Wgpu.ErrorCallback errorCallback = (type, message, _) => ErrorCallback(type, message);
            Wgpu.DeviceSetUncapturedErrorCallback(wgpuDevice, errorCallback, IntPtr.Zero);
        }

        // Methods
        public void Dispose()
        {
            if (wgpuDevice.Equals(default) == false)
            {
                Wgpu.DeviceRelease(wgpuDevice);
                wgpuDevice = default;
            }
        }

        public GraphicsBuffer CreateBuffer(ulong size, GraphicsBufferUsage usage)
        {
            // Create desc
            Wgpu.BufferDescriptor desc = new Wgpu.BufferDescriptor
            {
                label = null,
                mappedAtCreation = 0u,
                size = size,
                usage = (uint)usage,
            };

            // Create buffer
            Wgpu.BufferImpl buffer = Wgpu.DeviceCreateBuffer(wgpuDevice, desc);

            // Check for error
            if (buffer.Handle == IntPtr.Zero)
                return null;

            // Get result
            return new GraphicsBuffer(wgpuDevice, buffer, desc);
        }

        public GraphicsBuffer CreateBuffer<T>(ReadOnlySpan<T> data, GraphicsBufferUsage usage) where T : unmanaged
        {
            // Create desc
            Wgpu.BufferDescriptor desc = new Wgpu.BufferDescriptor
            {
                label = null,
                mappedAtCreation = 1u,
                size = (ulong)data.Length * (ulong)Marshal.SizeOf<T>(),
                usage = (uint)usage,
            };

            // Create buffer
            Wgpu.BufferImpl buffer = Wgpu.DeviceCreateBuffer(wgpuDevice, desc);

            // Check for error
            if (buffer.Handle == IntPtr.Zero)
                return null;

            // Get result
            GraphicsBuffer bufferCreated = new GraphicsBuffer(wgpuDevice, buffer, desc);

            // Copy data
            data.CopyTo(bufferCreated.MapRange<T>(0, data.Length));

            // Unmap
            bufferCreated.Unmap();

            // Get result
            return bufferCreated;
        }

        public Texture CreateTexture1D(int length, TextureFormat format, TextureUsage usage = TextureUsage.TextureBinding, int mipLevel = 1, int sampleCount = 1)
        {
            // Create desc
            Wgpu.TextureDescriptor wgpuTextureDesc = new Wgpu.TextureDescriptor
            {
                label = null,
                usage = (uint)usage,
                dimension = Wgpu.TextureDimension.OneDimension,
                size = new Wgpu.Extent3D
                {
                    width = (uint)length,
                },
                format = (Wgpu.TextureFormat)format,
                mipLevelCount = (uint)mipLevel,
                sampleCount = (uint)sampleCount,
            };

            // Create texture
            Wgpu.TextureImpl wgpuTexture = Wgpu.DeviceCreateTexture(wgpuDevice, wgpuTextureDesc);

            // Check for error
            if (wgpuTexture.Handle == IntPtr.Zero)
                return null;

            // Get result
            return new Texture(wgpuDevice, wgpuTexture, wgpuTextureDesc);
        }

        public Texture CreateTexture2D(int width, int height, TextureFormat format, TextureUsage usage = TextureUsage.TextureBinding, int mipLevel = 1, int sampleCount = 1)
        {
            // Create desc
            Wgpu.TextureDescriptor wgpuTextureDesc = new Wgpu.TextureDescriptor
            {
                label = null,
                usage = (uint)usage,
                dimension = Wgpu.TextureDimension.TwoDimensions,
                size = new Wgpu.Extent3D
                {
                    width = (uint)width,
                    height = (uint)height,
                },
                format = (Wgpu.TextureFormat)format,
                mipLevelCount = (uint)mipLevel,
                sampleCount = (uint)sampleCount,
            };

            // Create texture
            Wgpu.TextureImpl wgpuTexture = Wgpu.DeviceCreateTexture(wgpuDevice, wgpuTextureDesc);

            // Check for error
            if (wgpuTexture.Handle == IntPtr.Zero)
                return null;

            // Get result
            return new Texture(wgpuDevice, wgpuTexture, wgpuTextureDesc);
        }

        public Texture CreateTexture3D(int width, int height, int depth, TextureFormat format, TextureUsage usage = TextureUsage.TextureBinding, int mipLevel = 1, int sampleCount = 1)
        {
            // Create desc
            Wgpu.TextureDescriptor wgpuTextureDesc = new Wgpu.TextureDescriptor
            {
                label = null,
                usage = (uint)usage,
                dimension = Wgpu.TextureDimension.ThreeDimensions,
                size = new Wgpu.Extent3D
                {
                    width = (uint)width,
                    height = (uint)height,
                    depthOrArrayLayers = (uint)depth,
                },
                format = (Wgpu.TextureFormat)format,
                mipLevelCount = (uint)mipLevel,
                sampleCount = (uint)sampleCount,
            };

            // Create texture
            Wgpu.TextureImpl wgpuTexture = Wgpu.DeviceCreateTexture(wgpuDevice, wgpuTextureDesc);

            // Check for error
            if (wgpuTexture.Handle == IntPtr.Zero)
                return null;

            // Get result
            return new Texture(wgpuDevice, wgpuTexture, wgpuTextureDesc);
        }

        //public Shader CreateShader()
        //{

        //}

        //public GraphicsRenderPipeline CreateRenderPipeline(Shader shader)
        //{

        //}

        internal void OnDeviceLost(string reason)
        {

        }

        private static void ErrorCallback(Wgpu.ErrorType type, string message)
        {
            Debug.LogErrorF(LogFilter.Graphics, "Device error!: [{0}] - {1}", type, message);
        }
    }
}
