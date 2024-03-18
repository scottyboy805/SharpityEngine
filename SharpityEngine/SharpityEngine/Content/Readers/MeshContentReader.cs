using SharpityEngine.Graphics;

namespace SharpityEngine.Content.Readers
{
    internal sealed class MeshContentReader : IContentReader
    {
        // Type
        public struct MeshHeader
        {
            // Public
            public uint SubMeshCount;
        }

        public struct SubMeshHeader
        {
            // Public
            public Mesh.MeshFlags Flags;
            public IndexFormat IndexFormat;
            public uint VerticesCount;
            public uint IndicesCount;
        }

        // Properties
        public bool RequireStreamSeeking => false;

        // Methods
        public unsafe Task<object> ReadContentAsync(Stream readStream, IContentReader.ContentReadContext context, CancellationToken cancelToken)
        {
            // Create binary
            BinaryReader reader = new BinaryReader(readStream);

            // Read header
            MeshHeader header = default;

            header.SubMeshCount = reader.ReadUInt32();

            // Create result mesh
            Mesh mesh = new Mesh(context.ContentName);


            // Get mesh bounds
            Bounds bounds = default;
            bounds.Center.X = reader.ReadSingle();
            bounds.Center.Y = reader.ReadSingle();
            bounds.Center.Z = reader.ReadSingle();
            bounds.Extents.X = reader.ReadUInt32();
            bounds.Extents.Y = reader.ReadUInt32();
            bounds.Extents.Z = reader.ReadUInt32();
            mesh.bounds = bounds;


            // Read all sub meshes
            for (int i = 0; i < header.SubMeshCount; i++) 
            {
                // Read sub header
                SubMeshHeader subHeader = default;

                subHeader.Flags = (Mesh.MeshFlags)reader.ReadUInt32();
                subHeader.IndexFormat = (IndexFormat)reader.ReadUInt32();
                subHeader.VerticesCount = reader.ReadUInt32();
                subHeader.IndicesCount = reader.ReadUInt32();

                // Create sub mesh
                Mesh.SubMesh subMesh = mesh.CreateSubMesh();

                // Init sub mesh
                subMesh.indexFormat = subHeader.IndexFormat;
                subMesh.topology = PrimitiveTopology.TriangleStrip;

                // Check for indices
                if ((subHeader.Flags & Mesh.MeshFlags.Index) != 0 && subHeader.IndicesCount > 0)
                {
                    // Check index format 32
                    if (subHeader.IndexFormat == IndexFormat.Uint32)
                    {
                        // Create array
                        uint[] indices = new uint[subHeader.IndicesCount];

                        // Get pointer to array
                        fixed (uint* indicesPtr = indices)
                        {
                            // Read all data as a block
                            reader.Read(new Span<byte>(indicesPtr, indices.Length * sizeof(uint)));

                            // Fill buffer directly
                            subMesh.indexBuffer = context.GraphicsDevice.CreateBuffer(indices.AsSpan(), BufferUsage.Index, context.ContentName + " Index Buffer");
                        }
                    }
                    // Check for format 16
                    else if (subMesh.indexFormat == IndexFormat.Uint16)
                    {
                        // Create array
                        ushort[] indices = new ushort[subHeader.IndicesCount];

                        // Get pointer to array
                        fixed(ushort* indicesPtr = indices)
                        {
                            // Read all data as a block
                            reader.Read(new Span<byte>(indicesPtr, indices.Length * sizeof(ushort)));

                            // Fill buffer directly
                            subMesh.indexBuffer = context.GraphicsDevice.CreateBuffer(indices.AsSpan(), BufferUsage.Index, context.ContentName + " Index Buffer");
                        }
                    }
                    else
                        Debug.LogWarningF("Unknown format when reading mesh index buffer[{0}]: {1}", i, context.ContentPath);
                }

                // Check for vertices
                if(subHeader.VerticesCount > 0)
                {
                    // Create array
                    Mesh.MeshVertex[] vertices = new Mesh.MeshVertex[subHeader.VerticesCount];

                    // Create buffer
                    Vector3[] bufferV3 = new Vector3[subHeader.VerticesCount];
                    Vector2[] bufferV2 = null;
                    Color[] bufferC4 = null;

                    // Read vertices
                    {          
                        // Get pointer to array
                        fixed(Vector3* bufferPtr = bufferV3)
                        {
                            // Read all data as a block
                            reader.Read(new Span<byte>(bufferPtr, bufferV3.Length * sizeof(Vector3)));

                            // Insert array
                            Vector3* ptr = bufferPtr;
                            for (int j = 0; j < bufferV3.Length; j++)
                                vertices[j].Position = *ptr++;
                        }
                    }

                    // Read normals
                    if((subHeader.Flags & Mesh.MeshFlags.Normals) != 0)
                    {
                        // Get pointer to array
                        fixed(Vector3* bufferPtr = bufferV3)
                        {
                            // Read all data as a block
                            reader.Read(new Span<byte>(bufferPtr, bufferV3.Length * sizeof(Vector3)));

                            //// Insert array
                            //Vector3* ptr = bufferPtr;
                            //for (int j = 0; j < bufferV3.Length; j++)
                            //    vertices[j].Normal = *ptr++;
                        }
                    }

                    // Read colors
                    if((subHeader.Flags & Mesh.MeshFlags.Colors) != 0)
                    {
                        // Init buffer
                        if(bufferC4 == null) bufferC4 = new Color[subHeader.VerticesCount];

                        // Get pointer to array
                        fixed(Color* bufferPtr = bufferC4)
                        {
                            // Read all data as a block
                            reader.Read(new Span<byte>(bufferPtr, bufferC4.Length * sizeof(Color)));

                            // Insert array
                            Color* ptr = bufferPtr;
                            for(int j = 0; j < bufferC4.Length; j++)
                                vertices[j].Color = *ptr++;
                        }
                    }

                    // Read UV0
                    if ((subHeader.Flags & Mesh.MeshFlags.UVs_0) != 0)
                    {
                        // Init buffer
                        if(bufferV2 == null) bufferV2 = new Vector2[subHeader.VerticesCount];

                        // Get pointer to array
                        fixed(Vector2* bufferPtr = bufferV2)
                        {
                            // Read all data as a block
                            reader.Read(new Span<byte>(bufferPtr, bufferV2.Length * sizeof(Vector2)));

                            // Insert array
                            Vector2* ptr = bufferPtr;
                            for(int j = 0; j < bufferV2.Length; j++)
                                vertices[j].UV_0 = *ptr++;
                        }
                    }

                    // Read UV1
                    if ((subHeader.Flags & Mesh.MeshFlags.UVs_1) != 0)
                    {
                        // Init buffer
                        if (bufferV2 == null) bufferV2 = new Vector2[subHeader.VerticesCount];

                        // Get pointer to array
                        fixed (Vector2* bufferPtr = bufferV2)
                        {
                            // Read all data as a block
                            reader.Read(new Span<byte>(bufferPtr, bufferV2.Length * sizeof(Vector2)));

                            //// Insert array
                            //Vector2* ptr = bufferPtr;
                            //for (int j = 0; j < bufferV2.Length; j++)
                            //    vertices[j].UV_1 = *ptr++;
                        }
                    }

                    // Fill buffer directly
                    subMesh.vertexBuffer = context.GraphicsDevice.CreateBuffer(vertices.AsSpan(), BufferUsage.Vertex, context.ContentName + " Vertex Buffer");
                }

                // Check for cancelled
                cancelToken.ThrowIfCancellationRequested();
            }

            // Dispose
            reader.Dispose();

            // Create result
            return Task.FromResult<object>(mesh);
        }
    }
}
