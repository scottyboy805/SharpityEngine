using SharpityEngine.Graphics.Context;
using System.Runtime.InteropServices;
using WGPU.NET;

namespace SharpityEngine.Graphics
{
    public enum CompositeAlphaMode : int
    {
        Auto = 0x00000000,
        Opaque = 0x00000001,
        Premultiplied = 0x00000002,
        Unpremultiplied = 0x00000003,
        Inherit = 0x00000004,
    }

    public enum PresentMode : int
    {
        Fifo = 0x00000000,
        FifoRelaxed = 0x00000001,
        Immediate = 0x00000002,
        Mailbox = 0x00000003,
    }

    public sealed class GraphicsSurface : IDisposable
    {
        // Internal
        internal Wgpu.InstanceImpl wgpuInstance;
        internal Wgpu.SurfaceImpl wgpuSurface;

        // Private
        private IGraphicsContext context = null;

        // Constructor
        private GraphicsSurface(Wgpu.InstanceImpl wgpuInstance, Wgpu.SurfaceImpl wgpuSurface, IGraphicsContext context)
        {
            this.wgpuInstance = wgpuInstance;
            this.wgpuSurface = wgpuSurface;
            this.context = context;
        }

        // Methods
        public void Dispose()
        {
            if(wgpuSurface.Equals(default) == false)
            {
                // Release surface
                Wgpu.SurfaceRelease(wgpuSurface);
                wgpuSurface = default;
            }
        }

        public TextureFormat GetPreferredFormat(GraphicsAdapter adapter)
        {
            return (TextureFormat)Wgpu.SurfaceGetPreferredFormat(wgpuSurface,
                adapter != null ? adapter.wgpuAdapter : default);
        }

        public void Prepare(GraphicsDevice device, PresentMode presentMode, TextureFormat format, TextureUsage usage = TextureUsage.RenderAttachment, CompositeAlphaMode alphaMode = CompositeAlphaMode.Auto, int width = -1, int height = -1)
        {
            // Create config
            Wgpu.SurfaceConfiguration config = new Wgpu.SurfaceConfiguration
            {
                device = device.wgpuDevice,
                presentMode = (Wgpu.PresentMode)presentMode,
                viewFormatCount = 0,
                nextInChain = IntPtr.Zero,
                format = (Wgpu.TextureFormat)format,
                usage = (uint)usage,
                alphaMode = (Wgpu.CompositeAlphaMode)alphaMode,
                width = (uint)(width == -1 ? context.RenderWidth : width),
                height = (uint)(height == -1 ? context.RenderHeight : height),
            };

            // Configure surface
            Wgpu.SurfaceConfigure(wgpuSurface, config);
        }

        public void Present()
        {
            Wgpu.SurfacePresent(wgpuSurface);
        }

        public static GraphicsSurface CreateSurface(GameWindow window)
        {
            Wgpu.InstanceImpl wgpuInstance = Wgpu.CreateInstance(default);
            Wgpu.SurfaceImpl wgpuSurface = default;

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
                    wgpuSurface = Wgpu.InstanceCreateSurface(wgpuInstance, new Wgpu.SurfaceDescriptor
                    {
                        label = "Windows",
                        nextInChain = new WgpuStructChain()
                            .AddSurfaceDescriptorFromWindowsHWND(hinstance, hwnd)
                            .GetPointer(),
                    });
                }
                else
                {
                    Debug.LogError("Could not create surface for native windows platform!");
                    Wgpu.InstanceRelease(wgpuInstance);
                    return null;
                }
            }


            // Create our surface
            if (wgpuSurface.Handle != IntPtr.Zero)
                return new GraphicsSurface(wgpuInstance, wgpuSurface, (IGraphicsContext)window);


            //// Check platform
            //if (window is IGraphicsContext_WindowsNative)
            //{
            //    // Create surface
            //    return instance.CreateSurfaceFromWindowsHWND(
            //        ((IGraphicsContext_WindowsNative)window).HInstance,
            //        ((IGraphicsContext_WindowsNative)window).HWND);
            //}

            Debug.LogError("Could not create surface!");
            Wgpu.InstanceRelease(wgpuInstance);
            return null;
        }
    }
}
