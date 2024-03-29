﻿using SharpityEngine.Graphics.Pipeline;
using System.Runtime.InteropServices;

namespace SharpityEngine.Graphics
{
    public sealed class Mesh : GameAsset
    {
        // Type
        [Flags]
        internal enum MeshFlags : uint
        {
            None = 0,
            Index = 1,
            Vertices = 2,
            Normals = 4,
            //Tangents = 8,
            UVs_0 = 16,
            UVs_1 = 32,
            Colors = 64,
        }

        internal struct MeshVertex
        {
            // Public
            public Vector3 Position;            
            //public Vector3 Normal;
            public Color Color;
            public Vector2 UV_0;
            //public Vector2 UV_1;            
        }

        public sealed class SubMesh
        {
            // Internal
            internal GraphicsBuffer indexBuffer = null;
            internal GraphicsBuffer vertexBuffer = null;
            internal VertexBufferLayout vertexBufferLayout = default;
            internal Bounds bounds = default;
            internal MeshFlags flags = 0;

            internal PrimitiveTopology topology = PrimitiveTopology.TriangleStrip;
            internal IndexFormat indexFormat = IndexFormat.Undefined;
            internal List<ushort> indicesI16 = null;
            internal List<uint> indicesI32 = null;
            internal List<Vector3> vertices = null;
            internal List<Vector3> normals = null;
            internal List<Vector2> uvs_0 = null;
            internal List<Vector2> uvs_1 = null;
            internal List<Color> colors = null;

            // Properties
            public Bounds Bounds
            {
                get { return bounds; }
            }

            public PrimitiveTopology Topology
            {
                get { return topology; }
                set { topology = value; }
            }

            public int IndexCount
            {
                get 
                {
                    return indexFormat switch
                    {
                        IndexFormat.Uint16 => indicesI16.Count,
                        IndexFormat.Uint32 => indicesI32.Count,
                        _ => 0,
                    };
                }
            }

            public List<ushort> IndicesI16
            {
                get
                {
                    if(indicesI16 == null) indicesI16 = new List<ushort>();

                    // Switch index format
                    indexFormat = IndexFormat.Uint16;
                    indicesI32 = null;

                    return indicesI16;
                }
            }

            public List<uint> IndicesI32
            {
                get
                {
                    if(indicesI32 == null) indicesI32 = new List<uint>();

                    // Switch index format
                    indexFormat = IndexFormat.Uint32;
                    indicesI16 = null;

                    return indicesI32;
                }
            }

            public List<Vector3> Vertices
            {
                get
                {
                    if (vertices == null) vertices = new List<Vector3>();
                    return vertices;
                }
            }

            public List<Vector3> Normals
            {
                get
                {
                    if(normals == null) normals = new List<Vector3>();
                    return normals;
                }
            }

            public List<Vector2> UVs_0
            {
                get
                {
                    if(uvs_0 == null) uvs_0 = new List<Vector2>();
                    return uvs_0;
                }
            }

            public List<Vector2> UVs_1
            {
                get
                {
                    if(uvs_1 == null) uvs_1 = new List<Vector2>();
                    return uvs_1;
                }
            }

            public List <Color> Colors
            {
                get
                {
                    if(colors == null) colors = new List<Color>();
                    return colors;
                }
            }

            // Constructor
            internal SubMesh() { }
        }

        // Internal
        //internal unsafe static readonly VertexBufferLayout MeshBufferLayout =
        //    new VertexBufferLayout(sizeof(MeshVertex),
        //    new VertexAttribute(VertexFormat.Float32x3, 0, 0),                                                      // Pos
        //    new VertexAttribute(VertexFormat.Float32x3, sizeof(Vector3), 1),                                        // Normal
        //    new VertexAttribute(VertexFormat.Float32x4, sizeof(Vector3) + sizeof(Vector3), 2),                      // Color
        //    new VertexAttribute(VertexFormat.Float32x2, sizeof(Vector3) + sizeof(Vector3) + sizeof(Vector4), 3));   // UV 0
        //                                                                                                            //new VertexAttribute(VertexFormat.Float32x2, (sizeof(Vector3) * 2) + sizeof(Vector2), 3));

        internal unsafe static readonly VertexBufferLayout MeshBufferLayout =
            new VertexBufferLayout(sizeof(MeshVertex),
            new VertexAttribute(VertexFormat.Float32x3, 0, 0),                                                      // Pos
            //new VertexAttribute(VertexFormat.Float32x3, sizeof(Vector3), 1),                                        // Normal
            new VertexAttribute(VertexFormat.Float32x4, sizeof(Vector3), 1),                      // Color
            new VertexAttribute(VertexFormat.Float32x2, sizeof(Vector3) + sizeof(Vector4), 2));   // UV 0


        // Internal
        internal Bounds bounds = default;
        internal List<SubMesh> subMeshes = new List<SubMesh>();        
        
        // Properties
        public Bounds Bounds
        {
            get { return bounds; }
        }

        public IReadOnlyList<SubMesh> SubMeshes
        {
            get { return subMeshes; }
        }

        public int SubMeshCount
        {
            get { return subMeshes.Count; }
        }

        // Constructor
        public Mesh(string name = null)
            : base(name)
        {
        }

        // Methods
        protected override void OnDestroy()
        {
            // Clear collections
            Clear();
        }

        public void Clear(int subMesh = -1)
        {
            if (subMesh == -1)
            {
                // Release all buffers
                for (int i = 0; i < subMeshes.Count; i++)
                {
                    // Check for buffer created
                    if (subMeshes[i].vertexBuffer != null)
                    {
                        // Release buffer
                        subMeshes[i].vertexBuffer.Dispose();
                        subMeshes[i].vertexBuffer = null;
                    }
                }

                // Clear all sub meshes
                subMeshes.Clear();
            }
            else
            {
                // Check for buffer created
                if (subMeshes[subMesh].vertexBuffer != null)
                {
                    // Release buffer
                    subMeshes[subMesh].vertexBuffer.Dispose();
                    subMeshes[subMesh].vertexBuffer = null;
                }

                subMeshes[subMesh].vertices.Clear();
                subMeshes[subMesh].normals.Clear();
                subMeshes[subMesh].colors.Clear();
                subMeshes[subMesh].uvs_0.Clear();
                subMeshes[subMesh].uvs_1.Clear();                
            }
        }

        public SubMesh CreateSubMesh()
        {
            return CreateSubMesh(out _);
        }

        public SubMesh CreateSubMesh(out int index)
        {
            // Check for limit
            if (subMeshes.Count >= ushort.MaxValue)
                throw new NotSupportedException("Would exceed maximum allowed sub mesh count");

            // Create sub mesh
            SubMesh subMesh = new SubMesh();

            // Assign index
            index = subMeshes.Count;

            // Add sub mesh
            subMeshes.Add(subMesh);
            return subMesh;
        }

        public void CalculateBounds()
        {
            Vector3 min = default, max = default;

            // Update sub mesh
            for(int i = 0; i < subMeshes.Count; i++)
            {
                // Reset bounds
                subMeshes[i].bounds = default;

                // Calculate bounds for sub mesh
                if (subMeshes[i].vertices != null && subMeshes[i].vertices.Count > 0)
                    subMeshes[i].bounds = CalculateBounds(subMeshes[i].vertices);

                // Get min and max
                min = Vector3.Min(min, subMeshes[i].Bounds.Min);
                max = Vector3.Max(max, subMeshes[i].Bounds.Max);
            }

            // Create bounds for collective mesh
            bounds.Min = min;
            bounds.Max = max;
        }

        public void CalculateNormals(int subMesh = -1)
        {
            // Check for all
            if(subMesh == -1)
            {
                // Calculate normals all sub meshes
                for(int i = 0; i < subMeshes.Count; i++)
                    CalculateNormals(subMeshes[i]);
            }
            else
            {
                // Update specific sub mesh
                CalculateNormals(subMeshes[subMesh]);
            }
        }

        private void CalculateNormals(SubMesh subMesh)
        {
            // Check for no vertices
            if (subMesh.vertices == null)
                return;

            // Create normals
            if(subMesh.normals == null || subMesh.normals.Count != subMesh.vertices.Count)
                subMesh.normals = new List<Vector3>(subMesh.vertices.Count);

            // Check for all
            Span<Vector3> vertexSpan = CollectionsMarshal.AsSpan(subMesh.vertices);

            // Process all vertices
            for(int i = 0; i < vertexSpan.Length - 2; i += 3)
            {
                // Get triangle
                Vector3 a = vertexSpan[i];
                Vector3 b = vertexSpan[i + 1];
                Vector3 c = vertexSpan[i + 2];

                //// Calculate normal
                //Vector3 normal = (c - b).Cross(a - b).Normalized;

                //// Update normals
                //subMesh.normals[i] = normal;
                //subMesh.normals[i + 1] = normal;
                //subMesh.normals[i + 2] = normal;
            }
        }

        public void Apply(int subMesh = -1)
        {
            // Check for all
            if(subMesh == -1)
            {
                // Finalize all sub meshes
                for (int i = 0; i < subMeshes.Count; i++)
                    ApplySubMesh(subMeshes[i]);
            }
            else
            {
                // Finalize specific sub mesh
                ApplySubMesh(subMeshes[subMesh]);
            }
        }

        private unsafe void ApplySubMesh(SubMesh subMesh)
        {
            // Invalidate sub mesh
            subMesh.vertexBuffer?.Dispose();
            subMesh.vertexBuffer = null;
            subMesh.vertexBufferLayout = default;
            subMesh.bounds = default;
            subMesh.flags = default;

            // Calculate flags
            MeshFlags subMeshFlags = 0;

            // Indicies
            if ((subMesh.indicesI16 != null && subMesh.indicesI16.Count > 0) || (subMesh.indicesI32 != null && subMesh.indicesI32.Count > 0)) subMeshFlags |= MeshFlags.Index;
            // Vertices
            if (subMesh.vertices != null && subMesh.vertices.Count > 0) subMeshFlags |= MeshFlags.Vertices;            
            // Normals
            if(subMesh.normals != null && subMesh.normals.Count > 0) subMeshFlags |= MeshFlags.Normals;
            // Colors
            if (subMesh.colors != null && subMesh.colors.Count > 0) subMeshFlags |= MeshFlags.Colors;
            // UVs 0
            if (subMesh.uvs_0 != null && subMesh.uvs_0.Count > 0) subMeshFlags |= MeshFlags.UVs_0;
            // UVs 1
            if (subMesh.uvs_1 != null && subMesh.uvs_1.Count > 0) subMeshFlags |= MeshFlags.UVs_1;
            

            // Update flags
            subMesh.flags = subMeshFlags;

            // Check for any vertices
            if ((subMeshFlags & MeshFlags.Vertices) == 0)
                return;

            // Update bounds
            subMesh.bounds = CalculateBounds(subMesh.vertices);

            // Validate normal size
            if ((subMeshFlags & MeshFlags.Normals) != 0 && subMesh.normals.Count != subMesh.vertices.Count)
                throw new InvalidDataException("Normals count must match vertices count");

            // Validate color size
            if ((subMeshFlags & MeshFlags.Colors) != 0 && subMesh.colors.Count != subMesh.vertices.Count)
                throw new InvalidDataException("Color count must match vertices count");

            // Validate uv0 size
            if ((subMeshFlags & MeshFlags.UVs_0) != 0 && subMesh.uvs_0.Count != subMesh.vertices.Count)
                throw new InvalidDataException("UV0 count must match vertices count");

            // Validate uv1 size
            if ((subMeshFlags & MeshFlags.UVs_1) != 0 && subMesh.uvs_1.Count != subMesh.vertices.Count)
                throw new InvalidDataException("UV1 count must match vertices count");
                        

            // Create vertex buffer layout
            subMesh.vertexBufferLayout = MeshBufferLayout;

            // Create vertices
            Span<MeshVertex> vertices = new MeshVertex[subMesh.vertices.Count];

            // Fill out data
            for(int i = 0; i < vertices.Length; i++)
            {
                // Create vertex
                vertices[i] = new MeshVertex
                {
                    Position = subMesh.vertices[i],
                    //Normal = (subMeshFlags & MeshFlags.Normals) != 0 ? subMesh.normals[i] : default,
                    Color = (subMeshFlags & MeshFlags.Colors) != 0 ? subMesh.colors[i] : Color.White,
                    UV_0 = (subMeshFlags & MeshFlags.UVs_0) != 0 ? subMesh.uvs_0[i] : default,
                    //UV_1 = (subMeshFlags & MeshFlags.UVs_1) != 0 ? subMesh.uvs_1[i] : default,                    
                };
            }

            // Create index buffer
            if ((subMeshFlags & MeshFlags.Index) != 0)
            {
                // Check for index size
                if(subMesh.indexFormat == IndexFormat.Uint32)
                {
                    Span<uint> indices = CollectionsMarshal.AsSpan(subMesh.indicesI32);
                    subMesh.indexBuffer = Game.GraphicsDevice.CreateBuffer(indices, BufferUsage.Index, Name + " Index Buffer");
                }
                else if(subMesh.indexFormat == IndexFormat.Uint16)
                {
                    Span<ushort> indices = CollectionsMarshal.AsSpan(subMesh.indicesI16);
                    subMesh.indexBuffer = Game.GraphicsDevice.CreateBuffer(indices, BufferUsage.Index, Name + " Index Buffer");
                }
            }

            // Create vertex buffer
            subMesh.vertexBuffer = Game.GraphicsDevice.CreateBuffer(vertices, BufferUsage.Vertex, Name + " Vertex Buffer");
        }

        private static Bounds CalculateBounds(List<Vector3> vertices)
        {
            Vector3 min = default, max = default;

            // Get as span
            Span<Vector3> vertexSpan = CollectionsMarshal.AsSpan(vertices);

            // Process all vertices
            for(int i = 0; i < vertexSpan.Length; i++)
            {
                min = Vector3.Min(min, vertexSpan[i]);
                max = Vector3.Max(max, vertexSpan[i]);
            }

            // Create bounds for mesh
            Bounds bounds = default;
            bounds.Min = min;
            bounds.Max = max;

            return bounds;
        }
    }
}
