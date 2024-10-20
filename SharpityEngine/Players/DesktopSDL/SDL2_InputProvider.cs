using SDL2;
using SharpityEngine.Input;
using System.Runtime.InteropServices;

namespace SharpityEngine.Player
{
    internal sealed class SDL2_InputProvider : InputProvider
    {
        // Internal
        private Version sdlVersion = null;

        // Properties
        public override string APIName => "SDL2";

        public override Version APIVersion => sdlVersion;

        public override string ClipboardText
        {
            get { return SDL.SDL_GetClipboardText(); }
            set { SDL.SDL_SetClipboardText(value); }
        }

        // Constructor
        public SDL2_InputProvider()
        {
            // Get version
            SDL.SDL_version v;
            SDL.SDL_GetVersion(out v);

            this.sdlVersion = new Version(v.major, v.minor, v.patch);


            // Get joysticks
            int joystickCount = SDL.SDL_NumJoysticks();

            for (int i = 0; i < joystickCount; i++)
            {
                // Check for controllers
                if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_TRUE)
                {
                    // Create game controller
                    Controller controller = new SDL2_Controller(i);

                    // Add to input
                    OnControllerConnectedEvent(i, controller);
                }
            }
        }

        // Methods
        public unsafe void SDLTextInputEvent(in SDL.SDL_TextInputEvent evt)
        {
            // Convert to string
            fixed (byte* arr = evt.text)
            {
                OnInputStringEvent(Marshal.PtrToStringUTF8(new IntPtr(arr)));
            }
        }

        public unsafe void SDLTextEditEvent(in SDL.SDL_TextEditingEvent evt)
        {
            // Convert to string
            fixed (byte* arr = evt.text)
            {
                string modifiedText = Marshal.PtrToStringUTF8(new IntPtr(arr));

            }
        }

        public void SDLKeyEvent(in SDL.SDL_KeyboardEvent evt)
        {
            switch (evt.type)
            {
                case SDL.SDL_EventType.SDL_KEYDOWN:
                    {
                        OnKeyDownEvent((Key)evt.keysym.scancode);
                        break;
                    }

                case SDL.SDL_EventType.SDL_KEYUP:
                    {
                        OnKeyUpEvent((Key)evt.keysym.scancode);
                        break;
                    }
            }
        }

        public void SDLMouseButtonEvent(in SDL.SDL_MouseButtonEvent evt)
        {
            switch (evt.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    {
                        OnMouseDownEvent((MouseButton)evt.button);
                        break;
                    }

                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    {
                        OnMouseUpEvent((MouseButton)evt.button);
                        break;
                    }
            }
        }

        public void SDLMouseMotionEvent(in SDL.SDL_MouseMotionEvent evt)
        {
            OnMouseMotionEvent(new Point2(evt.x, evt.y));
        }

        public void SDLMouseWheelEvent(in SDL.SDL_MouseWheelEvent evt)
        {
            Point2 scrollDelta = new Point2(-1, -1);

            if (evt.x != 0)
                scrollDelta.X = evt.x;

            if (evt.y != 0)
                scrollDelta.Y = evt.y;

            OnMouseScrollEvent(scrollDelta);
        }

        public void SDL_ControllerDeviceEvent(in SDL.SDL_ControllerDeviceEvent evt)
        {
            switch (evt.type)
            {
                case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                    {
                        OnControllerConnectedEvent(evt.which, new SDL2_Controller(evt.which));
                        break;
                    }

                case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
                    {
                        OnControllerDisconnectedEvent(evt.which);
                        break;
                    }
            }
        }

        public void SDLControllerButtonEvent(in SDL.SDL_ControllerButtonEvent evt)
        {
            switch (evt.type)
            {
                case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                    {
                        ((SDL2_Controller)controllers[evt.which]).SDLControllerButtonDown((ControllerButton)evt.button);
                        break;
                    }

                case SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP:
                    {
                        ((SDL2_Controller)controllers[evt.which]).SDLControllerButtonUp((ControllerButton)evt.button);
                        break;
                    }
            }
        }

        public void SDLControllerAxisEvent(in SDL.SDL_ControllerAxisEvent evt)
        {
            if (controllers.ContainsKey(evt.which) == true)
                ((SDL2_Controller)controllers[evt.which]).SDLControllerAxis((ControllerAxis)evt.axis, evt.axisValue);
        }
    }
}
