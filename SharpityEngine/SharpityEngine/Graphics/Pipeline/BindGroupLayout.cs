using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharpityEngine.Graphics.Pipeline
{
    public enum ShaderStage
    {
        None = 0,
        Vertex = 1,
        Fragment = 2,
        Compute = 4,
        Force32 = int.MaxValue
    }

    public enum BufferBindingType
    {
        Undefined = 0,
        Uniform = 1,
        Storage = 2,
        ReadOnlyStorage = 3,
    }

    public enum SamplerBindingType
    {
        Undefined = 0,
        Filtering = 1,
        NonFiltering = 2,
        Comparison = 3,
    }

    public enum TextureSampleType
    {
        Float = 1,
        UnfilterableFloat = 2,
        Depth = 3,
        Sint = 4,
        Uint = 5,
    }

    public enum TextureViewDimension
    {
        OneDimension = 1,
        TwoDimensions = 2,
        TwoDimensionalArray = 3,
        Cube = 4,
        CubeArray = 5,
        ThreeDimensions = 6,
    }

    public sealed class BufferBindingLayout : BindingLayout
    {
        // Private
        [DataMember(Name = "BufferType")]
        private BufferBindingType bufferType = BufferBindingType.Uniform;
        [DataMember(Name = "MinBindingSize")]
        private ulong minBindingSize = 0;

        // Properties
        public BufferBindingType BufferType
        {
            get { return bufferType; }
        }

        public ulong MinBindingSize
        {
            get { return minBindingSize; }
        }
    }

    public sealed class SamplerBindingLayout : BindingLayout
    {
        // Private
        [DataMember(Name = "SamplerType")]
        private SamplerBindingType samplerType = SamplerBindingType.Filtering;

        // Properties
        public SamplerBindingType SamplerType
        {
            get { return samplerType; }
        }
    }

    public sealed class TextureBindingLayout : BindingLayout
    {
        // Private
        [DataMember(Name = "SampleType")]
        private TextureSampleType sampleType = TextureSampleType.Float;
        [DataMember(Name = "ViewDimension")]
        private TextureViewDimension viewDimension = TextureViewDimension.TwoDimensions;
        [DataMember(Name = "Multisampled")]
        private bool multisampled = true;

        // Properties
        public TextureSampleType SampleType
        {
            get { return sampleType; }
        }

        public TextureViewDimension ViewDimension
        {
            get { return viewDimension; }
        }

        public bool Multisampled
        {
            get { return multisampled; }
        }
    }

    public sealed class StorageTextureBindingLayout : BindingLayout
    {
        // Private
        [DataMember(Name = "Format")]
        private TextureFormat format = TextureFormat.RGBA32Float;
        [DataMember(Name = "ViewDimension")]
        private TextureViewDimension viewDimension = TextureViewDimension.TwoDimensions;

        // Properties
        public TextureFormat Format
        {
            get { return format; }
        }

        public TextureViewDimension ViewDimension
        {
            get { return viewDimension; }
        }
    }

    public abstract class BindingLayout
    {
        // Private
        [DataMember(Name = "BindingSlot")]
        private uint bindingSlot = 0;
        [DataMember(Name = "ShaderStage")]
        private ShaderStage shaderStage = ShaderStage.Vertex;

        // Properties
        public uint BindingSlot
        {
            get { return bindingSlot; }
        }

        public ShaderStage ShaderStage
        {
            get { return shaderStage; }
        }
    }
}
