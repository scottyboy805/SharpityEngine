using SharpityEngine.Graphics;
using System.Diagnostics;

namespace SharpityEngine
{
    public abstract class Game
    {
        // Private
        private static Game current = null;
        private readonly Thread mainThread = Thread.CurrentThread;

        private TypeManager typeManager = null;
        private GamePlatform platform = null;
        private GameWindow window = null;
        private GraphicsDevice graphicsDevice = null;

        private List<IGameUpdate> scheduledStartElements = new List<IGameUpdate>(256);
        private List<IGameUpdate> scheduledUpdateElements = new List<IGameUpdate>(256);
        private List<GameElement> scheduledDestroyDelayElements = new List<GameElement>();
        private Queue<GameElement> scheduleDestroyElements = new Queue<GameElement>();

        // Game timer and metrics timers
        private GameTime gameTime = new GameTime();
        private Stopwatch timer = null;
        private Stopwatch frameTimer = null;
        private Stopwatch frameUpdateTimer = null;
        private Stopwatch frameRenderTimer = null;
        private TimeSpan updateDuration = default;
        private TimeSpan renderDuration = default;

        // Setup game time and metrics        
        private int targetFrameRate = 60;
        private int frameCounter = 0;
        private float frameTarget = 0f;
        private float frameRate = 0f;

        // Properties
        internal static Game Current
        {
            get { return current; }
        }

        public Thread MainThread
        {
            get { return mainThread; }
        }

        public bool IsMainThread
        {
            get { return mainThread == Thread.CurrentThread; }
        }

        public TypeManager TypeManager
        {
            get { return typeManager; }
        }

        public GameWindow Window
        {
            get { return window; }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
        }

        public abstract bool IsHeadless { get; }

        public abstract bool IsEditor { get; }

        public abstract bool IsPlaying { get; }

        public abstract bool IsExiting { get; }

        internal abstract bool ShouldExit { get; }

        public int TargetFrameRate
        {
            get { return targetFrameRate; }
            set { targetFrameRate = value; }
        }

        // Constructor
        internal Game(TypeManager typeManager, GamePlatform platform, GameWindow window, GraphicsDevice graphicsDevice)
        {
            // Store current instance
            current = this;

            this.typeManager = typeManager;
            this.platform = platform;
            this.window = window;
            this.graphicsDevice = graphicsDevice;
        }

        // Methods
        protected internal virtual void DoGameInitialize()
        {
            // Start running timers
            timer = Stopwatch.StartNew();
            frameTimer = Stopwatch.StartNew();
            frameUpdateTimer = Stopwatch.StartNew();
            frameRenderTimer = Stopwatch.StartNew();
        }

        protected internal virtual void DoGameFrame()
        {
            // Get frame rate target
            frameTarget = (1000f / targetFrameRate);
            frameRate = 1f / (float)frameTimer.Elapsed.TotalSeconds;

            // Frame loop start
            //OnFrameStart();

            //// Frame start
            //gameModules.OnFrameStart();
            //{

            //    // Update the platform
            //    //platform.Tick();


            // Update and draw
            DoGameUpdate();
            DoGameDraw();
            //}
            //// Frame end
            //gameModules.OnFrameEnd();

            //OnFrameEnd();

            // Update destroyed elements
            //DoScheduledDestroyedElements(gameTime);

            // Next frame
            frameCounter++;


            int sleepMilliseconds = 0;

            // Cap frame rate
            if (targetFrameRate > 0)
            {
                float actualFrameTarget = (Window.Focused == true) ? frameTarget : frameTarget * 2;

                // Calculate sleep time
                sleepMilliseconds = ((int)(actualFrameTarget - gameTime.ElapsedSeconds));

                // Wait for target update
                if (sleepMilliseconds > 0)
                    Thread.Sleep(sleepMilliseconds);
            }
        }

        protected internal virtual void DoGameUpdate()
        {
            // Update game time and metrics
            gameTime.Update(timer.Elapsed, frameTimer.Elapsed, frameTarget, frameRate, frameCounter);
            //Metrics.Clear();
            frameTimer.Restart();
            frameUpdateTimer.Restart();

            //// Frame update
            //OnFrameUpdate(gameTime);

            //// Update modules
            //gameModules.OnUpdate(gameTime);


            // Update scene elements and scripts
            DoScheduledUpdateEvents(gameTime);

            // Get update time
            updateDuration = frameUpdateTimer.Elapsed;
        }
        protected internal virtual void DoGameDraw()
        {
            // ### Render loop start
            frameRenderTimer.Restart();
//            OnFrameRenderBegin();

//            // Clear screen
//            canvas.Clear(GraphicsDevice.ClearColor);
//            {
//                // Before render
//                OnFramePostRender(canvas);
//                {
//                    // Before draw
//                    gameModules.OnBeforeDraw();

//                    // Draw
//                    gameModules.OnDraw(canvas);

//                    // Draw stats
//                    if (Metrics.DrawStats == true)
//                        Metrics.DrawStatsText(canvas);

//                    // After draw
//                    gameModules.OnAfterDraw();
//                }
//                // After render
//                OnFramePostRender(canvas);


//                // Display fps and extra info in game window title bar
//#if DEBUG && !SIMPLE2D_WEB
//                // Check for title changed
//                if (window.Title.StartsWith(windowTitle) == false)
//                    windowTitle = window.Title;

//                window.Title = string.Format("{0} - Platform API ({1}, {2}) - Render API ({3}, {4})", windowTitle, platform.ApiName, platform.ApiVersion, graphicsDevice.ApiName, graphicsDevice.ApiVersion);
//#endif
//            }
//            // Update render timings
//            renderDuration = frameRenderTimer.Elapsed;

//            // Render end
//            OnFrameRenderEnd();

            // Swap buffers
            window.SwapBuffers();
        }

        protected internal virtual void DoGameShutdown()
        {
            Debug.Log("Shutdown...");

            //// Destroy game scenes
            //foreach (GameScene gameScene in modules.GetModulesOfType<GameScene>())
            //{
            //    gameScene.Destroy();
            //}

            //// Force all waiting elements to be destroyed
            //DoScheduledDestroyedElements(null);


            //// Shutdown modules
            //gameModules.OnDestroy();


            // Close window
            Debug.Log(LogFilter.Graphics, "Closing window...");
            window.Close();
            window = null;
        }

        internal protected void DoScheduledUpdateEvents(GameTime gameTime)
        {
            // Clear update lists
            scheduledStartElements.Clear();
            scheduledUpdateElements.Clear();

            //// Collect scripts for all active scenes
            //foreach (GameScene scene in modules.EnumerateModulesOfType<GameScene>())
            //{
            //    // Process all scripts in the scene
            //    scene.SubmitUpdateEvents(scheduledStartElements, scheduledUpdateElements);
            //}

            //// Order by priority
            //scheduledStartElements.Sort(GameScene.priorityComparer);
            //scheduledUpdateElements.Sort(GameScene.priorityComparer);

            // Do start events
            foreach (IGameUpdate startReceiver in scheduledStartElements)
            {
                try
                {
                    startReceiver.OnStart();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            // Do update events
            foreach (IGameUpdate updateReceiver in scheduledUpdateElements)
            {
                try
                {
                    updateReceiver.OnUpdate(gameTime);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        internal protected void DoScheduledDestroyedElements(GameTime time)
        {
            //// Update all delayed elements
            //foreach (GameElement element in scheduledDestroyDelayElements)
            //{
            //    if (time != null)
            //    {
            //        // Update time
            //        element.scheduledDestroyTime -= time.ElapsedSeconds;

            //        // Check for time expired
            //        if (element.scheduledDestroyTime <= 0f)
            //            scheduleDestroyElements.Enqueue(element);
            //    }
            //    else
            //    {
            //        scheduleDestroyElements.Enqueue(element);
            //    }
            //}

            //// Update all destroyed elements
            //while (scheduleDestroyElements.Count > 0)
            //{
            //    // Remove element
            //    GameElement destroyed = scheduleDestroyElements.Dequeue();

            //    // Remove from delayed collection
            //    if (scheduledDestroyDelayElements.Contains(destroyed) == true)
            //        scheduledDestroyDelayElements.Remove(destroyed);

            //    // Destroy element
            //    TypeManager.DestroyElementTypeInstance(destroyed);


            //}
        }

        internal protected void ScheduleDestruction(GameElement element)
        {
            // Check for error
            if (element == null || element.IsDestroyed == true)
                return;

            // Push to queue
            scheduleDestroyElements.Enqueue(element);
        }

        internal protected void ScheduleDestruction(GameElement element, float delay)
        {
            // Check for error
            if (element == null || element.IsDestroyed == true)
                return;

            // Set target delay
            element.scheduledDestroyTime = delay;

            // Push to collection
            scheduledDestroyDelayElements.Add(element);
        }

        public void Exit() => Exit(0);
        public abstract void Exit(int code);


    }
}
