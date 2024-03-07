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
        internal Wgpu.InstanceImpl wgpuInstance;
        internal Wgpu.AdapterImpl wgpuAdapter;
        internal Wgpu.AdapterProperties properties;
        internal Wgpu.SupportedLimits limits;

        // Private        
        private GraphicsLimits graphicsLimits = default;

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
            get { return graphicsLimits; }
        }

        // Constructor
        internal GraphicsAdapter(Wgpu.InstanceImpl wgpuInstance, Wgpu.AdapterImpl wgpuAdapter)
        {
            this.wgpuInstance = wgpuInstance;
            this.wgpuAdapter = wgpuAdapter;

            // Get properties
            Wgpu.AdapterGetProperties(wgpuAdapter, ref properties);

            // Get limits
            Wgpu.AdapterGetLimits(wgpuAdapter, ref limits);

            // Create limits
            graphicsLimits = new GraphicsLimits(limits.limits);            
        }

        // Methods
        public void Dispose()
        {
            // Check for already released
            if (wgpuInstance.Equals(default) == false)
            {
                // Release adapter
                Wgpu.AdapterRelease(wgpuAdapter);
                wgpuAdapter = default;

                // Release instance
                Wgpu.InstanceRelease(wgpuInstance);
                wgpuInstance = default;
            }
        }

        public async Task<GraphicsDevice> RequestDeviceAsync()
        {
            bool completed = false;
            GraphicsDevice requestedDevice = null;

            // Try to request device
            Wgpu.AdapterRequestDevice(wgpuAdapter, new Wgpu.DeviceDescriptor
            {
                defaultQueue = default,
                requiredLimits = IntPtr.Zero,
                requiredFeatureCount = 0,
                requiredFeatures = IntPtr.Zero,
                label = "Device",
                deviceLostCallback = (reason, message, _) => requestedDevice?.OnDeviceLost(message),
                nextInChain = IntPtr.Zero,
            },
            (status, device, message, _) =>
            {
                // Set completed flag
                completed = true;

                // Check status
                if (status == Wgpu.RequestDeviceStatus.Success)
                {
                    requestedDevice = new GraphicsDevice(wgpuInstance, device, this);
                }
                // Create device
                else
                {
                    Debug.LogErrorF(LogFilter.Graphics, "Failed to create device!: [{0}] - {1}", status, message);
                }
            }, IntPtr.Zero);           


            // Wait for completed
            while (completed == false)
                await Task.Delay(10);

            // Create device
            return requestedDevice;
        }


        public static async Task<GraphicsAdapter> CreateAsync(GraphicsSurface surface, GraphicsBackend backend, GraphicsPowerMode powerMode)
        {
            bool completed = false;
            GraphicsAdapter createdAdapter = null;

            // Request adapter
            Wgpu.InstanceRequestAdapter(surface.wgpuInstance, new Wgpu.RequestAdapterOptions
            {
                compatibleSurface = surface.wgpuSurface,
                powerPreference = (Wgpu.PowerPreference)powerMode,
                forceFallbackAdapter = 0u,
                nextInChain = backend != GraphicsBackend.Default
                    ? new WgpuStructChain()
                        .AddAdapterExtras((Wgpu.BackendType)backend)
                        .GetPointer()
                    : IntPtr.Zero,
            },
            (status, adapter, message, _) =>
            {
                // Set completed flag
                completed = true;

                // Check status
                if (status == Wgpu.RequestAdapterStatus.Success)
                {
                    createdAdapter = new GraphicsAdapter(surface.wgpuInstance, adapter);
                }
                // Create device
                else
                {
                    Debug.LogErrorF(LogFilter.Graphics, "Failed to create adapter!: [{0}] - {1}", status, message);
                }
            }, IntPtr.Zero);

            
            // Wait for completed
            while (completed == false)
                await Task.Delay(10);

            // Get adapter
            return createdAdapter;
        }
    }
}
