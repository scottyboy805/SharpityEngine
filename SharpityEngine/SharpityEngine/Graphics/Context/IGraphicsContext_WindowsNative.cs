
namespace SharpityEngine.Graphics.Context
{
    internal interface IGraphicsContext_WindowsNative : IGraphicsContext
    {
        // Properties
        IntPtr HInstance { get; }
        IntPtr HWND { get; }
    }
}
