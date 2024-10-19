using SDL2;
using SharpityEngine;
using SharpityEngine.Content;
using SharpityEngine.Graphics;

namespace SharpityEngine.Player
{
    internal sealed class SDL2_GamePlatform : GamePlatform
    {
        // Private
        private Version sdlVersion = null;
        private SDL2_GameWindow sdlWindow = null;        

        // Properties
        public override string APIName => "SDL2";

        public override Version APIVersion => sdlVersion;

        // Constructor
        public SDL2_GamePlatform(ContentProvider content)
            : base(content)
        {
            // Attach console logger
            Debug.AddLogger(new Debug.ConsoleLogger());

            // Attach file logger
            Debug.AddLogger(new Debug.FileLogger("Game.log", true));

            // Get version
            SDL.SDL_version v;
            SDL.SDL_GetVersion(out v);

            this.sdlVersion = new Version(v.major, v.minor, v.patch);
        }

        // Methods
        public async void RunAsync(string[] args)
        {
            // Initialize platform
            await InitializeAsync();

            // Start game loop
            while(Game.ShouldExit == false)
            {
                // Update platform and game
                Tick();
            }

            // Shutdown
            Shutdown();
        }

        public override GameWindow CreateWindow(string title, int width, int height, bool fullscreen)
        {
            return (sdlWindow = new SDL2_GameWindow(title, width, height, fullscreen));
        }

        public override Game CreateGame(GameWindow window, GraphicsSurface surface, GraphicsAdapter adapter, GraphicsDevice graphicsDevice)
        {
            return new SDL2_Game(TypeManager, this, window, surface, adapter, graphicsDevice);
        }

        public override void OpenURL(string url)
        {
            SDL.SDL_OpenURL(url);
        }

        public override async Task InitializeAsync()
        {
            // Init sdl
            if (SDL.SDL_Init(SDL.SDL_INIT_TIMER | SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_JOYSTICK | SDL.SDL_INIT_GAMECONTROLLER | SDL.SDL_INIT_EVENTS) != 0)
            {
                throw new Exception(SDL.SDL_GetError());
            }

            // Update base
            await base.InitializeAsync();
        }

        public override void Tick()
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                switch (e.type)
                {
                    // Quit
                    case SDL.SDL_EventType.SDL_QUIT:
                        {
                            Game.Exit(0);
                            return;
                        }

                    // Window Event
                    case SDL.SDL_EventType.SDL_WINDOWEVENT:
                        {
                            // Send event to window
                            sdlWindow.PlatformWindowEvent(e.window);
                            break;
                        }
                        

                    // Input Events
                    //case SDL.SDL_EventType.SDL_KEYDOWN:
                    //case SDL.SDL_EventType.SDL_KEYUP:
                    //    {
                    //        input?.SDLKeyEvent(e.key);
                    //        break;
                    //    }
                    //case SDL.SDL_EventType.SDL_TEXTEDITING:
                    //    {
                    //        input?.SDLTextEditEvent(e.edit);
                    //        break;
                    //    }
                    //case SDL.SDL_EventType.SDL_TEXTINPUT:
                    //    {
                    //        input?.SDLTextInputEvent(e.text);
                    //        break;
                    //    }
                    //case SDL.SDL_EventType.SDL_KEYMAPCHANGED:
                    //case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    //case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    //    {
                    //        input?.SDLMouseButtonEvent(e.button);
                    //        break;
                    //    }
                    //case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    //    {
                    //        input?.SDLMouseMotionEvent(e.motion);
                    //        break;
                    //    }

                    //case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    //    {
                    //        input?.SDLMouseWheelEvent(e.wheel);
                    //        break;
                    //    }
                    //case SDL.SDL_EventType.SDL_JOYAXISMOTION:
                    //case SDL.SDL_EventType.SDL_JOYBALLMOTION:
                    //case SDL.SDL_EventType.SDL_JOYHATMOTION:
                    //case SDL.SDL_EventType.SDL_JOYBUTTONDOWN:
                    //case SDL.SDL_EventType.SDL_JOYBUTTONUP:
                    //case SDL.SDL_EventType.SDL_JOYDEVICEADDED:
                    //case SDL.SDL_EventType.SDL_JOYDEVICEREMOVED:
                    //case SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION:
                    //    {
                    //        input?.SDLControllerAxisEvent(e.caxis);
                    //        break;
                    //    }
                    //case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                    //case SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP:
                    //    {
                    //        input?.SDLControllerButtonEvent(e.cbutton);
                    //        break;
                    //    }

                    //case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                    //case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
                    //    {
                    //        input?.SDL_ControllerDeviceEvent(e.cdevice);
                    //        break;
                    //    }
                    case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMAPPED:
                        {
                            //input.ProcessEvent(e);
                            break;
                        }
                }


                var error = SDL.SDL_GetError();
                if (!string.IsNullOrEmpty(error))
                    Console.WriteLine(e.type + ": " + error);
                SDL.SDL_ClearError();
            }

            // Update game
            base.Tick();
        }

        public override void Shutdown()
        {
            // Shutdown game
            base.Shutdown();

            // Cleanup sdl
            SDL.SDL_Quit();
        }
    }
}
