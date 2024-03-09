using SharpityEngine.Graphics;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SharpityEngine-SDL")]

namespace SharpityEngine
{
    internal abstract class GamePlatform
    {
        // Private
        private bool isInitialized = false;
        private TypeManager typeManager = null;
        private Game gameProvider = null;
        
        // Properties
        public abstract string APIName { get; }
        public abstract Version APIVersion { get; }

        public TypeManager TypeManager
        {
            get { return typeManager; }
        }

        public Game GameProvider
        {
            get { return gameProvider; }
        }

        // Constructor
        protected GamePlatform()
        {
            typeManager = new TypeManager();
        }

        // Methods
        public override string ToString()
        {
            return string.Format("{0}({1}, {2})", typeof(GamePlatform).FullName, APIName, APIVersion);
        }

        public virtual async Task InitializeAsync()
        {
            Debug.Log("Startup...");
            Debug.Log("Backend name: " + APIName);
            Debug.Log("Backend version: " + APIVersion.ToString(2));

            // Register this assembly
            typeManager.RegisterAssembly(Assembly.GetExecutingAssembly());

            // Load settings
            Debug.Log(LogFilter.Content, "Load game settings...");
            GameSettings gameSettings = new GameSettings();// await contentProvider.LoadAsync<GameSettings>("GameSettings.json");

            // Check for settings
            if (gameSettings == null)
                throw new Exception("Game settings could not be loaded");

            Debug.Log("Company name: " + gameSettings.CompanyName);
            Debug.Log("Game name: " + gameSettings.GameName);
            Debug.Log("Game version: " + gameSettings.GameVersion.ToString(2));
            Debug.Log("Preferred screen width: " + gameSettings.PreferredScreenWidth);
            Debug.Log("Preferred screen height: " + gameSettings.PreferredScreenHeight);
            Debug.Log("Preferred fullscreen: " + gameSettings.PreferredFullscreen);


            // Preload game assembly modules
#if !SIMPLE2D_DEDICATEDSERVER
            // Preload game assemblies
            Debug.LogF(LogFilter.Content, "Preloading assemblies ({0})...", gameSettings.GameAssemblies.Count);
            foreach (string assemblyPath in gameSettings.GameAssemblies)
            {
                // Load the assembly
                try
                {
                    // Try to load assembly
                    Debug.Log(LogFilter.Content, "Loading assembly module: " + assemblyPath);
                    Assembly gameAssembly = Assembly.LoadFrom(assemblyPath);

                    // Register is loaded
                    if (gameAssembly != null)
                        typeManager.RegisterAssembly(gameAssembly);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
#endif

            //// Preload bundles
            //Debug.LogF(LogFilter.Content, "Preloading bundles ({0})...", gameSettings.PreloadBundles.Count);
            //foreach (string content in gameSettings.PreloadBundles)
            //{
            //    contentProvider.LoadBundle(content);
            //}

            //// Preload content
            //Debug.LogF(LogFilter.Content, "Preloading content ({0})...", gameSettings.PreloadContent.Count);
            //foreach (string content in gameSettings.PreloadContent)
            //{
            //    contentProvider.Load(content);
            //}


            //// Preload bundles
            //string[] bundleFiles = Directory.GetFiles(contentProvider.ContentFolder, "*" + ContentBundle.bundleExtension, SearchOption.AllDirectories);

            //Debug.LogF(LogFilter.Content, "Preloading bundles ({0})...", bundleFiles.Length);
            //foreach (string bundle in bundleFiles)
            //{
            //    contentProvider.LoadBundle(bundle);
            //}


            // Get preferred size
            int screenWidth = 800;// platform.GameCommandLine.screenWidth != -1 ? platform.GameCommandLine.screenWidth : gameSettings.PreferredScreenWidth;
            int screenHeight = 600;// platform.GameCommandLine.screenHeight != -1 ? platform.GameCommandLine.screenHeight : gameSettings.PreferredScreenHeight;

            // Get preferred fullscreen
            bool fullscreen = false;// platform.GameCommandLine.forceFullScreen == true || platform.GameCommandLine.forceWindowed == true
                //? (platform.GameCommandLine.forceWindowed == true ? false : platform.GameCommandLine.forceFullScreen) : gameSettings.PreferredFullscreen;


            // Create game window
            GameWindow window = CreateWindow(gameSettings.GameName, screenWidth, screenHeight, fullscreen);

            Debug.Log(LogFilter.Graphics, "Window width: " + window.Width);
            Debug.Log(LogFilter.Graphics, "Window height: " + window.Height);
            Debug.Log(LogFilter.Graphics, "Window fullscreen: " + window.Fullscreen);


            // Create graphics surface
            GraphicsSurface surface = GraphicsSurface.CreateSurface(window);

            // Create graphics device
            GraphicsAdapter adapter = await GraphicsAdapter.CreateAsync(surface, 0, 0);
            GraphicsDevice device = null;

            // Create device
            if (adapter != null)
                device = await adapter.RequestDeviceAsync();

            // Get preferred format
            TextureFormat swapSurfaceFormat = surface.GetPreferredFormat(adapter);

            // Prepare surface
            surface.Prepare(device, PresentMode.Fifo, swapSurfaceFormat, TextureUsage.RenderAttachment);



            // Create the game provider
            gameProvider = CreateGame(window, surface, device);

            // Initialize the game
            gameProvider.DoGameInitialize();

            // Mark as initialized
            isInitialized = true;
        }

        public virtual void Tick()
        {
            // Check for initialized
            if(isInitialized == false)
            {
#if DEBUG
                throw new InvalidOperationException("Platform must be initialized before update");
#else
                return;
#endif
            }

            // Run a game frame
            gameProvider.DoGameFrame();
        }

        public virtual void Shutdown()
        {
            // Shutdown game
            gameProvider.DoGameShutdown();

            Debug.Log("Shutdown complete! Exit process...");

            // Stop logging
            Debug.Terminate();
        }

        public abstract GameWindow CreateWindow(string title, int width, int height, bool fullscreen);

        public abstract Game CreateGame(GameWindow window, GraphicsSurface surface, GraphicsDevice device);

        public abstract void OpenURL(string url);
    }
}
