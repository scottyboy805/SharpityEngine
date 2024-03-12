using SharpityEngine.Graphics.Pipeline;
using System.Runtime.InteropServices;

namespace SharpityEngine.Graphics
{
    public sealed class Mesh : GameAsset
    {
        // Type
        [Flags]
        internal enum MeshFlags
        {
            None = 0,
            Vertices = 1,
            Normals = 2,
            //Tangents = 4,
            UVs_0 = 8,
            UVs_1 = 16,
            Colors = 32,
        }

        private struct MeshVertex
        {
            // Public
            public Vector3 Position;            
            public Vector3 Normal;
            public Color Color;
            public Vector2 UV_0;
            public Vector2 UV_1;            
        }

        public sealed class SubMesh
        {
            // Internal
            internal GraphicsBuffer VertexBuffer = null;
            internal VertexBufferLayout VertexBufferLayout = default;
            internal Bounds bounds = default;
            internal MeshFlags flags = 0;

            internal PrimitiveTopology topology = PrimitiveTopology.TriangleStrip;
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

            public IList<Vector3> Vertices
            {
                get
                {
                    if (vertices == null) vertices = new List<Vector3>();
                    return vertices;
                }
            }

            public IList<Vector3> Normals
            {
                get
                {
                    if(normals == null) normals = new List<Vector3>();
                    return normals;
                }
            }

            public IList<Vector2> UVs_0
            {
                get
                {
                    if(uvs_0 == null) uvs_0 = new List<Vector2>();
                    return uvs_0;
                }
            }

            public IList<Vector2> UVs_1
            {
                get
                {
                    if(uvs_1 == null) uvs_1 = new List<Vector2>();
                    return uvs_1;
                }
            }

            public IList <Color> Colors
            {
                get
                {
                    if(colors == null) colors = new List<Color>();
                    return colors;
                }
            }
        }

        // Internal
        internal unsafe static readonly VertexBufferLayout MeshBufferLayout = 
            new VertexBufferLayout(sizeof(MeshVertex),
            new VertexAttribute(VertexFormat.Float32x3, 0, 0),
            new VertexAttribute(VertexFormat.Float32x3, sizeof(Vector3), 1),
            new VertexAttribute(VertexFormat.Float32x2, sizeof(Vector3) * 2, 2),
            new VertexAttribute(VertexFormat.Float32x2, (sizeof(Vector3) * 2) + sizeof(Vector2), 3),
            new VertexAttribute(VertexFormat.Float32x4, (sizeof(Vector3) * 2) + (sizeof(Vector2) * 2), 4));


        // Private
        private Bounds bounds = default;
        private List<SubMesh> subMeshes = new List<SubMesh>();        
        
        // Properties
        public Bounds Bounds
        {
            get { return bounds; }
        }

        public IReadOnlyList<SubMesh> SubMeshes
        {
            get { return subMeshes; }
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
                    if (subMeshes[i].VertexBuffer != null)
                    {
                        // Release buffer
                        subMeshes[i].VertexBuffer.Dispose();
                        subMeshes[i].VertexBuffer = null;
                    }
                }

                // Clear all sub meshes
                subMeshes.Clear();
            }
            else
            {
                // Check for buffer created
                if (subMeshes[subMesh].VertexBuffer != null)
                {
                    // Release buffer
                    subMeshes[subMesh].VertexBuffer.Dispose();
                    subMeshes[subMesh].VertexBuffer = null;
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

        public void Finalize(int subMesh = -1)
        {
            // Check for all
            if(subMesh == -1)
            {
                // Finalize all sub meshes
                for (int i = 0; i < subMeshes.Count; i++)
                    FinalizeSubMesh(subMeshes[i]);
            }
            else
            {
                // Finalize specific sub mesh
                FinalizeSubMesh(subMeshes[subMesh]);
            }
        }

        private unsafe void FinalizeSubMesh(SubMesh subMesh)
        {
            // Invalidate sub mesh
            subMesh.VertexBuffer?.Dispose();
            subMesh.VertexBuffer = null;
            subMesh.VertexBufferLayout = default;
            subMesh.bounds = default;
            subMesh.flags = default;

            // Calculate flags
            MeshFlags subMeshFlags = 0;

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
            subMesh.VertexBufferLayout = MeshBufferLayout;

            // Create vertices
            Span<MeshVertex> vertices = new MeshVertex[subMesh.vertices.Count];

            // Fill out data
            for(int i = 0; i < vertices.Length; i++)
            {
                // Create vertex
                vertices[i] = new MeshVertex
                {
                    Position = subMesh.vertices[i],
                    Normal = (subMeshFlags & MeshFlags.Normals) != 0 ? subMesh.normals[i] : default,
                    Color = (subMeshFlags & MeshFlags.Colors) != 0 ? subMesh.colors[i] : default,
                    UV_0 = (subMeshFlags & MeshFlags.UVs_0) != 0 ? subMesh.uvs_0[i] : default,
                    UV_1 = (subMeshFlags & MeshFlags.UVs_1) != 0 ? subMesh.uvs_1[i] : default,                    
                };
            }

            // Create buffer
            subMesh.VertexBuffer = Game.GraphicsDevice.CreateBuffer(vertices, BufferUsage.Vertex);
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
