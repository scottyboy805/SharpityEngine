
namespace SharpityEngine.Input
{
    public enum ControllerButton
    {
        None = -1,
        North = 0,
        South = 1,
        East = 2,
        West = 3,
        Back = 4,
        Guide = 5,
        Start = 6,
        LeftStick = 7,
        RightStick = 8,
        LeftShoulder = 9,
        RightShoulder = 10,
        DPadUp = 11,
        DPadDown = 12,
        DPadLeft = 13,
        DPadRight = 14,
    }

    public enum ControllerAxis
    {
        LeftStickX,
        LeftStickY,
        RightStickX,
        RightStickY,
        LeftTrigger,
        RightTrigger,
    }

    public enum ControllerAxisCombined
    {
        LeftStick,
        RightStick,
        Trigger,
    }

    public abstract class Controller
    {
        // Private
        private static Dictionary<string, ControllerButton> nameLookup = null;

        // Protected
        protected internal const int maxControllerButtons = 32;
        protected internal const int maxControllerAxis = 255;

        protected internal bool[] previousButtonStates = new bool[maxControllerButtons];
        protected internal bool[] buttonStates = new bool[maxControllerButtons];
        protected internal int[] previousAxisStates = new int[maxControllerAxis];
        protected internal int[] axisStates = new int[maxControllerAxis];

        // Public
        public const float AxisRange = 32768.0f;

        // Properties
        public abstract string Name { get; }

        public abstract int ID { get; }

        public abstract bool IsDisposed { get; }

        // Constructor
        static Controller()
        {
            // Create lookup
            nameLookup = new Dictionary<string, ControllerButton>();

            // Setup button name lookup
            foreach (ControllerButton button in Enum.GetValues(typeof(ControllerButton)))
            {
                nameLookup[button.ToString()] = button;
            }
        }

        // Methods
        public override string ToString()
        {
            return string.Format("{0}({1})", typeof(Controller).FullName, Name);
        }

        public abstract void Rumble(int lowFrequency, int highFrequency, int milliseconds);

        protected internal virtual void OnDestroy() { }

        public bool IsButtonDown(ControllerButton button)
        {
            // Check for disposed
            CheckDisposed();

            return buttonStates[(int)button] == true && previousButtonStates[(int)button] == false;
        }

        public bool IsButtonDown(string buttonName)
        {
            // Check for disposed
            CheckDisposed();

            return IsButtonDown(nameLookup[buttonName]);
        }

        public bool IsButtonUp(ControllerButton button)
        {
            // Check for disposed
            CheckDisposed();

            return buttonStates[(int)button] == false && previousButtonStates[(int)button] == true;
        }

        public bool IsButtonUp(string buttonName)
        {
            // Check for disposed
            CheckDisposed();

            return IsButtonUp(nameLookup[buttonName]);
        }

        public bool IsButtonHeld(ControllerButton button)
        {
            // Check for disposed
            CheckDisposed();

            return buttonStates[(int)button];
        }

        public bool IsButtonHeld(string buttonName)
        {
            // Check for disposed
            CheckDisposed();

            return IsButtonHeld(nameLookup[buttonName]);
        }

        public float GetAxis(ControllerAxis axis)
        {
            // Check for disposed
            CheckDisposed();

            int axisValue = axisStates[(int)axis];

            return -1 + (axisValue - -AxisRange) * (1 - -1) / (AxisRange - -AxisRange);
        }

        public Vector2 GetAxisCombined(ControllerAxisCombined axis)
        {
            // Check for disposed
            CheckDisposed();

            switch (axis)
            {
                case ControllerAxisCombined.LeftStick:
                    {
                        return new Vector2(
                            GetAxis(ControllerAxis.LeftStickX),
                            GetAxis(ControllerAxis.LeftStickY));
                    }

                case ControllerAxisCombined.RightStick:
                    {
                        return new Vector2(
                            GetAxis(ControllerAxis.RightStickX),
                            GetAxis(ControllerAxis.RightStickY));
                    }

                case ControllerAxisCombined.Trigger:
                    {
                        return new Vector2(
                            GetAxis(ControllerAxis.LeftTrigger),
                            GetAxis(ControllerAxis.RightTrigger));
                    }
            }
            return Vector2.Zero;
        }

        protected void CheckDisposed()
        {
            if (IsDisposed == true)
                throw new ObjectDisposedException("Controller has been disconnected");
        }
    }
}
