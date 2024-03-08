using System.Runtime.CompilerServices;
using WGPU.NET;

namespace SharpityEngine.Graphics.Pipeline
{
    public struct QuerySet : IDisposable
    {
        // Internal
        internal Wgpu.QuerySetImpl wgpuQuerySet;

        // Constructor
        internal QuerySet(Wgpu.QuerySetImpl wgpuQuerySet)
        {
            this.wgpuQuerySet = wgpuQuerySet;
        }

        // Methods
        public void Dispose()
        {
            if(wgpuQuerySet.Handle != IntPtr.Zero)
            {
                // Release query
                Wgpu.QuerySetDestroy(wgpuQuerySet);
                Wgpu.QuerySetRelease(wgpuQuerySet);
                wgpuQuerySet = default;
            }
        }
    }

    public sealed class CommandList : IDisposable
    {
        // Internal
        internal Wgpu.CommandEncoderImpl wgpuEncoder;

        // Constructor
        internal CommandList(Wgpu.CommandEncoderImpl wgpuEncoder)
        {
            this.wgpuEncoder = wgpuEncoder;
        }

        // Methods
        public void Dispose()
        {
            if(wgpuEncoder.Handle != IntPtr.Zero)
            {
                // Release encoder
                Wgpu.CommandEncoderRelease(wgpuEncoder);
                wgpuEncoder = default;
            }
        }

        public unsafe RenderCommandList BeginRenderPass(ColorAttachment[] colorAttachments)
        {
            // Create attachments
            Span<Wgpu.RenderPassColorAttachment> wgpuColors = stackalloc Wgpu.RenderPassColorAttachment[colorAttachments.Length];

            // Create color entries
            for (int i = 0; i < colorAttachments.Length; i++)
            {
                wgpuColors[i] = new Wgpu.RenderPassColorAttachment
                {
                    view = colorAttachments[i].View.wgpuTextureView,
                    resolveTarget = colorAttachments[i].ResolveTarget?.wgpuTextureView ?? default,
                    loadOp = (Wgpu.LoadOp)colorAttachments[i].LoadOp,
                    storeOp = (Wgpu.StoreOp)colorAttachments[i].StoreOp,
                    clearValue = new Wgpu.Color
                    {
                        r = colorAttachments[i].ClearColor.R,
                        g = colorAttachments[i].ClearColor.G,
                        b = colorAttachments[i].ClearColor.B,
                        a = colorAttachments[i].ClearColor.A,
                    },
                };
            }

            // Create desc
            Wgpu.RenderPassDescriptor wgpuRenderPassDesc = new Wgpu.RenderPassDescriptor
            {
                label = "Render Pass",
                colorAttachments = new IntPtr(Unsafe.AsPointer(ref wgpuColors.GetPinnableReference())),
                colorAttachmentCount = (uint)colorAttachments.Length,
                depthStencilAttachment = IntPtr.Zero,
            };

            // Start render pass
            Wgpu.RenderPassEncoderImpl wgpuRenderEncoder = Wgpu.CommandEncoderBeginRenderPass(wgpuEncoder, wgpuRenderPassDesc);

            // Check for error
            if (wgpuRenderEncoder.Handle == IntPtr.Zero)
                return null;

            // Create render command list
            return new RenderCommandList(wgpuRenderEncoder);
        }

        public unsafe RenderCommandList BeginRenderPass(ColorAttachment[] colorAttachments, in DepthStencilAttachment depthStencilAttachment)
        {
            // Create attachments
            Span<Wgpu.RenderPassColorAttachment> wgpuColors = stackalloc Wgpu.RenderPassColorAttachment[colorAttachments.Length];

            // Create color entries
            for (int i = 0; i < colorAttachments.Length; i++)
            {
                wgpuColors[i] = new Wgpu.RenderPassColorAttachment
                {
                    view = colorAttachments[i].View.wgpuTextureView,
                    resolveTarget = colorAttachments[i].ResolveTarget?.wgpuTextureView ?? default,
                    loadOp = (Wgpu.LoadOp)colorAttachments[i].LoadOp,
                    storeOp = (Wgpu.StoreOp)colorAttachments[i].StoreOp,
                    clearValue = new Wgpu.Color
                    {
                        r = colorAttachments[i].ClearColor.R,
                        g = colorAttachments[i].ClearColor.G,
                        b = colorAttachments[i].ClearColor.B,
                        a = colorAttachments[i].ClearColor.A,
                    },
                };
            }

            // Create depth stencil
            Wgpu.RenderPassDepthStencilAttachment wgpuDepthStencil = new Wgpu.RenderPassDepthStencilAttachment
            {
                view = depthStencilAttachment.View.wgpuTextureView,
                depthLoadOp = (Wgpu.LoadOp)depthStencilAttachment.DepthLoadOp,
                depthStoreOp = (Wgpu.StoreOp)depthStencilAttachment.DepthStoreOp,
                depthClearValue = depthStencilAttachment.DepthClearValue,
                depthReadOnly = depthStencilAttachment.DepthReadOnly ? 1u : 0u,
                stencilLoadOp = (Wgpu.LoadOp)depthStencilAttachment.StencilLoadOp,
                stencilStoreOp = (Wgpu.StoreOp)depthStencilAttachment.StencilStoreOp,
                stencilClearValue = depthStencilAttachment.StencilClearValue,
                stencilReadOnly = depthStencilAttachment.StencilReadOnly ? 1u : 0u,
            };

            // Create desc
            Wgpu.RenderPassDescriptor wgpuRenderPassDesc = new Wgpu.RenderPassDescriptor
            {
                label = "Render Pass",
                colorAttachments = new IntPtr(Unsafe.AsPointer(ref wgpuColors.GetPinnableReference())),
                colorAttachmentCount = (uint)colorAttachments.Length,
                depthStencilAttachment = new IntPtr(&wgpuDepthStencil),
            };

            // Start render pass
            Wgpu.RenderPassEncoderImpl wgpuRenderEncoder = Wgpu.CommandEncoderBeginRenderPass(wgpuEncoder, wgpuRenderPassDesc);

            // Check for error
            if (wgpuRenderEncoder.Handle == IntPtr.Zero)
                return null;

            // Create render command list
            return new RenderCommandList(wgpuRenderEncoder);

        }

        public void ClearBuffer(GraphicsBuffer buffer, long offset, long size) 
        {
            Wgpu.CommandEncoderClearBuffer(wgpuEncoder, buffer.wgpuBuffer, (ulong)offset, (ulong)size);
        }

        public void CopyBuffer(GraphicsBuffer source, long sourceOffset, GraphicsBuffer destination, long destinationOffset, long size)
        {
            Wgpu.CommandEncoderCopyBufferToBuffer(wgpuEncoder, source.wgpuBuffer, (ulong)sourceOffset, destination.wgpuBuffer, (ulong)destinationOffset, (ulong)size);
        }

        public CommandBuffer Finish()
        {
            // Finish command
            Wgpu.CommandBufferImpl wgpuCommandBuffer = Wgpu.CommandEncoderFinish(wgpuEncoder, default);

            // Create buffer
            return new CommandBuffer(wgpuCommandBuffer);
        }

        public void ResolveQuerySet(QuerySet querySet, uint firstQuery, uint queryCount, GraphicsBuffer destination, ulong destinationOffset)
        {
            Wgpu.CommandEncoderResolveQuerySet(wgpuEncoder, querySet.wgpuQuerySet, firstQuery, queryCount, destination.wgpuBuffer, destinationOffset);
        }

        public void WriteTimestamp(QuerySet querySet, uint queryIndex)
        {
            Wgpu.CommandEncoderWriteTimestamp(wgpuEncoder, querySet.wgpuQuerySet, queryIndex);
        }

        #region Debug
        public void InsertDebugMarker(string markerLabel)
        { 
            Wgpu.CommandEncoderInsertDebugMarker(wgpuEncoder, markerLabel);
        }

        public void PushDebugGroup(string groupLabel)
        {
            Wgpu.CommandEncoderPushDebugGroup(wgpuEncoder, groupLabel);
        }

        public void PopDebugGroup(string groupLabel)
        {
            Wgpu.CommandEncoderPopDebugGroup(wgpuEncoder);
        }
        #endregion
    }
}
