using SharpityEngine.Graphics.Context;
using WGPU.NET;
using Buffer = WGPU.NET.Buffer;

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
        private Instance instance = null;
        private GraphicsAdapter adapter = null;
        private Device device = null;

        // Properties
        public GraphicsAdapter Adapter
        {
            get { return adapter; }
        }

        // Constructor
        internal GraphicsDevice(Instance instance, GraphicsAdapter adapter, Device device)
        {
            this.instance = instance;
            this.adapter = adapter;
            this.device = device;

            // Set error callback
            device.SetUncapturedErrorCallback(ErrorCallback);
        }

        // Methods
        public void Dispose()
        {
            if (instance != null)
            {
                instance.Dispose();
                instance = null;
            }
        }

        public GraphicsBuffer CreateBuffer(ulong size, GraphicsBufferUsage usage)
        {
            // Try to create buffer
            Buffer buffer = device.CreateBuffer(string.Empty, false, size, (Wgpu.BufferUsage)usage);

            // Check for created
            if (buffer == null)
                return null;

            // Create buffer
            return new GraphicsBuffer(device, buffer);
        }

        public GraphicsBuffer CreateBuffer<T>(ReadOnlySpan<T> data, GraphicsBufferUsage usage) where T : unmanaged
        {
            // Try to create buffer
            Buffer buffer = device.CreateBuffer(string.Empty, true, (ulong)data.Length, (Wgpu.BufferUsage)usage);

            // Check for created
            if (buffer == null)
                return null;
            
            // Copy data
            data.CopyTo(buffer.GetMappedRange<T>(0, data.Length));
            
            // Unmap
            buffer.Unmap();

            // Create buffer
            return new GraphicsBuffer(device, buffer);
        }

        public Texture CreateTexture1D(int length, TextureFormat format, TextureUsage usage = TextureUsage.TextureBinding, int mipLevel = 1, int sampleCount = 1)
        {
            // Create size
            Wgpu.Extent3D size = new Wgpu.Extent3D
            {
                width = (uint)length,
                height = (uint)0,
                depthOrArrayLayers = 0,
            };

            // Create texture
            WGPU.NET.Texture texture = device.CreateTexture(
                string.Empty,
                (Wgpu.TextureUsage)usage,
                (Wgpu.TextureDimension)TextureDimension.Texture1D,
                size,
                (Wgpu.TextureFormat)format,
                (uint)mipLevel, (uint)sampleCount);

            // Check for created
            if (texture == null)
                return null;

            return new Texture(device, texture, format, TextureDimension.Texture2D, mipLevel, sampleCount);
        }

        public Texture CreateTexture2D(int width, int height, TextureFormat format, TextureUsage usage = TextureUsage.TextureBinding, int mipLevel = 1, int sampleCount = 1)
        {
            // Create size
            Wgpu.Extent3D size = new Wgpu.Extent3D
            {
                width = (uint)width,
                height = (uint)height,
                depthOrArrayLayers = 0,
            };

            // Create texture
            WGPU.NET.Texture texture = device.CreateTexture(
                string.Empty,
                (Wgpu.TextureUsage)usage,
                (Wgpu.TextureDimension)TextureDimension.Texture2D,
                size,
                (Wgpu.TextureFormat)format,
                (uint)mipLevel, (uint)sampleCount);

            // Check for created
            if (texture == null)
                return null;

            return new Texture(device, texture, format, TextureDimension.Texture2D, mipLevel, sampleCount);
        }

        public Texture CreateTexture3D(int width, int height, int depth, TextureFormat format, TextureUsage usage = TextureUsage.TextureBinding, int mipLevel = 1, int sampleCount = 1)
        {
            // Create size
            Wgpu.Extent3D size = new Wgpu.Extent3D
            {
                width = (uint)width,
                height = (uint)height,
                depthOrArrayLayers = (uint)depth,
            };

            // Create texture
            WGPU.NET.Texture texture = device.CreateTexture(
                string.Empty,
                (Wgpu.TextureUsage)usage,
                (Wgpu.TextureDimension)TextureDimension.Texture3D,
                size,
                (Wgpu.TextureFormat)format,
                (uint)mipLevel, (uint)sampleCount);

            // Check for created
            if (texture == null)
                return null;

            return new Texture(device, texture, format, TextureDimension.Texture3D, mipLevel, sampleCount);
        }

        public GraphicsSwapChain CreateSwapChain(IGraphicsContext surface, TextureFormat format = 0, PresentMode presentMode = PresentMode.Fifo)
        {
            // Get surface preferred format
            TextureFormat swapChainFormat = format == 0
                ? (TextureFormat)adapter.surface.GetPreferredFormat(adapter.adapter)
                : format;

            // Create swap chain
            return CreateSwapChain(surface.RenderWidth, surface.RenderHeight, swapChainFormat, presentMode);
        }

        public GraphicsSwapChain CreateSwapChain(int width, int height, TextureFormat format, PresentMode presentMode = PresentMode.Fifo)
        {
            // Create description
            Wgpu.SwapChainDescriptor desc = new Wgpu.SwapChainDescriptor
            {
                usage = (uint)Wgpu.TextureUsage.RenderAttachment,
                width = (uint)width,
                height = (uint)height,
                format = (Wgpu.TextureFormat)format,
                presentMode = (Wgpu.PresentMode)presentMode,
            };

            // Create swap chain
            SwapChain swapChain = device.CreateSwapChain(adapter.surface, desc);

            // Check for created
            if (swapChain == null)
                return null;

            // Create swap chain
            return new GraphicsSwapChain(swapChain, width, height, format, presentMode);
        }

        private static void ErrorCallback(Wgpu.ErrorType type, string message)
        {
            Debug.LogErrorF(LogFilter.Graphics, "Device error!: [{0}] - {1}", type, message);
        }

        public static async Task<GraphicsDevice> Create(GameWindow window, GraphicsBackend backend, GraphicsPowerMode powerMode = GraphicsPowerMode.HighPower)
        {
            // Create adapter
            GraphicsAdapter adapter = await GraphicsAdapter.CreateAsync(window, backend, powerMode);

            // Check for created
            if (adapter == null)
                return null;

            // Create device
            return await adapter.RequestDeviceAsync();
        }
    }
}
