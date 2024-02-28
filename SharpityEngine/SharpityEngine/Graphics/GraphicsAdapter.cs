using SharpityEngine.Graphics.Context;
using System.Runtime.InteropServices;
using WGPU.NET;

namespace SharpityEngine.Graphics
{
    public struct GraphicsLimits
    {
        // Internal
        internal Wgpu.Limits limits = default;

        // Properties
        public int MaxTextureDimension1D => (int)limits.maxTextureDimension1D;
        public int MaxTextureDimension2D => (int)limits.maxTextureDimension2D;
        public int MaxTextureDimension3D => (int)limits.maxTextureDimension3D;
        public int MaxTextureArrayLayers => (int)limits.maxTextureArrayLayers;
        public int MaxBindGroups => (int)limits.maxBindGroups;
        public int MaxBindingsPerBindGroup => (int)limits.maxBindingsPerBindGroup;
        public int MaxDynamicUniformBuffersPerPipelineLayout => (int)limits.maxDynamicUniformBuffersPerPipelineLayout;
        public int MaxDynamicStorageBuffersPerPipelineLayout => (int)limits.maxDynamicStorageBuffersPerPipelineLayout;
        public int MaxSampledTexturesPerShaderStage => (int)limits.maxSampledTexturesPerShaderStage;
        public int MaxSamplersPerShaderStage => (int)limits.maxSamplersPerShaderStage;
        public int MaxStorageBuffersPerShaderStage => (int)limits.maxStorageBuffersPerShaderStage;
        public int MaxStorageTexturesPerShaderStage => (int)limits.maxStorageTexturesPerShaderStage;
        public int MaxUniformBuffersPerShaderStage => (int)limits.maxUniformBuffersPerShaderStage;
        public long MaxUniformBufferBindingSize => (long)limits.maxUniformBufferBindingSize;
        public long MaxStorageBufferBindingSize => (long)limits.maxStorageBufferBindingSize;
        public int MinUniformBufferOffsetAlignment => (int)limits.minUniformBufferOffsetAlignment;
        public int MinStorageBufferOffsetAlignment => (int)limits.minStorageBufferOffsetAlignment;
        public int MaxVertexBuffers => (int)limits.maxVertexBuffers;
        public long MaxBufferSize => (long)limits.maxBufferSize;
        public int MaxVertexAttributes => (int)limits.maxVertexAttributes;
        public int MaxVertexBufferArrayStride => (int)limits.maxVertexBufferArrayStride;
        public int MaxInterStageShaderComponents => (int)limits.maxInterStageShaderComponents;
        public int MaxInterStageShaderVariables => (int)limits.maxInterStageShaderVariables;
        public int MaxColorAttachments => (int)limits.maxColorAttachments;
        public int MaxColorAttachmentBytesPerSample => (int)limits.maxColorAttachmentBytesPerSample;
        public int MaxComputeWorkgroupStorageSize => (int)limits.maxComputeWorkgroupStorageSize;
        public int MaxComputeInvocationsPerWorkgroup => (int)limits.maxComputeInvocationsPerWorkgroup;
        public int MaxComputeWorkgroupSizeX => (int)limits.maxComputeWorkgroupSizeX;
        public int MaxComputeWorkgroupSizeY => (int)limits.maxComputeWorkgroupSizeY;
        public int MaxComputeWorkgroupSizeZ => (int)limits.maxComputeWorkgroupSizeZ;
        public int MaxComputeWorkgroupsPerDimension => (int)limits.maxComputeWorkgroupsPerDimension;

        // Constructor
        internal GraphicsLimits(Wgpu.Limits limits)
        {
            this.limits = limits;
        }
    }

    public sealed class GraphicsAdapter : IDisposable
    {
        // Internal
        internal Instance instance = null;
        internal Adapter adapter = null;
        internal Surface surface = null;

        // Private
        private Wgpu.AdapterProperties properties = default;
        private GraphicsLimits limits = default;

        // Properties
        public uint VendorID
        {
            get { return properties.vendorID; }
        }

        public string VendorName
        {
            get { return properties.vendorName; }
        }

        public string Architecture
        {
            get { return properties.architecture; }
        }

        public uint DeviceID
        {
            get { return properties.deviceID; }
        }

        public string DeviceName
        {
            get { return properties.name; }
        }

        public string DriverDescription
        {
            get { return properties.driverDescription; }
        }

        public GraphicsLimits Limits
        {
            get { return limits; }
        }

        // Constructor
        internal GraphicsAdapter(Instance instance, Adapter adapter , Surface surface)
        {
            this.instance = instance;
            this.adapter = adapter;
            this.surface = surface;

            // Get properties
            adapter.GetProperties(out properties);

            // Get limits
            Wgpu.SupportedLimits supportedLimits;
            adapter.GetLimits(out supportedLimits);

            // Create limits
            limits = new GraphicsLimits(supportedLimits.limits);            
        }

        // Methods
        public void Dispose()
        {
            if (instance != null)
            {
                // Release adapter
                adapter.Dispose();
                adapter = null;

                // Release instance
                instance.Dispose();
                instance = null;
            }
        }

        public async Task<GraphicsDevice> RequestDeviceAsync()
        {
            Device result = null;
            bool completed = false;

            // Called on completion
            RequestDeviceCallback callback = (Wgpu.RequestDeviceStatus status, Device device, string message) =>
            {
                // Set completed
                completed = true;

                // Check for status
                if (status == Wgpu.RequestDeviceStatus.Success)
                {
                    result = device;
                }
                else
                {
                    Debug.LogErrorF(LogFilter.Graphics, "Failed to create device!: [{0}] - {1}", status, message);
                }
            };

            // Try to create device
            adapter.RequestDevice(
                callback, "Device", 
                Array.Empty<Wgpu.NativeFeature>(),
                limits: limits.limits);

            // Wait for completed
            while (completed == false)
                await Task.Delay(10);

            // Create device
            return new GraphicsDevice(instance, this, result);
        }


        public static async Task<GraphicsAdapter> CreateAsync(GameWindow window, GraphicsBackend backend, GraphicsPowerMode powerMode)
        {
            // Create instance
            Instance instance = new Instance();

            // Create surface
            Surface surface = CreateSurface(instance, window, backend);

            // Check for surface
            if (surface == null)
                return null;


            Adapter result = null;
            bool completed = false;

            // Called on completion
            RequestAdapterCallback callback = (Wgpu.RequestAdapterStatus status, Adapter adapter, string message) =>
            {
                // Set completed
                completed = true;

                // Check for status
                if (status == Wgpu.RequestAdapterStatus.Success)
                {
                    result = adapter;
                }
                else
                {
                    Debug.LogErrorF("Failed to create adapter!: [{0}] - {1}", status, message);
                }
            };

            // Try to create adapter
            instance.RequestAdapter(surface,
                (Wgpu.PowerPreference)powerMode,
                false, callback,
                (Wgpu.BackendType)backend);
            
            // Wait for completed
            while (completed == false)
                await Task.Delay(10);

            // Get adapter
            return new GraphicsAdapter(instance, result, surface);
        }

        private static Surface CreateSurface(Instance instance, GameWindow window, GraphicsBackend backend)
        {
            // Check platform
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == true)
            {
                // Check for windows native
                if (window is IGraphicsContext_WindowsNative)
                {
                    // Create windows surface
                    IntPtr hinstance, hwnd;

                    // Get window pointer
                    ((IGraphicsContext_WindowsNative)window).GetWindowNative(out hinstance, out hwnd);

                    // Check for valid
                    //if(hinstance != IntPtr.Zero && hwnd != IntPtr.Zero)
                        return instance.CreateSurfaceFromWindowsHWND(hinstance, hwnd, "GameWindow");
                }

                Debug.LogError("Could not create surface for native windows platform!");
                return null;
            }

            //// Check platform
            //if (window is IGraphicsContext_WindowsNative)
            //{
            //    // Create surface
            //    return instance.CreateSurfaceFromWindowsHWND(
            //        ((IGraphicsContext_WindowsNative)window).HInstance,
            //        ((IGraphicsContext_WindowsNative)window).HWND);
            //}

            Debug.LogError("Could not create surface!");
            return null;
        }
    }
}
