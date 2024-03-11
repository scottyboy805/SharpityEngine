using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WGPU.NET;

namespace SharpityEngine.Graphics.Pipeline
{
    public sealed class RenderCommandList : IDisposable
    {
        // Internal
        internal Wgpu.RenderPassEncoderImpl wgpuRenderEncoder;

        // Constructor
        internal RenderCommandList(Wgpu.RenderPassEncoderImpl wgpuRenderEncoder)
        {
            this.wgpuRenderEncoder = wgpuRenderEncoder;
        }

        // Methods
        public void Dispose()
        {
            if(wgpuRenderEncoder.Handle != IntPtr.Zero)
            {
                // Release encoder
                Wgpu.RenderPassEncoderRelease(wgpuRenderEncoder);
                wgpuRenderEncoder = default;
            }
        }

        public void Draw(int vertexCount, int instanceCount, int firstVertex, int firstInstance)
        {
            Wgpu.RenderPassEncoderDraw(wgpuRenderEncoder, (uint)vertexCount, (uint)instanceCount, (uint)firstVertex, (uint) firstInstance);
        }

        public void DrawIndexed(int indexCount, int instanceCount, int firstIndex, int baseVertex, int firstInstance)
        {
            Wgpu.RenderPassEncoderDrawIndexed(wgpuRenderEncoder, (uint)indexCount, (uint)instanceCount, (uint)firstIndex, baseVertex, (uint)firstInstance);
        }

        public void DrawIndirect(GraphicsBuffer indirectBuffer, long indirectOffset)
        {
            Wgpu.RenderPassEncoderDrawIndirect(wgpuRenderEncoder, indirectBuffer.wgpuBuffer, (ulong)indirectOffset);
        }

        public void DrawIndexedIndirect(GraphicsBuffer indirectBuffer, long indirectOffset)
        {
            Wgpu.RenderPassEncoderDrawIndexedIndirect(wgpuRenderEncoder, indirectBuffer.wgpuBuffer, (ulong)indirectOffset);
        }

        public void BeginOcclusionQuery(uint queryIndex)
        {
            Wgpu.RenderPassEncoderBeginOcclusionQuery(wgpuRenderEncoder, queryIndex);
        }

        public void BeginPipelineStatisticsQuery(QuerySet querySet, uint queryIndex)
        {
            Wgpu.RenderPassEncoderBeginPipelineStatisticsQuery(wgpuRenderEncoder, querySet.wgpuQuerySet, queryIndex);
        }

        public void End()
        {
            Wgpu.RenderPassEncoderEnd(wgpuRenderEncoder);
        }

        public void EndOcclusionQuery()
        {
            Wgpu.RenderPassEncoderEndOcclusionQuery(wgpuRenderEncoder);
        }

        public void EndPipelineStatisticsQuery()
        {
            Wgpu.RenderPassEncoderEndPipelineStatisticsQuery(wgpuRenderEncoder);
        }

        public unsafe void SetBindGroup(BindGroup group, int groupIndex, int[] dynamicOffsets = null)
        {
            fixed (int* offsetsPtr = dynamicOffsets)
            {
                Wgpu.RenderPassEncoderSetBindGroup(wgpuRenderEncoder, (uint)groupIndex, group.wgpuBindGroup, dynamicOffsets != null ? (uint)dynamicOffsets.Length : 0u, ref Unsafe.AsRef<uint>(offsetsPtr));
            }
        }

        public void SetIndexBuffer(GraphicsBuffer buffer, IndexFormat format, long offset, long size)
        {
            Wgpu.RenderPassEncoderSetIndexBuffer(wgpuRenderEncoder, buffer.wgpuBuffer, (Wgpu.IndexFormat)format, (ulong)offset, (ulong)size);
        }

        public void SetVertexBuffer(GraphicsBuffer buffer, int slot, long offset, long size)
        {
            Wgpu.RenderPassEncoderSetVertexBuffer(wgpuRenderEncoder, (uint)slot, buffer.wgpuBuffer, (ulong)offset, (ulong)size);
        }

        public void SetPipeline(RenderPipeline pipeline)
        {
            Wgpu.RenderPassEncoderSetPipeline(wgpuRenderEncoder, pipeline.wgpuRenderPipeline);
        }

        //public void SetBindGroup(int groupIndex, BindGroup group, int[] dynamicOffsets)
        //{

        //}

        public unsafe void SetPushConstants<T>(ShaderStage stages, int offset, ReadOnlySpan<T> data) where T : unmanaged
        {
            Wgpu.RenderPassEncoderSetPushConstants(wgpuRenderEncoder, (uint)stages, (uint)offset, (uint)(data.Length * sizeof(T)),
                (IntPtr)Unsafe.AsPointer(ref MemoryMarshal.GetReference(data)));
        }

        public void SetBlendConstant(in Color color)
        {
            // Convert color
            Wgpu.Color col = new Wgpu.Color { r = color.R, g = color.G, b = color.B, a = color.A };
            Wgpu.RenderPassEncoderSetBlendConstant(wgpuRenderEncoder, col);
        }

        public void SetScissorRect(in RectInt scissorRect)
        {
            Wgpu.RenderPassEncoderSetScissorRect(wgpuRenderEncoder, (uint)scissorRect.X, (uint)scissorRect.Y, (uint)scissorRect.Width, (uint)scissorRect.Height);
        }

        public void SetViewport(Rect viewRect, float minDepth, float maxDepth)
        {
            Wgpu.RenderPassEncoderSetViewport(wgpuRenderEncoder, viewRect.X, viewRect.Y, viewRect.Width, viewRect.Height, minDepth, maxDepth);
        }

        public void SetStencilReference(int reference)
        {
            Wgpu.RenderPassEncoderSetStencilReference(wgpuRenderEncoder, (uint)reference);
        }

        #region Debug
        public void InsertDebugMarker(string markerLabel)
        {
            Wgpu.RenderPassEncoderInsertDebugMarker(wgpuRenderEncoder, markerLabel);
        }

        public void PushDebugGroup(string groupLabel)
        {
            Wgpu.RenderPassEncoderPushDebugGroup(wgpuRenderEncoder, groupLabel);
        }

        public void PopDebugGroup(string groupLabel)
        {
            Wgpu.RenderPassEncoderPopDebugGroup(wgpuRenderEncoder);
        }
        #endregion
    }
}
