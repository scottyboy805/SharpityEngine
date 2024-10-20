using SharpityEngine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpityEngine.Input
{
    public enum MouseButton
    {
        None = 0,
        Left = 1,
        Middle = 2,
        Right = 3,
    }

    public abstract class InputProvider : IGameModule
    {
        // Events
        public GameEvent<Key> OnKeyDown = new GameEvent<Key>();
        public GameEvent<Key> OnKeyUp = new GameEvent<Key>();
        public GameEvent<string> OnInputString = new GameEvent<string>();
        public GameEvent<Point2> OnMouseMove = new GameEvent<Point2>();
        public GameEvent<MouseButton> OnMouseDown = new GameEvent<MouseButton>();
        public GameEvent<MouseButton> OnMouseUp = new GameEvent<MouseButton>();
        public GameEvent<Point2> OnMouseScroll = new GameEvent<Point2>();
        public GameEvent<Controller> OnControllerConnected = new GameEvent<Controller>();
        public GameEvent<Controller> OnControllerDisconnected = new GameEvent<Controller>();

        // Private
        private static Dictionary<string, Key> nameLookup = null;

        // Protected
        protected const int MaxKeys = 512;
        protected const int MaxMouse = 8;

        protected bool[] previousKeyStates = new bool[MaxKeys];
        protected bool[] keyStates = new bool[MaxKeys];
        protected bool[] previousMouseStates = new bool[MaxMouse];
        protected bool[] mouseStates = new bool[MaxMouse];

        protected Point2 previousMousePosition = Point2.Zero;
        protected Point2 mousePosition = Point2.Zero;
        protected Point2 mouseDelta = Point2.Zero;
        protected Vector2 mouseScrollDelta = Vector2.Zero;

        protected string inputText = "";
        protected Dictionary<int, Controller> controllers = new Dictionary<int, Controller>();

        // Properties
        public abstract string APIName { get; }

        public abstract Version APIVersion { get; }

        public abstract string ClipboardText { get; set; }

        public virtual Point2 MousePosition
        {
            get { return mousePosition; }
        }

        public virtual Point2 MouseDelta
        {
            get { return mouseDelta; }
        }

        public virtual Vector2 MouseScroll
        {
            get { return mouseScrollDelta; }
        }

        public virtual string InputText
        {
            get { return inputText; }
        }

        public bool IsAnyControllerConnected
        {
            get { return controllers.Count > 0; }
        }

        public int ControllerCount
        {
            get { return controllers.Count; }
        }

        #region IGameModule
        int IGameUpdate.Priority => -100;

        bool IGameUpdate.Enabled => true;

        int IGameDraw.DrawOrder => 0;
        #endregion

        // Constructor
        static InputProvider()
        {
            // Create lookup
            nameLookup = new Dictionary<string, Key>();

            // Setup keyboard name lookup
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                nameLookup[key.ToString()] = key;
            }
        }

        // Methods
        public override string ToString()
        {
            return string.Format("{0}({1}, {2})", typeof(InputProvider).FullName, APIName, APIVersion);
        }


        #region Keyboard
        public virtual bool IsKeyDown(Key key)
        {
            return keyStates[(int)key] == true && previousKeyStates[(int)key] == false;
        }

        public bool IsKeyDown(string keyName)
        {
            return IsKeyDown(nameLookup[keyName]);
        }

        public virtual bool IsKeyUp(Key key)
        {
            return keyStates[(int)key] == false && previousKeyStates[(int)key] == true;
        }

        public bool IsKeyUp(string keyName)
        {
            return IsKeyUp(nameLookup[keyName]);
        }

        public virtual bool IsKeyHeld(Key key)
        {
            return keyStates[(int)key];
        }

        public bool IsKeyHeld(string keyName)
        {
            return IsKeyHeld(nameLookup[keyName]);
        }
        #endregion

        #region Mouse
        public virtual bool IsMouseDown(MouseButton button)
        {
            return mouseStates[(int)button] == true && previousMouseStates[(int)button] == false;
        }

        public virtual bool IsMouseUp(MouseButton button)
        {
            return mouseStates[(int)button] == false && previousMouseStates[(int)button] == true;
        }

        public virtual bool IsMouseHeld(MouseButton button)
        {
            return mouseStates[(int)button];
        }

        #endregion

        #region Controller
        public virtual bool IsControllerConnected(int index)
        {
            return controllers.ContainsKey(index);
        }

        public virtual Controller GetController(int index)
        {
            return controllers[index];
        }
        #endregion

        #region IGameModule
        void IGameModule.OnFrameStart() { }

        void IGameModule.OnFrameEnd()
        {
            UpdateInputState();
        }

        void IGameModule.OnDestroy()
        {
            // Release controllers
            foreach (Controller controller in controllers.Values)
            {
                // Destroy the controller
                controller.OnDestroy();
            }
        }

        void IGameUpdate.OnStart() { }
        void IGameUpdate.OnUpdate(GameTime gameTime) { }
        void IGameDraw.OnBeforeDraw() { }
        void IGameDraw.OnDraw(BatchRenderer batchRenderer) { }
        void IGameDraw.OnAfterDraw() { }
        #endregion

        private void UpdateInputState()
        {
            // Update keyboard
            for (int i = 0; i < MaxKeys; i++)
            {
                previousKeyStates[i] = keyStates[i];
            }

            // Update mouse
            for (int i = 0; i < MaxMouse; i++)
            {
                previousMouseStates[i] = mouseStates[i];
            }

            // Update mouse position
            mouseDelta = mousePosition - previousMousePosition;
            previousMousePosition = mousePosition;

            // Update input text
            inputText = string.Empty;

            // Update controllers
            foreach (Controller controller in controllers.Values)
            {
                // Update buttons
                for (int i = 0; i < Controller.maxControllerButtons; i++)
                {
                    controller.previousButtonStates[i] = controller.buttonStates[i];
                }

                // Update axis
                for (int i = 0; i < Controller.maxControllerAxis; i++)
                {
                    controller.previousAxisStates[i] = controller.axisStates[i];
                }
            }
        }

        #region SystemEvents
        protected void OnKeyDownEvent(Key key)
        {
            keyStates[(int)key] = true;

            // Trigger event
            OnKeyDown.Raise(key);
        }

        protected void OnKeyUpEvent(Key key)
        {
            keyStates[(int)key] = false;

            // Trigger event
            OnKeyUp.Raise(key);
        }

        protected void OnInputStringEvent(string inputString)
        {
            this.inputText = inputString;

            // Trigger event
            OnInputString.Raise(inputString);
        }

        protected void OnMouseDownEvent(MouseButton mouse)
        {
            mouseStates[(int)mouse] = true;

            // Trigger event
            OnMouseDown.Raise(mouse);
        }

        protected void OnMouseUpEvent(MouseButton mouse)
        {
            mouseStates[(int)mouse] = false;

            // Trigger event
            OnMouseUp.Raise(mouse);
        }

        protected void OnMouseMotionEvent(in Point2 mousePosition)
        {
            this.mousePosition = mousePosition;

            // Trigger event
            OnMouseMove.Raise(mousePosition);
        }

        protected void OnMouseScrollEvent(in Point2 scrollDelta)
        {
            if (scrollDelta.X != -1)
                mouseScrollDelta.X = scrollDelta.X;

            if (scrollDelta.Y != -1)
                mouseScrollDelta.Y = scrollDelta.Y;

            // Trigger event
            OnMouseScroll.Raise(scrollDelta);
        }

        internal protected void OnControllerConnectedEvent(int controllerID, Controller controller)
        {
            Debug.LogF(LogFilter.Input, "Controller connected ({0}): {1}", controllerID, controller.Name);
            if (controllers.ContainsKey(controllerID) == false)
            {
                // Register the game pad
                controllers.Add(controllerID, controller);

                // Trigger connected
                OnControllerConnected.Raise(controller);
            }
        }

        protected void OnControllerDisconnectedEvent(int controllerID)
        {
            Debug.Log(LogFilter.Input, "Controller disconnected: " + controllerID);
            if (controllers.ContainsKey(controllerID) == true)
            {
                // Trigger disconnected
                OnControllerDisconnected.Raise(controllers[controllerID]);

                // Destroy controller
                controllers[controllerID].OnDestroy();

                // Unregister the game pad
                controllers.Remove(controllerID);
            }
        }
        #endregion
    }
}
