using SharpityEngine;
using SharpityEngine.Graphics;
using SharpityEngine.Graphics.Pipeline;

namespace SharpityEngine.Player
{
    internal class TestRendering : IGameModule
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
            public Matrix4 Transform;
        }

        const string shaderSource = "/*\r\n*       VERTEX SHADER\r\n*/\r\n\r\nstruct UniformBuffer {\r\n    mdlMat : mat4x4<f32>\r\n};\r\n\r\nstruct VOut {\r\n    @builtin(position) pos : vec4<f32>,\r\n    @location(1) col : vec4<f32>,\r\n    @location(2) uv : vec2<f32>\r\n};\r\n\r\n@group(0)\r\n@binding(0)\r\nvar<uniform> ub : UniformBuffer;\r\n\r\n@vertex\r\nfn vs_main(@location(0) pos: vec3<f32>, @location(1) col: vec4<f32>, @location(2) uv: vec2<f32>) -> VOut {\r\n    return VOut(ub.mdlMat * vec4<f32>(pos, 1.0), col, uv);\r\n}\r\n\r\n/*\r\n*       FRAGMENT SHADER\r\n*/\r\n\r\n@group(0)\r\n@binding(1)\r\nvar samp : sampler;\r\n\r\n@group(0)\r\n@binding(2)\r\nvar tex : texture_2d<f32>;\r\n\r\n@fragment\r\nfn fs_main(in : VOut) -> @location(0) vec4<f32> {\r\n    let rpos = vec2<f32>(\r\n        floor(in.uv.x * 10.0),\r\n        floor(in.uv.y * 10.0)\r\n    );\r\n\r\n    let texCol = textureSample(tex,samp, in.uv);\r\n\r\n    let col = mix(in.col, vec4<f32>(texCol.rgb, 1.0), texCol.a);\r\n\r\n    return col * mix(1.0, 0.9, f32((rpos.x % 2.0 + 2.0) % 2.0 == (rpos.y % 2.0 + 2.0) % 2.0));\r\n}";

        Game game = null;
        GraphicsBuffer vertexBuffer = null;
        GraphicsBuffer uniformBuffer = null;
        Texture texture = null;
        Sampler sampler = null;
        Shader shader = null;
        BindGroupLayout bindGroupLayout = null;
        BindGroup bindGroup = null;

        Texture depthTexture = null;
        TextureView depthTextureView = null;

        public int Priority => 0;
        public bool Enabled => true;
        public int DrawOrder => 0;
        public bool Visible => true;

        // Constructor
        public TestRendering(Game game)
        {
            this.game = game;
        }

        // Methods
        public unsafe void OnStart()
        {
            Span<Vertex> vertices = new Vertex[]
            {
                new Vertex(new(-1, -1, 0), new(1, 1, 0, 1), new(-.2f, 1.0f)),
                new Vertex(new(1, -1, 0), new(0, 1, 1, 1), new(1.2f, 1.0f)),
                new Vertex(new(0, 1, 0), new(1, 0, 1, 1), new(0.5f, -.5f)),
            };

            // Create buffers
            vertexBuffer = game.GraphicsDevice.CreateBuffer<Vertex>(vertices, BufferUsage.Vertex);
            uniformBuffer = game.GraphicsDevice.CreateBuffer(sizeof(UniformBuffer), BufferUsage.Uniform | BufferUsage.CopyDst);

            // Create texture
            texture = game.GraphicsDevice.CreateTexture2D(512, 512, TextureFormat.RGBA32Float);

            // Create sampler
            sampler = game.GraphicsDevice.CreateSampler(WrapMode.ClampToEdge, FilterMode.Linear);

            // Create shader
            shader = game.GraphicsDevice.CreateShader(shaderSource);

            // Create bind group layout
            //bindGroupLayout = game.GraphicsDevice.CreateBindGroupLayout(
            //    new BufferBindLayoutData(BufferBindingType.Uniform, sizeof(UniformBuffer), 0, ShaderStage.Vertex),
            //    new SamplerBindLayoutData(SamplerBindingType.Filtering, 1, ShaderStage.Fragment),
            //    new TextureBindLayoutData(TextureSampleType.Float, TextureViewDimension.Texture2D, false, 2, ShaderStage.Fragment));

            //// Create bind group
            //bindGroup = game.GraphicsDevice.CreateBindGroup(bindGroupLayout,
            //    new BufferBindData(uniformBuffer, 0, 0, sizeof(UniformBuffer)),
            //    new SamplerBindData(sampler, 1),
            //    new TextureBindData(texture.CreateView(), 2));


            depthTexture = game.GraphicsDevice.CreateTexture2D(game.Window.Width, game.Window.Height, TextureFormat.Depth32Float, TextureUsage.RenderAttachment);
            depthTextureView = depthTexture.CreateView();
        }

        public void OnBeforeDraw()
        {
        }

        public void OnDraw(BatchRenderer batchRenderer)
        {
            //TextureView nextView = game.GraphicsSurface.GetCurrentTextureView();

            //// Check for could not acquire
            //if(nextView == null)
            //{
            //    Debug.LogError("Could not acquire next swap chain texture");
            //    return;
            //}

            //CommandList commandList = game.GraphicsDevice.CreateCommandList();
            //RenderCommandList renderCommandList = commandList.BeginRenderPass(new ColorAttachment[] {
            //    new ColorAttachment(nextView, Color.CornflowerBlue) },
            //    new DepthStencilAttachment(depthTextureView));
        }

        public void OnAfterDraw()
        {
        }

        public void OnDestroy()
        {
        }


        public void OnUpdate(GameTime gameTime) { }
        public void OnFrameEnd() { }

        public void OnFrameStart() { }
    }
}
