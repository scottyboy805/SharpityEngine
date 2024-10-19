using SDL2;
using SharpityEngine;
using SharpityEngine.Graphics.Context;
using System.Diagnostics;

namespace SharpityEngine.Player
{
    internal sealed class SDL2_GameWindow : GameWindow, IGraphicsContext_WindowsNative
    {
        // Internal
        internal IntPtr windowPtr;
        internal uint windowId;

        internal bool useOpenGL = true;

        // Private
        private bool closed = false;

        // Properties
        public override IntPtr Handle
        {
            get
            {
                var info = new SDL.SDL_SysWMinfo();
                SDL.SDL_VERSION(out info.version);
                SDL.SDL_GetWindowWMInfo(windowPtr, ref info);

                switch (info.subsystem)
                {
                    case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINDOWS:
                        return info.info.win.window;
                    case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_X11:
                        return info.info.x11.window;
                    case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_DIRECTFB:
                        return info.info.dfb.window;
                    case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_COCOA:
                        return info.info.cocoa.window;
                    case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_UIKIT:
                        return info.info.uikit.window;
                    case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WAYLAND:
                        return info.info.wl.surface;
                    case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_MIR:
                        return info.info.mir.surface;
                    case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_WINRT:
                        return info.info.winrt.window;
                    case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_ANDROID:
                        return info.info.android.window;
                    case SDL.SDL_SYSWM_TYPE.SDL_SYSWM_UNKNOWN:
                        break;
                }

                throw new NotImplementedException();
            }
        }

        public override bool Focused
        {
            get
            {
                // Get the window flags
                uint flags = SDL.SDL_GetWindowFlags(windowPtr);

                // Check for flag set
                return (flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_INPUT_FOCUS) != 0;
            }
        }

        public override int RenderWidth
        {
            get { return Size.X; }
        }

        public override int RenderHeight
        {
            get { return Size.Y; }
        }

        // Constructor
        public SDL2_GameWindow(string title, int width, int height, bool fullscreen)
            : base(title, width, height, fullscreen)
        {
            // Get default flags
            SDL.SDL_WindowFlags windowFlags = SDL.SDL_WindowFlags.SDL_WINDOW_ALLOW_HIGHDPI;

            // Check for fullscreen
            if (fullscreen == true)
                windowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP;

            //// Check for Open Gl context required
            //if (useOpenGL == true)
            //{
            //    windowFlags |= SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL;

            //    // Setup gl
            //    SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, 3);
            //    SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, 3);
            //    SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL.SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE);
            //    SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_CONTEXT_FLAGS, (int)SDL.SDL_GLcontext.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG);
            //    SDL.SDL_GL_SetAttribute(SDL.SDL_GLattr.SDL_GL_DOUBLEBUFFER, 1);
            //}
                        
            // Create the window
            this.windowPtr = SDL.SDL_CreateWindow(title, 0x2FFF0000, 0x2FFF0000, width, height, windowFlags);

            // Check for error
            if (windowPtr == IntPtr.Zero)
                throw new Exception("Failed to create window: " + SDL.SDL_GetError());

            // Get window id
            this.windowId = SDL.SDL_GetWindowID(windowPtr);

            // Window position
            Point2 position = default;
            SDL.SDL_GetWindowPosition(windowPtr, out position.X, out position.Y);

            // Trigger callback
            OnRepositionedCallbackEvent(position);
        }

        // Methods
        internal void PlatformWindowEvent(SDL.SDL_WindowEvent e)
        {
            switch (e.windowEvent)
            {
                // Call resized
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                //case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                    {
                        // Get new size
                        Point2 point = default;
                        SDL.SDL_GetWindowSize(windowPtr, out point.X, out point.Y);

                        // Trigger callback
                        OnResizedCallbackEvent(point);
                        break;
                    }

                // Call closing
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                    {
                        // Trigger callback
                        OnClosingCallbackEvent();
                        break;
                    }

                // Call restored
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                    {
                        // Trigger callback
                        OnRestoredCallbackEvent();
                        break;
                    }

                // Call minimized
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                    {
                        // Trigger callback
                        OnMinimizedCallbackEvent();
                        break;
                    }

                // Call onFocus
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_TAKE_FOCUS:
                case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                    {
                        // Trigger callback
                        OnFocusedCallbackEvent();
                        break;
                    }
            };
        }

        //public override bool SetDisplayMode(DisplayResolution resolution)
        //{
        //    // Convert to mode
        //    SDL.SDL_DisplayMode specifiedMode = new SDL.SDL_DisplayMode
        //    {
        //        w = resolution.Width,
        //        h = resolution.Height,
        //        refresh_rate = resolution.RefreshRate,
        //    };

        //    // Get closest match
        //    //SDL.SDL_DisplayMode closestMode;
        //    //SDL.SDL_GetClosestDisplayMode()

        //    // Try to set display mode
        //    return SDL.SDL_SetWindowDisplayMode(windowPtr, ref specifiedMode) == 1;
        //}

        public override void Close()
        {
            if (closed == false)
            {
                // Destroy window
                SDL.SDL_DestroyWindow(windowPtr);

                // Set closed flag
                closed = true;
            }
        }

        public override void Focus()
        {
            SDL.SDL_RaiseWindow(windowPtr);
        }

        protected override void OnSetBordered(bool on)
        {
            SDL.SDL_SetWindowBordered(windowPtr, on == true 
                ? SDL.SDL_bool.SDL_TRUE 
                : SDL.SDL_bool.SDL_FALSE);
        }

        protected override void OnSetFullscreen(bool on)
        {
            if (on == true)
            {
                SDL.SDL_SetWindowFullscreen(windowPtr, (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP);
            }
            else
            {
                SDL.SDL_SetWindowFullscreen(windowPtr, (uint)0);
            }
        }

        protected override void OnSetPosition(in Point2 position)
        {
            SDL.SDL_SetWindowPosition(windowPtr, position.X, position.Y);
        }

        protected override void OnSetSize(in Point2 size)
        {
            SDL.SDL_SetWindowSize(windowPtr, size.X, size.Y);
        }

        protected override void OnSetResizable(bool on)
        {
            SDL.SDL_SetWindowResizable(windowPtr, on == true 
                ? SDL.SDL_bool.SDL_TRUE 
                : SDL.SDL_bool.SDL_FALSE);
        }

        protected override void OnSetTitle(string title)
        {
            SDL.SDL_SetWindowTitle(windowPtr, title);
        }


        #region IGraphicsContext_NativeWindows
        void IGraphicsContext_WindowsNative.GetWindowNative(out nint hinstance, out nint hwnd)
        {
            // Get wm version
            SDL.SDL_SysWMinfo wmInfo = default;
            SDL.SDL_GetWindowWMInfo(windowPtr, ref wmInfo);

            // Get hinstance
            hinstance = wmInfo.info.win.hinstance;

            // Get HWND
            hwnd = wmInfo.info.win.window;
        }
        #endregion
    }
}
