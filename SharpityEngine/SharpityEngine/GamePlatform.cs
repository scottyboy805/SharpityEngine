using SharpityEngine.Content;
using SharpityEngine.Graphics;
using SharpityEngine.Input;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Player")]

namespace SharpityEngine
{
    internal abstract class GamePlatform
    {
        // Private
        private bool isInitialized = false;
        private TypeManager typeManager = null;
        private ContentProvider contentProvider = null;
        private Game game = null;
        private string[] args = null;

        // Properties
        public abstract string APIName { get; }
        public abstract Version APIVersion { get; }

        public TypeManager TypeManager
        {
            get { return typeManager; }
        }

        public ContentProvider ContentProvider
        {
            get { return contentProvider; }
        }

        public Game Game
        {
            get { return game; }
        }

        // Constructor
        protected GamePlatform(ContentProvider contentProvider)
        {
            this.typeManager = contentProvider.typeManager;
            this.contentProvider = contentProvider;
        }

        // Methods
        public override string ToString()
        {
            return string.Format("{0}({1}, {2})", typeof(GamePlatform).FullName, APIName, APIVersion);
        }

        public virtual async Task InitializeAsync(string[] args)
        {
            this.args = args;

            Debug.Log("Startup...");
            Debug.Log("Backend name: " + APIName);
            Debug.Log("Backend version: " + APIVersion.ToString(2));

            // Register this assembly
            typeManager.RegisterAssembly(Assembly.GetExecutingAssembly());

            // Load settings
            Debug.Log(LogFilter.Content, "Load game settings...");
            GameSettings gameSettings = await contentProvider.LoadAsync<GameSettings>("GameSettings.json");

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
                    Assembly gameAssembly = Assembly.LoadFrom(contentProvider.ContentFolder + "/"+ assemblyPath);

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

            // Preload bundles
            Debug.LogF(LogFilter.Content, "Preloading bundles ({0})...", gameSettings.PreloadBundles.Count);
            foreach (string content in gameSettings.PreloadBundles)
            {
                contentProvider.LoadBundle(content);
            }

            // Preload content
            Debug.LogF(LogFilter.Content, "Preloading content ({0})...", gameSettings.PreloadContent.Count);
            foreach (string content in gameSettings.PreloadContent)
            {
                contentProvider.Load(content);
            }


            //// Preload bundles
            //string[] bundleFiles = Directory.GetFiles(contentProvider.ContentFolder, "*" + ContentBundle.bundleExtension, SearchOption.AllDirectories);

            //Debug.LogF(LogFilter.Content, "Preloading bundles ({0})...", bundleFiles.Length);
            //foreach (string bundle in bundleFiles)
            //{
            //    contentProvider.LoadBundle(bundle);
            //}


            // Get preferred size
            int screenWidth = GetArgument("width", gameSettings.PreferredScreenWidth);
            int screenHeight = GetArgument("height", gameSettings.PreferredScreenHeight);

            // Get preferred fullscreen
            bool fullscreen = GetArgument("fullscreen", gameSettings.PreferredFullscreen);

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


            // Create the input
            Debug.Log(LogFilter.Input, "Initialize input...");
            InputProvider input = CreateInput();


            // Create the game provider
            game = CreateGame(window, surface, adapter, device, input);

            // Preload scenes
            Debug.LogF(LogFilter.Content, "Loading startup scenes ({0})...", gameSettings.StartupScenes.Count);
            foreach (string scene in gameSettings.StartupScenes)
            {
                contentProvider.LoadScene(scene);
            }

            // Initialize the game
            game.DoGameInitialize();

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
            game.DoGameFrame();
        }

        public virtual void Shutdown()
        {
            // Shutdown game
            game.DoGameShutdown();

            Debug.Log("Shutdown complete! Exit process...");

            // Stop logging
            Debug.Terminate();
        }

        public abstract GameWindow CreateWindow(string title, int width, int height, bool fullscreen);

        public abstract InputProvider CreateInput();

        public abstract Game CreateGame(GameWindow window, GraphicsSurface surface, GraphicsAdapter adapter, GraphicsDevice device, InputProvider input);

        public abstract void OpenURL(string url);

        public bool HasArgument(string name)
        {
            return string.IsNullOrEmpty(GetArgumentParameter(name)) == false;
        }

        public string GetArgument(string name)
        {
            return GetArgumentParameter(name);
        }

        public int GetArgument(string name, int defaultValue)
        {
            // Try to find parameter
            string param = GetArgumentParameter(name);

            // Check for found
            int value;
            if (string.IsNullOrEmpty(param) == false && int.TryParse(param, out value) == true)
                return value;

            return defaultValue;
        }

        public bool GetArgument(string name, bool defaultValue)
        {
            // Try to find parameter
            string param = GetArgumentParameter(name);

            // Check for found
            bool value;
            if (string.IsNullOrEmpty(param) == false && bool.TryParse(param, out value) == true)
                return value;

            return default;
        }

        private string GetArgumentParameter(string name)
        {
            // Try to get argument
            if (args != null && args.Length > 0)
            {
                // Get search name
                string argSearch = "-" + name;

                // Try to find
                foreach (string arg in args)
                {
                    // Check for index
                    if(arg.IndexOf(argSearch) == 0)
                    {
                        // Get trimmed arg
                        string argTrimmed = arg.Remove(0, argSearch.Length);

                        // Check for parameters
                        if(argTrimmed.Length > 0 && argTrimmed[0] == '=')
                        {
                            // Get parameter
                            string argValue = argTrimmed.Remove(0, 1);

                            // Get the result
                            return argValue != null
                                ? argValue : name;
                        }
                    }
                }
            }
            return null;
        }
    }
}
