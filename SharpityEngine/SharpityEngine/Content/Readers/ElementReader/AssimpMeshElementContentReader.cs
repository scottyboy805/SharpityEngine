using SharpityEngine.Graphics;
using System.Runtime.InteropServices;

namespace SharpityEngine.Content.Readers
{
    internal static class AssimpMeshElementContentReader
    {
        // Type
        [Flags]
        public enum ImportFlags
        {
            None = 0,
            UseIndices = 1,
        }

        // Methods
        public static unsafe Mesh ImportContentAsSingleMesh(Assimp.Scene scene)
        {
            // Check for single mesh
            if (scene.MeshCount < 1)
                return null;

            // Get imp mesh
            Assimp.Mesh impMesh = scene.Meshes[0];

            // Create mesh
            Mesh mesh = new Mesh(impMesh.Name);

            // Create sub mesh
            Mesh.SubMesh subMesh = mesh.CreateSubMesh();

            // Get indices
            Span<int> indices = impMesh.GetIndices();


            // Vertices
            Span<Assimp.Vector3D> vertices = CollectionsMarshal.AsSpan(impMesh.Vertices);
            {
                // Ensure capacity
                subMesh.Vertices.EnsureCapacity(vertices.Length);

                // Process all indexes
                for (int i = 0; i < indices.Length; i++)
                {
                    // Get vertex
                    Assimp.Vector3D vertex = vertices[indices[i]];

                    // Add vertex
                    subMesh.vertices.Add(*(Vector3*)&vertex);
                }
            }


            // Normals
            if (impMesh.HasNormals == true)
            {
                // Get normals collection
                Span<Assimp.Vector3D> normals = CollectionsMarshal.AsSpan(impMesh.Normals);
                
                // Ensure capacity
                subMesh.Normals.EnsureCapacity(normals.Length);

                // Process all indexes
                for (int i = 0; i < indices.Length; i++)
                {
                    // Get normal
                    Assimp.Vector3D normal = normals[indices[i]];

                    // Add normal
                    subMesh.normals.Add(*(Vector3*)&normal);
                }                
            }


            // UV0
            if (impMesh.TextureCoordinateChannelCount > 0)
            {
                // Get uvs collection
                Span<Assimp.Vector3D> uvs = CollectionsMarshal.AsSpan(impMesh.TextureCoordinateChannels[0]);
                
                // Ensure capacity
                subMesh.UVs_0.EnsureCapacity(uvs.Length);

                // Process all indexes
                for(int i = 0; i < indices.Length; i++) 
                { 
                    // Get uv
                    Assimp.Vector3D uv = uvs[indices[i]];

                    // Add uv
                    subMesh.uvs_0.Add(*(Vector2*)&uv);
                }
            }

            // Apply the mesh
            mesh.Apply();

            // Get the mesh result
            return mesh;
        }
    }
}
