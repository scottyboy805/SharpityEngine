using WGPU.NET;

namespace SharpityEngine.Graphics.Pipeline
{
    public sealed class RenderPipelineLayout : IDisposable
    {
        // Internal
        internal Wgpu.PipelineLayoutImpl wgpuPipelineLayout;

        // Constructor
        internal RenderPipelineLayout(Wgpu.PipelineLayoutImpl wgpuPipelineLayout)
        {
            this.wgpuPipelineLayout = wgpuPipelineLayout;
        }

        // Methods
        public void Dispose()
        {
            if(wgpuPipelineLayout.Handle != IntPtr.Zero)
            {
                // Release pipeline layout
                Wgpu.PipelineLayoutRelease(wgpuPipelineLayout);
                wgpuPipelineLayout = default;
            }
        }
    }
}
