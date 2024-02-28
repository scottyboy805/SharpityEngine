
namespace SharpityEngine.Graphics.Context
{
    internal interface IGraphicsContext_WindowsNative : IGraphicsContext
    {
        // Methods
        void GetWindowNative(out IntPtr hinstance, out IntPtr hwnd);
    }
}
