using SharpityEngine.Graphics.Pipeline;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WGPU.NET;

namespace SharpityEngine.Graphics
{
    public enum GraphicsPowerMode
    {
        LowPower = 1,
        HighPower = 2,
    }

    public enum GraphicsBackend
    {
        Default = 0,
        Null = 1,
        WebGPU = 2,
        D3D11 = 3,
        D3D12 = 4,
        Metal = 5,
        Vulkan = 6,
        OpenGL = 7,
        OpenGLES = 8,
    }

    public enum QueryType : int
    {
        Occlusion = 0x00000000,
        Timestamp = 0x00000001,
    }

    public enum PipelineStatisticName : int
    {
        VertexShaderInvocations = 0x00000000,
        ClipperInvocations = 0x00000001,
        ClipperPrimitivesOut = 0x00000002,
        FragmentShaderInvocations = 0x00000003,
        ComputeShaderInvocations = 0x00000004,
    }

    public sealed class GraphicsDevice : IDisposable
    {
        // Internal
        internal Wgpu.InstanceImpl wgpuInstance;
        internal Wgpu.DeviceImpl wgpuDevice;

        // Private
        private GraphicsAdapter adapter = null;
        private GraphicsQueue queue = null;

        // Properties
        public GraphicsAdapter Adapter
        {
            get { return adapter; }
        }

        public GraphicsQueue Queue
        {
            get { return queue; }
        }

        // Constructor
        internal GraphicsDevice(Wgpu.InstanceImpl wgpuInstance, Wgpu.DeviceImpl wgpuDevice, GraphicsAdapter adapter)
        {
            this.wgpuInstance = wgpuInstance;
            this.wgpuDevice = wgpuDevice;
            this.adapter = adapter;

            // Create queue
            this.queue = new GraphicsQueue(Wgpu.DeviceGetQueue(wgpuDevice));

            // Set error callback
            Wgpu.ErrorCallback errorCallback = (type, message, _) => ErrorCallback(type, message);
            Wgpu.DeviceSetUncapturedErrorCallback(wgpuDevice, errorCallback, IntPtr.Zero);
        }

        // Methods
        public void Dispose()
        {
            if (wgpuDevice.Handle != IntPtr.Zero)
            {
                // Release queue
                queue.Dispose();
                queue = null;

                // Release device
                Wgpu.DeviceDestroy(wgpuDevice);
                Wgpu.DeviceRelease(wgpuDevice);
                wgpuDevice = default;
            }
        }

        public GraphicsBuffer CreateBuffer(long size, BufferUsage usage, string label = null)
        {
            // Create desc
            Wgpu.BufferDescriptor desc = new Wgpu.BufferDescriptor
            {
                label = label,
                mappedAtCreation = 0u,
                size = (ulong)size,
                usage = (uint)usage,
            };

            // Create buffer
            Wgpu.BufferImpl buffer = Wgpu.DeviceCreateBuffer(wgpuDevice, desc);

            // Check for error
            if (buffer.Handle == IntPtr.Zero)
                return null;

            // Get result
            return new GraphicsBuffer(wgpuDevice, buffer, desc);
        }

        public GraphicsBuffer CreateBuffer<T>(Span<T> data, BufferUsage usage, string label = null) where T : unmanaged
        {
            return CreateBuffer<T>((ReadOnlySpan<T>)data, usage, label);
        }

        public GraphicsBuffer CreateBuffer<T>(ReadOnlySpan<T> data, BufferUsage usage, string label = null) where T : unmanaged
        {
            // Create desc
            Wgpu.BufferDescriptor desc = new Wgpu.BufferDescriptor
            {
                label = label,
                mappedAtCreation = 1u,
                size = (ulong)data.Length * (ulong)Marshal.SizeOf<T>(),
                usage = (uint)usage,
            };

            // Create buffer
            Wgpu.BufferImpl buffer = Wgpu.DeviceCreateBuffer(wgpuDevice, desc);

            // Check for error
            if (buffer.Handle == IntPtr.Zero)
                return null;

            // Get result
            GraphicsBuffer bufferCreated = new GraphicsBuffer(wgpuDevice, buffer, desc);

            // Copy data
            data.CopyTo(bufferCreated.MapRange<T>(0, data.Length));

            // Unmap
            bufferCreated.Unmap();

            // Get result
            return bufferCreated;
        }

        public Texture CreateTexture1D(int length, TextureFormat format, TextureUsage usage = TextureUsage.TextureBinding | TextureUsage.CopyDst, int mipLevel = 1, int sampleCount = 1)
        {
            // Create desc
            Wgpu.TextureDescriptor wgpuTextureDesc = new Wgpu.TextureDescriptor
            {
                label = null,
                usage = (uint)usage,
                dimension = Wgpu.TextureDimension.OneDimension,
                size = new Wgpu.Extent3D
                {
                    width = (uint)length,
                    depthOrArrayLayers = 1,
                },
                format = (Wgpu.TextureFormat)format,
                mipLevelCount = (uint)mipLevel,
                sampleCount = (uint)sampleCount,
            };

            // Create texture
            Wgpu.TextureImpl wgpuTexture = Wgpu.DeviceCreateTexture(wgpuDevice, wgpuTextureDesc);

            // Check for error
            if (wgpuTexture.Handle == IntPtr.Zero)
                return null;

            // Get result
            return new Texture(wgpuTexture, wgpuTextureDesc);
        }

        public Texture CreateTexture2D(int width, int height, TextureFormat format, TextureUsage usage = TextureUsage.TextureBinding | TextureUsage.CopyDst, int mipLevel = 1, int sampleCount = 1)
        {
            // Create desc
            Wgpu.TextureDescriptor wgpuTextureDesc = new Wgpu.TextureDescriptor
            {
                label = null,
                usage = (uint)usage,
                dimension = Wgpu.TextureDimension.TwoDimensions,
                size = new Wgpu.Extent3D
                {
                    width = (uint)width,
                    height = (uint)height,
                    depthOrArrayLayers = 1,
                },
                format = (Wgpu.TextureFormat)format,
                mipLevelCount = (uint)mipLevel,
                sampleCount = (uint)sampleCount,
            };

            // Create texture
            Wgpu.TextureImpl wgpuTexture = Wgpu.DeviceCreateTexture(wgpuDevice, wgpuTextureDesc);

            // Check for error
            if (wgpuTexture.Handle == IntPtr.Zero)
                return null;

            // Get result
            return new Texture(wgpuTexture, wgpuTextureDesc);
        }

        public Texture CreateTexture3D(int width, int height, int depth, TextureFormat format, TextureUsage usage = TextureUsage.TextureBinding | TextureUsage.CopyDst, int mipLevel = 1, int sampleCount = 1)
        {
            // Create desc
            Wgpu.TextureDescriptor wgpuTextureDesc = new Wgpu.TextureDescriptor
            {
                label = null,
                usage = (uint)usage,
                dimension = Wgpu.TextureDimension.ThreeDimensions,
                size = new Wgpu.Extent3D
                {
                    width = (uint)width,
                    height = (uint)height,
                    depthOrArrayLayers = (uint)depth,
                },
                format = (Wgpu.TextureFormat)format,
                mipLevelCount = (uint)mipLevel,
                sampleCount = (uint)sampleCount,
            };

            // Create texture
            Wgpu.TextureImpl wgpuTexture = Wgpu.DeviceCreateTexture(wgpuDevice, wgpuTextureDesc);

            // Check for error
            if (wgpuTexture.Handle == IntPtr.Zero)
                return null;

            // Get result
            return new Texture(wgpuTexture, wgpuTextureDesc);
        }

        public Sampler CreateSampler(WrapMode wrapMode = WrapMode.ClampToEdge, FilterMode filter = FilterMode.Linear)
        {
            // Call through
            return CreateSampler(wrapMode, wrapMode, wrapMode, filter, filter, filter);
        }

        public Sampler CreateSampler(WrapMode uMode, WrapMode vMode, WrapMode wMode, FilterMode magFilter, FilterMode minFilter, FilterMode mipFilter, float lodMinClamp = 0f, float logMaxClamp = 1f, CompareFunction compareMode = CompareFunction.Default)
        {
            // Create desc
            Wgpu.SamplerDescriptor wgpuSamplerDesc = new Wgpu.SamplerDescriptor
            {
                label = "Sampler",
                addressModeU = (Wgpu.AddressMode)uMode,
                addressModeV = (Wgpu.AddressMode)vMode,
                addressModeW = (Wgpu.AddressMode)wMode,
                magFilter = (Wgpu.FilterMode)magFilter,
                minFilter = (Wgpu.FilterMode)minFilter,
                mipmapFilter = (Wgpu.MipmapFilterMode)mipFilter,
                lodMinClamp = lodMinClamp,
                lodMaxClamp = logMaxClamp,
                compare = (Wgpu.CompareFunction)compareMode,
                maxAnisotropy = 1,
            };

            // Create sampler
            Wgpu.SamplerImpl wgpuSampler = Wgpu.DeviceCreateSampler(wgpuDevice, wgpuSamplerDesc);

            // Check for error
            if (wgpuSampler.Handle == IntPtr.Zero)
                return null;

            // Get result
            return new Sampler(wgpuSampler);
        }

        public Shader CreateShader(string name = null)
        {
            // Create shader from asset
            return new Shader(wgpuDevice, name);
        }

        public Shader CreateShaderSource(string shaderSource, string name = null)
        {
            // Create desc
            Wgpu.ShaderModuleDescriptor wgpuShaderDesc = new Wgpu.ShaderModuleDescriptor
            {
                label = name,
                nextInChain = new WgpuStructChain()
                    .AddShaderModuleWGSLDescriptor(shaderSource)
                    .GetPointer(),
            };

            // Create shader
            Wgpu.ShaderModuleImpl wgpuShader = Wgpu.DeviceCreateShaderModule(wgpuDevice, wgpuShaderDesc);

            // Check for error
            if(wgpuShader.Handle == IntPtr.Zero) 
                return null;

            // Get result
            return new Shader(wgpuDevice, wgpuShader, shaderSource, name);
        }

        public unsafe BindGroupLayout CreateBindGroupLayout(BindLayoutData[] layoutData, string label = null)
        {
            // Create entry
            Span<Wgpu.BindGroupLayoutEntry> wgpuEntries = stackalloc Wgpu.BindGroupLayoutEntry[layoutData.Length];

            // Fill out data
            for (int i = 0; i < layoutData.Length; i++)
                wgpuEntries[i] = layoutData[i].GetLayoutEntry();

            // Create desc
            Wgpu.BindGroupLayoutDescriptor wgpuBindGroupLayoutDesc = new Wgpu.BindGroupLayoutDescriptor
            {
                label = label,
                entries = (IntPtr)Unsafe.AsPointer(ref MemoryMarshal.GetReference(wgpuEntries)),
                entryCount = (uint)layoutData.Length,
            };

            // Create binding group
            Wgpu.BindGroupLayoutImpl wgpuBindGroupLayout = Wgpu.DeviceCreateBindGroupLayout(wgpuDevice, wgpuBindGroupLayoutDesc);

            // Check for error
            if (wgpuBindGroupLayout.Handle == IntPtr.Zero)
                return null;

            // Create binding group
            return new BindGroupLayout(wgpuBindGroupLayout);
        }

        public unsafe BindGroup CreateBindGroup(BindGroupLayout layout, params BindData[] bindData)
        {
            // Create entry
            Span<Wgpu.BindGroupEntry> wgpuEntries = stackalloc Wgpu.BindGroupEntry[bindData.Length];

            // Fill out data
            for (int i = 0; i < bindData.Length; i++)
                wgpuEntries[i] = bindData[i].GetEntry();

            // Create desc
            Wgpu.BindGroupDescriptor wgpuBindGroupDesc = new Wgpu.BindGroupDescriptor
            {
                layout = layout.wgpuBindGroupLayout,
                entries = (IntPtr)Unsafe.AsPointer(ref MemoryMarshal.GetReference(wgpuEntries)),
                entryCount = (uint)bindData.Length,
            };

            // Create bind group
            Wgpu.BindGroupImpl wgpuBindGroup = Wgpu.DeviceCreateBindGroup(wgpuDevice, wgpuBindGroupDesc);

            // Check for error
            if(wgpuBindGroup.Handle == IntPtr.Zero) 
                return null;

            // Create bind group
            return new BindGroup(wgpuBindGroup);
        }

        public CommandList CreateCommandList()
        {
            // Create encoder
            Wgpu.CommandEncoderImpl wgpuEncoder = Wgpu.DeviceCreateCommandEncoder(wgpuDevice, default);

            // Check for error
            if (wgpuEncoder.Handle == IntPtr.Zero)
                return null;

            // Create result
            return new CommandList(wgpuEncoder);
        }

        public unsafe RenderPipelineLayout CreateRenderPipelineLayout(BindGroupLayout[] bindGroupLayouts, string label = null)
        {
            // Create entry
            Span<Wgpu.BindGroupLayoutImpl> wgpuBindGroupLayouts = stackalloc Wgpu.BindGroupLayoutImpl[bindGroupLayouts.Length];

            // Fill out data
            for (int i = 0; i < bindGroupLayouts.Length; i++)
                wgpuBindGroupLayouts[i] = bindGroupLayouts[i].wgpuBindGroupLayout;

            // Create desc
            Wgpu.PipelineLayoutDescriptor wgpuPipelineLayoutDesc = new Wgpu.PipelineLayoutDescriptor
            {
                label = label,
                bindGroupLayouts = new IntPtr(Unsafe.AsPointer(ref wgpuBindGroupLayouts.GetPinnableReference())),
                bindGroupLayoutCount = (uint)bindGroupLayouts.Length,
            };

            // Create pipeline layout
            Wgpu.PipelineLayoutImpl wgpuPipelineLayout = Wgpu.DeviceCreatePipelineLayout(wgpuDevice, wgpuPipelineLayoutDesc);

            // Check for error
            if(wgpuPipelineLayout.Handle == IntPtr.Zero) 
                return null;

            // Create result
            return new RenderPipelineLayout(wgpuPipelineLayout);
        }

        public unsafe RenderPipeline CreateRenderPipeline(RenderPipelineLayout layout, Shader shader, in VertexState vertexState, in PrimitiveState primitiveState, in MultisampleState multisampleState, FragmentState? fragmentState = null, DepthStencilState? depthStencilState = null, string label = null)
        {
            // Create desc
            Wgpu.RenderPipelineDescriptor wgpuRenderPipelineDesc = new Wgpu.RenderPipelineDescriptor
            {
                layout = layout.wgpuPipelineLayout,
                vertex = new Wgpu.VertexState
                {
                    module = shader.wgpuShader,
                    entryPoint = vertexState.EntryPoint,
                    buffers = AllocStructEnumerablePtr(vertexState.BufferLayouts.Select(b => new Wgpu.VertexBufferLayout
                    {
                        arrayStride = (ulong)b.ArrayStride,
                        stepMode = (Wgpu.VertexStepMode)b.StepMode,
                        attributes = AllocStructArrayPtr(b.Attributes),
                        attributeCount = (uint)b.Attributes.Length,
                    }), vertexState.BufferLayouts.Length),
                    bufferCount = (uint)vertexState.BufferLayouts.Length,
                },
                primitive = new Wgpu.PrimitiveState
                {
                    topology = (Wgpu.PrimitiveTopology)primitiveState.Topology,
                    stripIndexFormat = (Wgpu.IndexFormat)primitiveState.StripIndexFormat,
                    frontFace = (Wgpu.FrontFace)primitiveState.FrontFace,
                    cullMode = (Wgpu.CullMode)primitiveState.CullMode,
                },
                multisample = new Wgpu.MultisampleState
                {
                    count = (uint)multisampleState.Count,
                    mask = multisampleState.Mask,
                    alphaToCoverageEnabled = multisampleState.AlphaToCoverageEnabled ? 1u : 0u,
                },
                fragment = fragmentState != null ? AllocStructPtr(new Wgpu.FragmentState
                {
                    module = shader.wgpuShader,
                    entryPoint = fragmentState.Value.EntryPoint,
                    targets = AllocStructEnumerablePtr(fragmentState.Value.ColorTargets.Select(c => new Wgpu.ColorTargetState
                    {
                        format = (Wgpu.TextureFormat)c.Format,
                        blend = c.BlendState != null ? AllocStructPtr(c.BlendState.Value) : IntPtr.Zero,
                        writeMask = (uint)c.WriteMask,
                    }), fragmentState.Value.ColorTargets.Length),
                    targetCount = (uint)fragmentState.Value.ColorTargets.Length,
                }) : IntPtr.Zero,
                depthStencil = depthStencilState != null ? AllocStructPtr(depthStencilState.Value) : IntPtr.Zero,
            };

            // Create pipeline
            Wgpu.RenderPipelineImpl wgpuRenderPipeline = Wgpu.DeviceCreateRenderPipeline(wgpuDevice, wgpuRenderPipelineDesc);

            //// Free memory
            //if(fragmentStatePtr != IntPtr.Zero) Marshal.FreeHGlobal(fragmentStatePtr);
            //if(depthStencilPtr != IntPtr.Zero) Marshal.FreeHGlobal(depthStencilPtr);

            // Check for created
            if (wgpuRenderPipeline.Handle == IntPtr.Zero)
                return null;

            // Create result
            return new RenderPipeline(wgpuRenderPipeline);
        }



        public unsafe QuerySet CreateQuerySet(QueryType type, int count, PipelineStatisticName[] pipelineStatistics)
        {
            fixed(PipelineStatisticName* pipelineStats = pipelineStatistics)
            {
                // Create desc
                Wgpu.QuerySetDescriptor wgpuQueryDesc = new Wgpu.QuerySetDescriptor
                {
                    type = (Wgpu.QueryType)type,
                    count = (uint)count,
                };

                // Create query
                Wgpu.QuerySetImpl wgpuQuery = Wgpu.DeviceCreateQuerySet(wgpuDevice, wgpuQueryDesc);

                // Create result
                return new QuerySet(wgpuQuery);
            }
        }

        internal void OnDeviceLost(string reason)
        {

        }

        private static void ErrorCallback(Wgpu.ErrorType type, string message)
        {
            Console.WriteLine(type.ToString() + ": " + message);
            Debug.LogErrorF(LogFilter.Graphics, "Device error!: [{0}] - {1}", type, message);
        }

        private static IntPtr AllocStructPtr<T>(T val) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(val));
            Marshal.StructureToPtr(val, ptr, false);

            return ptr;
        }

        private static unsafe IntPtr AllocStructEnumerablePtr<T>(IEnumerable<T> items, int count) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>() * count);
            Span<T> arrSpan = new Span<T>((void*)ptr, count);
            int index = 0;

            foreach(T item in items)
            {
                arrSpan[index] = item;
                index++;
            }
            return ptr;
        }

        private static unsafe IntPtr AllocStructArrayPtr<T>(T[] arr) where T : struct
        {
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>() * arr.Length);
            Span<T> arrSpan = new Span<T>((void*)ptr, arr.Length);

            for(int i = 0; i < arr.Length; i++)
                arrSpan[i] = arr[i];

            return ptr;
        }
    }
}
