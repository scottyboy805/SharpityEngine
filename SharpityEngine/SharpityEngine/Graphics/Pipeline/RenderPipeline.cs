using WGPU.NET;

namespace SharpityEngine.Graphics.Pipeline
{
    public sealed class RenderPipeline : IDisposable
    {
        // Internal
        internal Wgpu.RenderPipelineImpl wgpuRenderPipeline;

        // Constructor
        internal RenderPipeline(Wgpu.RenderPipelineImpl wgpuRenderPipeline)
        {
            this.wgpuRenderPipeline = wgpuRenderPipeline;
        }

        // Methods
        public void Dispose()
        {
            if (wgpuRenderPipeline.Handle != IntPtr.Zero)
            {
                // Release pipeline
                Wgpu.RenderPipelineRelease(wgpuRenderPipeline);
                wgpuRenderPipeline = default;
            }
        }
    }
}
