
namespace SharpityEngine
{
    public abstract class GameWindow
    {
        // Events        
        public GameEvent<Point2> OnResized = new GameEvent<Point2>();
        public GameEvent OnRestored = new GameEvent();
        public GameEvent OnMinimized = new GameEvent();
        public GameEvent OnFocused = new GameEvent();
        public GameEvent OnClosing = new GameEvent();

        // Private
        private Point2 position = Point2.Zero;
        private Point2 size = Point2.Zero;

        private string title = "";
        private bool bordered = false;
        private bool resizable = false;
        private bool fullscreen = false;
        private bool visible = false;
        private bool vsync = false;

        // Properties
        public abstract IntPtr Handle { get; }

        public int X
        {
            get { return position.X; }
            set { Position = new Point2(value, position.Y); }
        }

        public int Y
        {
            get { return position.Y; }
            set { Position = new Point2(position.X, value); }
        }

        public int Width
        {
            get { return size.X; }
            set { Size = new Point2(value, size.Y); }
        }

        public int Height
        {
            get { return size.Y; }
            set { Size = new Point2(size.X, value); }
        }

        public Point2 Position
        {
            get { return position; }
            set
            {
                if (this.position != value)
                {
                    this.position = value;

                    // Call impl
                    OnSetPosition(position);
                }
            }
        }

        public Point2 Size
        {
            get { return size; }
            set
            {
                if (this.size != value)
                {
                    this.size = value;

                    // Call impl
                    OnSetSize(size);

                    // Trigger event
                    OnResized.Raise(this.size);
                }
            }
        }

        public string Title
        {
            get { return title; }
            set
            {
                if (this.title != value)
                {
                    this.title = value;

                    // Call impl
                    OnSetTitle(title);
                }
            }
        }

        public bool Bordered
        {
            get { return bordered; }
            set
            {
                if (this.bordered != value)
                {
                    this.bordered = value;

                    // Call impl
                    OnSetBordered(bordered);
                }
            }
        }

        public bool Resizable
        {
            get { return resizable; }
            set
            {
                if (this.resizable != value)
                {
                    this.resizable = value;

                    // Call impl
                    OnSetResizable(resizable);
                }
            }
        }

        public bool Fullscreen
        {
            get { return fullscreen; }
            set
            {
                if (this.fullscreen != value)
                {
                    this.fullscreen = value;

                    // Call impl
                    OnSetFullscreen(fullscreen);
                }
            }
        }

        public bool Visible
        {
            get { return visible; }
            set
            {
                if (this.visible != value)
                {
                    this.visible = value;

                    // Call impl
                    OnSetVisible(visible);
                }
            }
        }

        public bool VSync
        {
            get { return vsync; }
            set
            {
                if (this.vsync != value)
                {
                    this.vsync = value;

                    // Call impl
                    OnSetVSync(vsync);
                }
            }
        }

        public abstract bool Focused { get; }

        // Constructor
        protected GameWindow(string title, int width, int height, bool fullscreen)
        {
            this.title = title;
            this.Width = width;
            this.Height = height;
            this.fullscreen = fullscreen;
        }

        // Methods
        protected abstract void OnSetPosition(in Point2 position);

        protected abstract void OnSetSize(in Point2 size);

        protected abstract void OnSetTitle(string title);

        protected abstract void OnSetBordered(bool on);

        protected abstract void OnSetResizable(bool on);

        protected abstract void OnSetFullscreen(bool on);

        protected abstract void OnSetVisible(bool on);

        protected abstract void OnSetVSync(bool on);

        public abstract void Focus();

        public abstract void Close();

        public abstract void SwapBuffers();
    }
}
