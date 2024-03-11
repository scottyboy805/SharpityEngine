using SDL2;
using SharpityEngine.Graphics.Pipeline;
using SharpityEngine.Graphics;
using System.Diagnostics;
using System.Numerics;
using SharpityEngine.Graphics.Context;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SharpityEngine.Content;

namespace SharpityEngine_SDL
{
    internal class TestIsolatedRendering
    {
        struct Vertex
        {
            public Vector3 Position;
            public Vector4 Color;
            public Vector2 UV;

            public Vertex(Vector3 position, Vector4 color, Vector2 uv)
            {
                Position = position;
                Color = color;
                UV = uv;
            }
        }

        struct UniformBuffer
        {
            public Matrix4x4 Transform;
        }

        struct SDLWindow : IGraphicsContext_WindowsNative
        {
            public IntPtr windowPtr;
            public int RenderWidth { get{ int x; SDL.SDL_GetWindowSize(windowPtr, out x, out _); return x;} }
            public int RenderHeight { get{ int y; SDL.SDL_GetWindowSize(windowPtr, out _, out y); return y;} }

            public void GetWindowNative(out nint hinstance, out nint hwnd)
            {
                SDL.SDL_SysWMinfo info = default;
                SDL.SDL_GetWindowWMInfo(windowPtr, ref info);
                hinstance = info.info.win.hinstance;
                hwnd = info.info.win.window;
            }
        }

        public static unsafe void MainTest(string[] args)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_EVENTS);
            IntPtr window = SDL.SDL_CreateWindow("Hello HGPU.Net", SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 800, 600, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            // Create surface
            GraphicsSurface surface = GraphicsSurface.CreateSurface(new SDLWindow { windowPtr = window });

            // Create adapter
            GraphicsAdapter adapter = GraphicsAdapter.CreateAsync(surface, 0, 0).Result;

            // Create device
            GraphicsDevice device = adapter.RequestDeviceAsync().Result;

            Span<Vertex> vertices = new Vertex[]
            {
                new Vertex(new(-1, -1, 0), new(1, 1, 0, 1), new(-.2f, 1.0f)),
                new Vertex(new(1, -1, 0), new(0, 1, 1, 1), new(1.2f, 1.0f)),
                new Vertex(new(0, 1, 0), new(1, 0, 1, 1), new(0.5f, -.5f)),
            };

            // Vertex buffer
            GraphicsBuffer vertexBuffer = device.CreateBuffer<Vertex>(vertices, BufferUsage.Vertex);

            UniformBuffer uniformBufferData = new UniformBuffer
            {
                Transform = Matrix4x4.Identity
            };

            // Uniform buffer
            GraphicsBuffer uniformBuffer = device.CreateBuffer(sizeof(UniformBuffer), BufferUsage.Uniform | BufferUsage.CopyDst);

            //var image = Image.Load<Rgba32>(Path.Combine("Resources", "WGPU-Logo.png"));


            SharpityEngine.Game game = new SDL2_Game(new SharpityEngine.TypeManager(), new SDL2_GamePlatform(), null, surface, device);
            FileContentProvider content = new FileContentProvider(Environment.CurrentDirectory);

            Texture texture = content.Load<Texture>("Resources/WGPU-Logo.png");

            // Texture
            //Texture texture = device.CreateTexture2D(image.Width, image.Height, TextureFormat.RGBA8Unorm, TextureUsage.TextureBinding | TextureUsage.CopyDst);

            //Span<Rgba32> pixels = new Rgba32[image.Width * image.Height];
            //image.CopyPixelDataTo(pixels);

            //// Write texture data
            //device.Queue.WriteTexture<Rgba32>(pixels, texture, new TextureDataLayout(sizeof(Rgba32) * image.Width, image.Height));

            // Sampler
            Sampler sampler = device.CreateSampler(WrapMode.ClampToEdge, FilterMode.Linear);

            // Bind group layout
            BindGroupLayout bindGroupLayout = device.CreateBindGroupLayout(
                BindLayoutData.Buffer(BufferBindingType.Uniform, sizeof(UniformBuffer), 0, ShaderStage.Vertex),
                BindLayoutData.Sampler(SamplerBindingType.Filtering, 1, ShaderStage.Fragment),
                BindLayoutData.Texture(TextureSampleType.Float, TextureViewDimension.Texture2D, 2, ShaderStage.Fragment));

            // Bind group
            BindGroup bindGroup = device.CreateBindGroup(bindGroupLayout,
                BindData.Buffer(uniformBuffer, 0, 0, sizeof(UniformBuffer)),
                BindData.Sampler(sampler, 1),
                BindData.Texture(texture.CreateView(), 2));

            // Shader
            Shader shader = device.CreateShader(File.ReadAllText("shader.wgsl"));

            // Pipeline layout
            RenderPipelineLayout pipelineLayout = device.CreateRenderPipelineLayout(bindGroupLayout);

            // Vertex state
            VertexState vertexState = new VertexState(
                new VertexBufferLayout(sizeof(Vertex),
                    new VertexAttribute(VertexFormat.Float32x3, 0, 0),                                      // Pos
                    new VertexAttribute(VertexFormat.Float32x4, sizeof(Vector3), 1),                        // Col
                    new VertexAttribute(VertexFormat.Float32x2, sizeof(Vector3) + sizeof(Vector4), 2)));    // UV

            TextureFormat swapChainFormat = surface.GetPreferredFormat(adapter);

            // Fragment state
            FragmentState fragmentState = new FragmentState(
                new ColorTargetState(swapChainFormat, ColorWriteMask.All,
                    new BlendState(
                        new BlendComponent(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero),
                        new BlendComponent(BlendOperation.Add, BlendFactor.One, BlendFactor.Zero))));

            // Render pipeline
            RenderPipeline renderPipeline = device.CreateRenderPipeline(pipelineLayout, shader, new RenderPipelineState(
                vertexState, fragmentState, new PrimitiveState(PrimitiveTopology.TriangleStrip, IndexFormat.Undefined),
                new MultisampleState(1), new DepthStencilState(TextureFormat.Depth32Float, CompareFunction.Always,
                    new StencilFaceState(CompareFunction.Always))));


            //glfw.GetWindowSize(window, out int prevWidth, out int prevHeight);
            SDL.SDL_GetWindowSize(window, out int prevWidth, out int prevHeight);

            // Prepare surface
            surface.Prepare(device, PresentMode.Fifo, swapChainFormat);

            // Depth texture
            Texture depthTexture = device.CreateTexture2D(prevWidth, prevHeight, TextureFormat.Depth32Float, TextureUsage.RenderAttachment);
            TextureView depthTextureView = depthTexture.CreateView();


            Span<UniformBuffer> uniformBufferSpan = stackalloc UniformBuffer[1];

            var startTime = DateTime.Now;

            var lastFrameTime = startTime;

            while (true)//!glfw.WindowShouldClose(window))
            {
                SDL.SDL_GetMouseState(out int mouseX, out int mouseY);
                SDL.SDL_GetWindowSize(window, out int width, out int height);
                //glfw.GetCursorPos(window, out double mouseX, out double mouseY);
                //glfw.GetWindowSize(window, out int width, out int height);

                if ((width != prevWidth || height != prevHeight) && width != 0 && height != 0)
                {
                    prevWidth = width;
                    prevHeight = height;
                    //surfaceConfiguration.width = (uint)width;
                    //surfaceConfiguration.height = (uint)height;

                    //depthTextureDescriptor.size.width = (uint)width;
                    //depthTextureDescriptor.size.height = (uint)height;

                    //surface.Configure(device, surfaceConfiguration);

                    //depthTexture.Dispose();
                    //depthTexture = device.CreateTexture(depthTextureDescriptor);
                    //depthTextureView = depthTexture.CreateTextureView();
                }


                var currentTime = DateTime.Now;

                TimeSpan duration = currentTime - startTime;

                Vector2 nrmMouseCoords = new Vector2(
                    (float)(mouseX * 1 - prevWidth * 0.5f) / prevWidth,
                    (float)(mouseY * 1 - prevHeight * 0.5f) / prevHeight
                );

                uniformBufferData.Transform =
                    Matrix4x4.CreateRotationY(
                        MathF.Sign(nrmMouseCoords.X) * (MathF.Log(Math.Abs(nrmMouseCoords.X) + 1)) * 0.9f
                    ) *
                    Matrix4x4.CreateRotationX(
                        MathF.Sign(nrmMouseCoords.Y) * (MathF.Log(Math.Abs(nrmMouseCoords.Y) + 1)) * 0.9f
                    ) *
                    Matrix4x4.CreateScale(
                        (float)(1 + 0.1 * Math.Sin(duration.TotalSeconds * 2.0))
                    ) *
                    Matrix4x4.CreateTranslation(0, 0, -3)
                    ;

                uniformBufferData.Transform *=
                    CreatePerspective(MathF.PI / 4f, (float)prevWidth / prevHeight, 0.01f, 1000);

                var nextTexture = surface.GetCurrentTextureView();

                if (nextTexture == null)
                {
                    Console.WriteLine("Could not acquire next swap chain texture");
                    Console.ReadLine();
                    return;
                }

                CommandList commandList = device.CreateCommandList();
                RenderCommandList renderPass = commandList.BeginRenderPass(new ColorAttachment[]
                {
                    new ColorAttachment(nextTexture, new SharpityEngine.Color(0f, 0.02f, 0.1f, 1f))
                }, new DepthStencilAttachment(depthTextureView));


                renderPass.SetPipeline(renderPipeline);

                renderPass.SetBindGroup(bindGroup, 0);
                renderPass.SetVertexBuffer(vertexBuffer, 0, 0, (vertices.Length * sizeof(Vertex)));
                renderPass.Draw(3, 1, 0, 0);
                renderPass.End();

                nextTexture.Dispose();

                var queue = device.Queue;

                uniformBufferSpan[0] = uniformBufferData;

                queue.WriteBuffer<UniformBuffer>(uniformBuffer, 0, uniformBufferSpan);

                var commandBuffer = commandList.Finish(); //encoder.Finish(null);

                queue.Submit(new CommandBuffer[]
                {
                commandBuffer
                });

                surface.Present();

                //glfw.PollEvents();
                SDL.SDL_PumpEvents();
            }
        }


        //glfw.DestroyWindow(window);
        //glfw.Terminate();

        private static Matrix4x4 CreatePerspective(float fov, float aspectRatio, float near, float far)
        {
            if (fov <= 0.0f || fov >= MathF.PI)
                throw new ArgumentOutOfRangeException(nameof(fov));

            if (near <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(near));

            if (far <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(far));

            float yScale = 1.0f / MathF.Tan(fov * 0.5f);
            float xScale = yScale / aspectRatio;

            Matrix4x4 result;

            result.M11 = xScale;
            result.M12 = result.M13 = result.M14 = 0.0f;

            result.M22 = yScale;
            result.M21 = result.M23 = result.M24 = 0.0f;

            result.M31 = result.M32 = 0.0f;
            var negFarRange = float.IsPositiveInfinity(far) ? -1.0f : far / (near - far);
            result.M33 = negFarRange;
            result.M34 = -1.0f;

            result.M41 = result.M42 = result.M44 = 0.0f;
            result.M43 = near * negFarRange;

            return result;
        }
    }
    
}
