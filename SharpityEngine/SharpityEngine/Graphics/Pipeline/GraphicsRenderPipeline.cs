using WGPU.NET;

namespace SharpityEngine.Graphics.Pipeline
{
    public sealed class GraphicsRenderPipeline : IDisposable
    {
        // Private
        private RenderPipeline pipeline = null;

        // Constructor
        internal GraphicsRenderPipeline(RenderPipeline pipeline)
        {
            this.pipeline = pipeline;
        }

        // Methods
        public void Dispose()
        {
            if (pipeline != null)
            {
                pipeline.Dispose();
                pipeline = null;
            }
        }
    }
}
