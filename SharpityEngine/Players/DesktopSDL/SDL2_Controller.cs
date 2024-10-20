using SDL2;
using SharpityEngine.Input;

namespace SharpityEngine.Player
{
    [TypeManagerIgnore]
    internal sealed class SDL2_Controller : Controller
    {
        // Internal
        internal int id = -1;
        internal IntPtr controllerPtr = IntPtr.Zero;
        internal string controllerName = null;

        // Properties
        public override string Name
        {
            get
            {
                CheckDisposed();
                return controllerName;
            }
        }

        public override int ID
        {
            get
            {
                CheckDisposed();
                return id;
            }
        }

        public override bool IsDisposed => controllerPtr == IntPtr.Zero;

        // Constructor
        public SDL2_Controller(int id)
        {
            this.id = id;

            // Open the controller
            this.controllerPtr = SDL.SDL_GameControllerOpen(id);
            this.controllerName = SDL.SDL_GameControllerName(controllerPtr);
        }

        // Methods
        protected internal override void OnDestroy()
        {
            // Close the controller
            SDL.SDL_GameControllerClose(controllerPtr);

            // Dispose controller
            controllerPtr = IntPtr.Zero;
            id = -1;
        }

        public override void Rumble(int lowFrequency, int highFrequency, int milliseconds)
        {
            // Check for disposed
            CheckDisposed();

            // Set rumble effect
            SDL.SDL_GameControllerRumble(controllerPtr, (ushort)lowFrequency, (ushort)highFrequency, (uint)milliseconds);
        }

        internal void SDLControllerButtonDown(ControllerButton button)
        {
            buttonStates[(int)button] = true;
        }

        internal void SDLControllerButtonUp(ControllerButton button)
        {
            buttonStates[(int)button] = false;
        }

        internal void SDLControllerAxis(ControllerAxis axis, int value)
        {
            if (value < (AxisRange / 10) || value > (AxisRange / 10))
            {
                axisStates[(int)axis] = value;
            }
        }
    }
}
