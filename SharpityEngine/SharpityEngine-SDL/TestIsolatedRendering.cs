using SDL2;
using SharpityEngine.Graphics.Pipeline;
using SharpityEngine.Graphics;
using System.Diagnostics;
using System.Numerics;
using SharpityEngine.Graphics.Context;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SharpityEngine.Content;
using Vector3 = SharpityEngine.Vector3;

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


            SharpityEngine.Game game = new SDL2_Game(new SharpityEngine.TypeManager(), new SDL2_GamePlatform(), null, surface, adapter, device);
            FileContentProvider content = new FileContentProvider(Environment.CurrentDirectory);


            // Create mesh triangle
            Mesh mesh = new Mesh();
            Mesh.SubMesh subMesh = mesh.CreateSubMesh();
            subMesh.Vertices.Add(new(-1, -1, 0)); subMesh.Vertices.Add(new(1, -1, 0)); subMesh.Vertices.Add(new (0, 1, 0));
            subMesh.Colors.Add(new(1, 1, 0, 1)); subMesh.Colors.Add(new(0, 1, 1, 1)); subMesh.Colors.Add(new(1, 0, 1, 1));
            subMesh.UVs_0.Add(new(-.2f, 1.0f)); subMesh.UVs_0.Add(new(1.2f, 1.0f)); subMesh.UVs_0.Add(new(0.5f, -0.5f));
            mesh.Apply();

            GraphicsBuffer vertexBuffer = subMesh.vertexBuffer;

            UniformBuffer uniformBufferData = new UniformBuffer
            {
                Transform = Matrix4x4.Identity
            };

            // Uniform buffer
            GraphicsBuffer uniformBuffer = device.CreateBuffer(sizeof(UniformBuffer), BufferUsage.Uniform | BufferUsage.CopyDst);

            // Load texture content
            Texture texture = content.Load<Texture>("Resources/WGPU-Logo.png");

            // Load mesh content
            Mesh cubeMesh = content.Load<Mesh>("Resources/Cube.fbx");
            vertexBuffer = cubeMesh.SubMeshes[0].vertexBuffer;

            // Sampler
            Sampler sampler = device.CreateSampler(WrapMode.ClampToEdge, FilterMode.Linear);

            // Shader
            Shader shader = device.CreateShaderSource(File.ReadAllText("shader.wgsl"));

            // Material
            Material material = new Material(shader);

            material.SetBuffer(uniformBuffer, 0);
            material.SetSampler(sampler, 1);
            material.SetTextureView(texture.CreateView(), 2);
            material.Apply();

            TextureFormat swapChainFormat = surface.GetPreferredFormat(adapter);

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
                    Matrix4x4.CreateTranslation(0, 0, -5)
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


                renderPass.SetPipeline(material.Shader.renderPipeline);

                renderPass.SetBindGroup(material.bindGroup, 0);
                renderPass.SetVertexBuffer(vertexBuffer, 0, 0, (cubeMesh.SubMeshes[0].Vertices.Count * sizeof(Vertex)));
                renderPass.Draw(cubeMesh.SubMeshes[0].Vertices.Count, 1, 0, 0);
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
