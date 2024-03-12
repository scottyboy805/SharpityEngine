using SharpityEngine.Graphics.Pipeline;
using System.Runtime.Serialization;

namespace SharpityEngine.Graphics
{
    public sealed class Material : GameAsset
    {
        // Internal
        internal GraphicsBuffer uniformBuffer = null;
        internal BindGroup bindGroup = null;

        // Private
        [DataMember(Name = "Shader")]
        private Shader shader = null;
        private unsafe BindData[] bindingData =
        {
            BindData.Buffer(null, 0, 0, sizeof(Matrix4)),
            BindData.Sampler(null, 1),
            BindData.Texture(null, 2),
        };

        // Properties
        public Shader Shader
        {
            get { return shader; }
            set
            {
                shader = value;
                UpdateBindData();
            }
        }

        public int RenderQueue
        {
            get { return shader != null ? shader.RenderQueue : -1; }
        }

        // Constructor
        public Material() { }

        public Material(Shader shader)
        {
            this.shader = shader;
        }

        // Methods
        protected override void OnLoaded()
        {
            // Update binding
            UpdateBindData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Release bind group
            if (bindGroup != null)
            {
                bindGroup.Dispose();
                bindGroup = null;
            }

            // Release uniform buffer
            if(uniformBuffer != null)
            {
                uniformBuffer.Dispose();
                uniformBuffer = null;
            }
        }

        public void SetBuffer(GraphicsBuffer buffer, int slot, long offset = 0, long size = -1)
        {
            // Get size
            if (size == -1)
                size = buffer.SizeInBytes;

            // Create buffer
            bindingData[slot] = BindData.Buffer(buffer, slot, offset, size);
        }

        public void SetSampler(Sampler sampler, int slot)
        {
            // Create sample
            bindingData[slot] = BindData.Sampler(sampler, slot);
        }

        public void SetTextureView(TextureView textureView, int slot)
        {
            // Create texture
            bindingData[slot] = BindData.Texture(textureView, slot);
        }

        public void Apply()
        {
            UpdateBindData();
        }

        private unsafe void UpdateBindData()
        {
            // Release bind group
            if(bindGroup != null)
            {
                bindGroup.Dispose();
                bindGroup = null;
            }

            // Check for shader
            if (shader == null || shader.renderPipeline == null)
                return;

            // Create buffer
            if(uniformBuffer == null)
                uniformBuffer = Game.GraphicsDevice.CreateBuffer(sizeof(Matrix4), BufferUsage.Uniform | BufferUsage.CopyDst);

            // Create bind group
            bindGroup = Game.GraphicsDevice.CreateBindGroup(shader.bindGroupLayout, bindingData);
        }
    }
}
