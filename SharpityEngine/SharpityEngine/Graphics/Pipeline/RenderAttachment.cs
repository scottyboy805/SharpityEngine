
using System.Runtime.InteropServices;

namespace SharpityEngine.Graphics.Pipeline
{
    public enum LoadOp : int
    {
        Clear = 0x00000001,
        Load = 0x00000002,
    }

    public enum StoreOp : int
    {
        Store = 0x00000001,
        Discard = 0x00000002,
    }

    public enum StencilOperation : int
    {
        Keep = 0x00000000,
        Zero = 0x00000001,
        Replace = 0x00000002,
        Invert = 0x00000003,
        IncrementClamp = 0x00000004,
        DecrementClamp = 0x00000005,
        IncrementWrap = 0x00000006,
        DecrementWrap = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorAttachment
    {
        // Public
        public TextureView View;
        public TextureView ResolveTarget;
        public LoadOp LoadOp;
        public StoreOp StoreOp;
        public Color ClearColor;

        // Constructor
        public ColorAttachment(TextureView view, Color clearColor, LoadOp loadOp = LoadOp.Clear, StoreOp storeOp = StoreOp.Store)
        {
            this.View = view;
            this.ClearColor = clearColor;
            this.LoadOp = loadOp;
            this.StoreOp = storeOp;
        }

        public ColorAttachment(TextureView view, TextureView resolveTarget, Color clearColor, LoadOp loadOp = LoadOp.Clear, StoreOp storeOp = StoreOp.Store)
        {
            this.View = view;
            this.ResolveTarget = resolveTarget;
            this.ClearColor = clearColor;
            this.LoadOp = loadOp;
            this.StoreOp = storeOp;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DepthStencilAttachment
    {
        // Public
        public TextureView View;
        public LoadOp DepthLoadOp;
        public StoreOp DepthStoreOp;
        public float DepthClearValue;
        public bool DepthReadOnly;
        public LoadOp StencilLoadOp;
        public StoreOp StencilStoreOp;
        public uint StencilClearValue;
        public bool StencilReadOnly;

        // Constructor
        public DepthStencilAttachment(TextureView view)
        {
            this.View = view;
            this.DepthLoadOp = LoadOp.Clear;
            this.DepthStoreOp = StoreOp.Store;
            this.DepthClearValue = 0f;
            this.StencilLoadOp = LoadOp.Clear;
            this.StencilStoreOp = StoreOp.Discard;
        }

        public DepthStencilAttachment(TextureView view, LoadOp depthLoadOp, StoreOp depthStoreOp, float depthClearValue, LoadOp stencilLoadOp, StoreOp stencilStoreOp, uint stencilClearValue)
        {
            this.View = view;
            this.DepthLoadOp = depthLoadOp;
            this.DepthStoreOp = depthStoreOp;
            this.DepthClearValue = depthClearValue;
            this.StencilLoadOp = stencilLoadOp;
            this.StencilStoreOp = stencilStoreOp;
            this.StencilClearValue = stencilClearValue;
        }

        public DepthStencilAttachment(TextureView view, LoadOp depthLoadOp, StoreOp depthStoreOp, bool depthReadOnly, float depthClearValue, LoadOp stencilLoadOp, StoreOp stencilStoreOp, uint stencilClearValue, bool stencilReadOnly)
        {
            this.View = view;
            this.DepthLoadOp = depthLoadOp;
            this.DepthStoreOp = depthStoreOp;
            this.DepthClearValue = depthClearValue;
            this.DepthReadOnly = depthReadOnly;
            this.StencilLoadOp = stencilLoadOp;
            this.StencilStoreOp = stencilStoreOp;
            this.StencilClearValue = stencilClearValue;
            this.StencilReadOnly = stencilReadOnly;
        }
    }
}
