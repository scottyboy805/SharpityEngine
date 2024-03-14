using SharpityEngine.Graphics;
using SharpityEngine.Graphics.Pipeline;
using SharpityEngine.Scene;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public sealed class Camera : Component, IGameEnable
    {
        // Type
        private sealed class CameraSorter : IComparer<Camera>
        {
            // Methods
            public int Compare(Camera x, Camera y)
            {
                return x.renderQueue.CompareTo(y.renderQueue);
            }
        }

        // Events
        [DataMember(Name = "OnWillRender")]
        public readonly GameEvent OnWillRender = new GameEvent();

        // Private
        private static readonly List<Camera> allCameras = new List<Camera>();
        private static readonly List<Camera> allActiveCameras = new List<Camera>();
        private static readonly CameraSorter cameraSorter = new CameraSorter();

        private BatchRenderer batchRenderer = null;
        private Matrix4 viewProjectionMatrix = Matrix4.Identity;
        private Texture depthTexture = null;
        private TextureView depthTextureView = null;
        private TextureView renderTextureView = null;
        private HashSet<IGameDraw> drawCalls = new HashSet<IGameDraw>();

        [DataMember(Name = "RenderTexture")]
        private Texture renderTexture = null;
        [DataMember(Name = "CullingMask")]
        private uint cullingMask = uint.MaxValue;
        [DataMember(Name = "ClearColor")]
        private Color clearColor = Color.CornflowerBlue;
        [DataMember(Name = "RenderQueue")]
        private int renderQueue = 0;
        [DataMember(Name = "Near")]
        private float near = 0.01f;
        [DataMember(Name = "Far")]
        private float far = 1000f;
        [DataMember(Name = "FieldOfView")]
        private float fieldOfView = 60f;
        [DataMember(Name = "Orthographic")]
        private bool orthographic = false;

        // Properties
        public static IReadOnlyList<Camera> AllCameras
        {
            get { return allActiveCameras; }
        }

        public static IReadOnlyList<Camera> AllActiveCameras
        {
            get { return allActiveCameras; }
        }

        public static Camera MainCamera
        {
            get { return allActiveCameras.Count > 0 ? allActiveCameras[0] : null; }
        }

        public Matrix4 ViewProjectionMatrix
        {
            get { return viewProjectionMatrix; }
        }

        public Texture DepthTexture
        {
            get { return depthTexture; }
        }

        public Texture RenderTexture
        {
            get { return renderTexture; }
            set
            {
                renderTexture = value;
                CreateViewProjectionMatrix();
            }
        }

        public uint CullingMask
        {
            get { return cullingMask; }
            set { cullingMask = value; }
        }

        public Color ClearColor
        {
            get { return clearColor; }
            set { clearColor = value; }
        }

        public int RenderQueue
        {
            get { return renderQueue; }
            set { renderQueue = value; }
        }

        public float Near
        {
            get { return near; }
            set 
            { 
                near = value;
                CreateViewProjectionMatrix();
            }
        }

        public float Far
        {
            get { return far; }
            set
            {
                far = value;
                CreateViewProjectionMatrix();
            }
        }

        public float FieldOfView
        {
            get { return fieldOfView; }
            set
            {
                fieldOfView = value;
                CreateViewProjectionMatrix();
            }
        }

        public bool Orthographic
        {
            get { return orthographic; }
            set
            {
                orthographic = value;
                CreateViewProjectionMatrix();
            }
        }

        public int RenderWidth
        {
            get
            {
                // Check for render texture
                if (renderTexture != null)
                    return renderTexture.Width;

                // Check for surface
                if (Game.GraphicsSurface != null)
                    return Game.GraphicsSurface.RenderWidth;

                // Get window width
                return Window.Width;
            }
        }

        public int RenderHeight
        {
            get
            {
                // Check for render texture
                if (renderTexture != null)
                    return renderTexture.Height;

                // Check for surface
                if (Game.GraphicsSurface != null)
                    return Game.GraphicsSurface.RenderHeight;

                // Get window width
                return Window.Height;
            }
        }

        public float AspectRatio
        {
            get { return (float)RenderWidth / RenderHeight; }
        }

        // Constructor
        public Camera()
        {
            // Create batch renderer
            batchRenderer = new BatchRenderer(512,
                Game.Content.Load<Material>("Error.mat"));

            // Add camera
            allCameras.Add(this);

            // Sort by render queue
            allCameras.Sort(cameraSorter);
        }

        // Methods
        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Remove camera
            allCameras.Remove(this);
        }

        void IGameEnable.OnEnable()
        {
            // Add camera
            allActiveCameras.Add(this);

            // Sort by render queue
            allActiveCameras.Sort(cameraSorter);
        }

        void IGameEnable.OnDisable()
        { 
            // Remove camera
            allActiveCameras.Remove(this);
        }

        public void Render(CommandList commandList = null)
        {
            // Get next texture
            TextureView viewTexture = Game.GraphicsSurface.GetCurrentTextureView();

            // Check for view
            if(viewTexture == null)
            {
                Debug.LogError(LogFilter.Graphics, "Could not acquire next swap chain texture");
                return;
            }

            bool ownedCommandList = false;

            // Check for command list
            if(commandList == null)
            {
                // Create command list
                commandList = Game.GraphicsDevice.CreateCommandList();

                // Command list is owned
                ownedCommandList = true;
            }

            // Render pass
            RenderPass(commandList, viewTexture, viewProjectionMatrix);

            // Release swap chain texture
            viewTexture.Dispose();
            viewTexture = null;

            // Submit command
            if(ownedCommandList == true)
            {
                // Finish drawing
                CommandBuffer commandBuffer = commandList.Finish();

                // Submit commands
                Game.GraphicsDevice.Queue.Submit(commandBuffer);

                // Release command list
                commandList.Dispose();
                commandList = null;


                // Present to display
                Game.GraphicsSurface.Present();
            }
        }

        public void Render(TextureView renderTexture, CommandList commandList = null)
        {
            // Check for no render texture
            if(renderTexture == null)
                throw new ArgumentNullException(nameof(renderTexture));

            bool ownedCommandList = false;

            // Check for command list
            if (commandList == null)
            {
                // Create command list
                commandList = Game.GraphicsDevice.CreateCommandList();

                // Command list is owned
                ownedCommandList = true;
            }

            // Render pass
            RenderPass(commandList, renderTexture, viewProjectionMatrix);

            // Submit command
            if (ownedCommandList == true)
            {
                // Finish drawing
                CommandBuffer commandBuffer = commandList.Finish();

                // Submit commands
                Game.GraphicsDevice.Queue.Submit(commandBuffer);

                // Release command list
                commandList.Dispose();
                commandList = null;


                // Present to display
                Game.GraphicsSurface.Present();
            }
        }

        private void RenderPass(CommandList commandList, TextureView renderView, in Matrix4 viewProjection)
        {
            // Create depth texture
            if(depthTexture == null)
            {
                depthTexture = Game.GraphicsDevice.CreateTexture2D(RenderWidth, RenderHeight, TextureFormat.Depth32Float, TextureUsage.RenderAttachment);
                depthTextureView = depthTexture.CreateView();
            }

            // Create render pass
            RenderCommandList renderPass = commandList.BeginRenderPass(
                new ColorAttachment(renderView, clearColor),
                new DepthStencilAttachment(depthTextureView));

            // Start batch
            batchRenderer.Begin(renderPass);
            {
                // Draw early game modules
                Game.GameModules.OnDrawEarly(batchRenderer);


                // Get all scenes
                foreach(GameScene scene in Game.GameModules.EnumerateModulesOfType<GameScene>())
                {
                    // Check for scene visible
                    if (scene.Enabled == false)
                        continue;

                    // Get draw calls
                    HashSet<IGameDraw> sceneDrawCalls = scene.sceneDrawCalls;

                    // Check for any draw calls
                    if (scene.sceneDrawCalls == null || scene.sceneDrawCalls.Count == 0)
                        continue;

                    // Before draw all
                    foreach (IGameDraw drawCall in scene.sceneDrawCalls)
                        drawCall.OnBeforeDraw();

                    // Main draw loop
                    // Draw all
                    foreach(IGameDraw drawCall in sceneDrawCalls)
                    {
                        // Submit the batched draw request
                        drawCall.OnDraw(batchRenderer);
                    }

                    // After draw all
                    foreach (IGameDraw drawCall in scene.sceneDrawCalls)
                        drawCall.OnAfterDraw();
                }


                // Draw late game modules
                Game.GameModules.OnDrawLate(batchRenderer);
            }
            // End batch
            batchRenderer.End();

            // Release render pass
            renderPass.Dispose();
            renderPass = null;

            // Clear draw calls
            drawCalls.Clear();
        }

        private void CreateViewProjectionMatrix()
        {
            // Check for orthographic
            if(orthographic == false)
            {
                // Create perspective
                viewProjectionMatrix = Matrix4.PerspectiveFieldOfView(
                    fieldOfView, AspectRatio, near, far);
            }
            else
            {
                // Create orthographic
                viewProjectionMatrix = Matrix4.Orthographic(
                    -1, 1, near, far);
            }
        }
    }
}
