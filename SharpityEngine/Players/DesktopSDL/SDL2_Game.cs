﻿using SharpityEngine;
using SharpityEngine.Content;
using SharpityEngine.Graphics;
using SharpityEngine.Input;

namespace SharpityEngine.Player
{
    internal sealed class SDL2_Game : Game
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
        public SDL2_Game(TypeManager typeManager, GamePlatform platform, GameWindow window, GraphicsSurface graphicsSurface, GraphicsAdapter graphicsAdapter, GraphicsDevice graphicsDevice, InputProvider input)
            : base(typeManager, platform, window, graphicsSurface, graphicsAdapter, graphicsDevice, input)
        {
            isRunning = true;

            GameModules.AddModule(new TestRendering(this));
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
