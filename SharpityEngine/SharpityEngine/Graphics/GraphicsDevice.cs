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
