
using SharpityEngine.Graphics;

namespace SharpityEngine
{
    internal sealed class Game : GameProvider
    {
        // Private
        private bool isRunning = false;
        private bool isExiting = false;        

        // Properties
        public override bool IsHeadless => false;

        public override bool IsEditor => false;

        public override bool IsPlaying => true;

        public override bool IsExiting => isExiting;

        internal override bool ShouldExit
        {
            get { return isRunning == false; }
        }

        // Constructor
        public Game(TypeManager typeManager, GamePlatformProvider platform, GameWindow window, GraphicsDevice graphicsDevice)
            : base(typeManager, platform, window, graphicsDevice)
        {
            isRunning = true;
        }

        // Methods
        public override void Exit(int code)
        {
            // Check for already exiting
            if (isExiting == true)
                return;

            // Set exit code
            Environment.ExitCode = code;

            // Set exit flag
            isRunning = false;
            isExiting = true;
        }
    }
}
