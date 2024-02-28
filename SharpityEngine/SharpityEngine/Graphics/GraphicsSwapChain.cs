using WGPU.NET;

namespace SharpityEngine.Graphics
{
    public enum PresentMode
    {
        Immediate = 0,
        Mailbox = 1,
        Fifo = 2,
    }

    public sealed class GraphicsSwapChain : IDisposable
    {
        // Private
        private SwapChain swapChain = null;        
        private int width = 0;
        private int height = 0;
        private TextureFormat format = 0;
        private PresentMode presentMode = 0;

        // Properties
        public int Width
        {
            get { return width; }
        }

        public int Height
        {
            get { return height; }
        }

        public TextureFormat Format
        {
            get { return format; }
        }

        public PresentMode PresentMode
        {
            get { return presentMode; }
        }

        // Constructor
        internal GraphicsSwapChain(SwapChain swapChain, int width, int height, TextureFormat format, PresentMode presentMode)
        {
            this.swapChain = swapChain;
            this.width = width;
            this.height = height;
            this.format = format;
            this.presentMode = presentMode;
        }

        // Methods
        public void Dispose()
        {
            if(swapChain != null)
            {
                swapChain.Dispose();
                swapChain = null;
            }
        }

        public void Present()
        {
            swapChain.Present();
        }
    }
}
