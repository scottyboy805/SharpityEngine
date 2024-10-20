using SharpityEngine.Graphics.Pipeline;
using System;

namespace SharpityEngine.Graphics
{
    public sealed class BatchRenderer
    {
        // Type
        private sealed class BatchDrawCallSorter : IComparer<BatchDrawCall>
        {
            // Methods
            public int Compare(BatchDrawCall x, BatchDrawCall y)
            {
                return x.RenderQueue.CompareTo(y.RenderQueue);
            }
        }

        private struct BatchDrawCall
        {
            // Public            
            public int RenderQueue;
            public RenderPipeline ShaderRenderPipeline;
            public BindGroup MaterialBindGroup;
            public GraphicsBuffer VertexBuffer;
            public GraphicsBuffer IndexBuffer;
            public IndexFormat IndexFormat;
            public int VertexCount;
            public int IndexCount;
        }

        // Private
        private static readonly BatchDrawCallSorter drawSorter = new BatchDrawCallSorter();

        private int maxBatches = 512;
        private Camera camera = null;
        private Material activeMaterial = null;
        private Material errorMaterial = null;
        private List<BatchDrawCall> batchCalls = null;
        private RenderCommandList commandList = null;

        // Properties
        public Material ActiveMaterial
        {
            get { return activeMaterial; }
        }

        // Constructor
        public BatchRenderer(int maxBatches, Camera camera, Material errorMaterial = null)
        {
            this.maxBatches = maxBatches;
            this.camera = camera;
            this.errorMaterial = errorMaterial;
            this.batchCalls = new List<BatchDrawCall>(maxBatches);            
        }

        // Methods
        public void Begin(RenderCommandList commandList)
        {
            // Check for batch already in process
            if (this.commandList != null)
                throw new InvalidOperationException("Begin has already been called");

            this.commandList = commandList;
        }

        public void End()
        {
            // Draw all buffered
            Flush();

            // End command list
            this.commandList.End();
            this.commandList = null;
        }

        public void Flush()
        {
            // Sort all draw calls
            batchCalls.Sort(drawSorter);

            // Store active pipeline
            RenderPipeline activePipeline = null;
            BindGroup activeBindGroup = null;

            // Draw all items
            for(int i = 0; i < batchCalls.Count; i++)
            {
                // Get draw call
                BatchDrawCall drawCall = batchCalls[i];

                // Change pipeline
                if(activePipeline == null || activePipeline != drawCall.ShaderRenderPipeline)
                {
                    commandList.SetPipeline(drawCall.ShaderRenderPipeline);
                    activePipeline = drawCall.ShaderRenderPipeline;
                }

                // Set bind data
                if(activeBindGroup == null || activeBindGroup != drawCall.MaterialBindGroup)
                {
                    commandList.SetBindGroup(drawCall.MaterialBindGroup, 0);
                    activeBindGroup = drawCall.MaterialBindGroup;
                }

                // Set buffers
                commandList.SetVertexBuffer(drawCall.VertexBuffer, 0, 0, drawCall.VertexBuffer.SizeInBytes);

                if(drawCall.IndexBuffer != null)
                    commandList.SetIndexBuffer(drawCall.IndexBuffer, drawCall.IndexFormat, 0, drawCall.IndexBuffer.SizeInBytes);

                // Send draw call
                if(drawCall.IndexBuffer != null)
                {
                    commandList.DrawIndexed(drawCall.IndexCount, 1, 0, 0, 0);
                }
                else
                {
                    commandList.Draw(drawCall.VertexCount, 1, 0, 0);
                }
            }

            // Clear draw calls
            batchCalls.Clear();
        }

        public void DrawBatched(Material material, GraphicsBuffer vertexBuffer, int vertexCount, GraphicsBuffer indexBuffer = null, int indexCount = 0, IndexFormat indexFormat = 0)
        {
            // Check for no material or shader
            if (material == null || material.Shader == null)
            {
#if DEBUG
                material = errorMaterial;

                if (errorMaterial == null || errorMaterial.Shader == null)
                    return;
#else
                return;
#endif
            }

            // Add draw call
            batchCalls.Add(new BatchDrawCall
            {
                RenderQueue = material.Shader.RenderQueue,
                ShaderRenderPipeline = material.Shader.renderPipeline,
                MaterialBindGroup = material.bindGroup,
                VertexBuffer = vertexBuffer,
                VertexCount = vertexCount,
                IndexBuffer = indexBuffer,
                IndexCount = indexCount,
                IndexFormat = indexFormat,
            });

            // Update active material state
            Material last = activeMaterial;
            activeMaterial = material;

            // Auto flush
            if (batchCalls.Count >= maxBatches || (batchCalls.Count > 0 && last != activeMaterial))
                Flush();
        }
    }
}
