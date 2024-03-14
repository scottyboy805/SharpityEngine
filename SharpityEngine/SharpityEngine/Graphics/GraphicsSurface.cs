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

    public enum SurfaceTextureStatus : int
    {
        Success = 0x00000000,
        Timeout = 0x00000001,
        Outdated = 0x00000002,
        Lost = 0x00000003,
        OutOfMemory = 0x00000004,
        DeviceLost = 0x00000005,
    }

    public sealed class GraphicsSurface : IDisposable
    {
        // Internal
        internal Wgpu.InstanceImpl wgpuInstance;
        internal Wgpu.SurfaceImpl wgpuSurface;

        // Private
        private IGraphicsContext context = null;

        // Properties
        public int RenderWidth
        {
            get { return context?.RenderWidth ?? 0; }
        }

        public int RenderHeight
        {
            get { return context?.RenderHeight ?? 0; }
        }

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
            if(wgpuSurface.Handle != IntPtr.Zero)
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

        public Texture GetCurrentTexture()
        {
            return GetCurrentTexture(out _, out _);
        }

        public Texture GetCurrentTexture(out SurfaceTextureStatus status, out int suboptimal)
        {
            // Get surface
            Wgpu.SurfaceTexture wgpuSurfaceTexture = default;
            Wgpu.SurfaceGetCurrentTexture(wgpuSurface, ref wgpuSurfaceTexture);

            // Assign out
            status = (SurfaceTextureStatus)wgpuSurfaceTexture.status;
            suboptimal = (int)wgpuSurfaceTexture.suboptimal;

            // Check for error
            if (wgpuSurfaceTexture.status != Wgpu.SurfaceGetCurrentTextureStatus.Success)
                return null;

            // Create texture desc
            Wgpu.TextureDescriptor wgpuTextureDesc = new Wgpu.TextureDescriptor
            {
                dimension = Wgpu.TextureGetDimension(wgpuSurfaceTexture.texture),
                format = Wgpu.TextureGetFormat(wgpuSurfaceTexture.texture),
                size = new Wgpu.Extent3D
                {
                    width = Wgpu.TextureGetWidth(wgpuSurfaceTexture.texture),
                    height = Wgpu.TextureGetHeight(wgpuSurfaceTexture.texture),
                    depthOrArrayLayers = Wgpu.TextureGetDepthOrArrayLayers(wgpuSurfaceTexture.texture),
                },
                usage = Wgpu.TextureGetUsage(wgpuSurfaceTexture.texture),
                sampleCount = Wgpu.TextureGetSampleCount(wgpuSurfaceTexture.texture),
                mipLevelCount = Wgpu.TextureGetMipLevelCount(wgpuSurfaceTexture.texture),
            };

            // Check for error
            if (status != SurfaceTextureStatus.Success)
                return null;

            // Create texture
            return new Texture(wgpuSurfaceTexture.texture, wgpuTextureDesc);
        }

        public TextureView GetCurrentTextureView()
        {
            return GetCurrentTextureView(out _, out _);
        }

        public TextureView GetCurrentTextureView(out SurfaceTextureStatus status, out int suboptimal)
        {
            // Get current texture
            Texture texture = GetCurrentTexture(out status, out suboptimal);

            // Check for error
            if (texture == null)
                return null;

            // Create view
            return texture.CreateView();
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

        public static GraphicsSurface CreateSurface(IGraphicsContext window)
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
