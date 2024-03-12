using Assimp;
using System.Runtime.InteropServices;

namespace SharpityEngine.Content.Readers
{
    #region Formats
    [ContentReader(".fbx")]
    [ContentReader(".dae")]
    [ContentReader(".gltf")]
    [ContentReader(".blend")]
    [ContentReader(".3ds")]
    [ContentReader(".ase")]
    [ContentReader(".obj")]
    [ContentReader(".ifc")]
    [ContentReader(".xgl")]
    [ContentReader(".zgl")]
    [ContentReader(".ply")]
    [ContentReader(".dxf")]
    [ContentReader(".lwo")]
    [ContentReader(".lws")]
    [ContentReader(".lxo")]
    [ContentReader(".stl")]
    [ContentReader(".x")]
    [ContentReader(".ac")]
    [ContentReader(".ms3d")]
    [ContentReader(".cob")]
    [ContentReader(".snc")]
    [ContentReader(".bvh")]
    [ContentReader(".csm")]
    [ContentReader(".irrmesh")]
    [ContentReader(".irr")]
    [ContentReader(".mdl")]
    [ContentReader(".md2")]
    [ContentReader(".md3")]
    [ContentReader(".pk3")]
    [ContentReader(".mdc")]
    [ContentReader(".md5")]
    [ContentReader(".smd")]
    [ContentReader(".vta")]
    [ContentReader(".ogex")]
    [ContentReader(".3d")]
    [ContentReader(".b3d")]
    [ContentReader(".q3d")]
    [ContentReader(".q3s")]
    [ContentReader(".nff")]
    [ContentReader(".off")]
    [ContentReader(".raw")]
    [ContentReader(".ter")]
    [ContentReader(".hmp")]
    [ContentReader(".ndo")]
    #endregion
    internal sealed class ModelContentReader : IContentReader
    {
        // Properties
        public bool RequireStreamSeeking => false;

        // Methods
        public Task<object> ReadContentAsync(Stream readStream, in IContentReader.ContentReadContext context, CancellationToken cancelToken)
        {
            // Create import context
            AssimpContext importContext = new AssimpContext();

            // Configure
            importContext.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(false));

            // Post process flags
            PostProcessSteps importFlags = PostProcessSteps.Triangulate
                | PostProcessSteps.MakeLeftHanded
                | PostProcessSteps.FlipWindingOrder;

            // Import the asset
            Assimp.Scene scene = importContext.ImportFileFromStream(readStream, importFlags, context.ContentExtension);

            // Get result
            object result = null;

            // Check for hint type
            if(context.HintType != null)
            {
                // Check for mesh
                if (context.HintType == typeof(Graphics.Mesh))
                {
                    result = ImportContentAsMesh(scene);
                }
            }

            // Check for loaded
            if(result != null)
                return Task.FromResult(result);

            // Fallback to game object import
            return Task.FromResult<object>(null);
        }

        private unsafe object ImportContentAsMesh(Assimp.Scene scene)
        {
            Assimp.Mesh mesh = scene.Meshes[0];

            // Create resulting mesh
            Graphics.Mesh result = new Graphics.Mesh(mesh.Name);

            // Create sub mesh
            Graphics.Mesh.SubMesh subMesh = result.CreateSubMesh();

            // Get indices
            int[] indicies = mesh.GetIndices();

            // Vertices
            Span<Vector3D> vertices = CollectionsMarshal.AsSpan(mesh.Vertices);

            // Resize collection
            subMesh.Vertices.EnsureCapacity(indicies.Length);

            // Store vertices
            for (int i = 0; i < indicies.Length; i++)
            {
                // Get vertex
                Vector3D vert = vertices[indicies[i]];
                subMesh.Vertices.Add(new Vector3(vert.X, vert.Y, vert.Z));
                subMesh.Colors.Add(Color.White);
            }

            // UVs
            Span<Vector3D> uvs = CollectionsMarshal.AsSpan(mesh.TextureCoordinateChannels[0]);

            // Resize collection
            subMesh.UVs_0.EnsureCapacity(uvs.Length);

            // Store uvs
            for (int i = 0; i < indicies.Length; i++)
            {
                Vector3D uv = uvs[indicies[i]];
                subMesh.UVs_0.Add(new Vector2(uv.X, uv.Y));
            }


            // Apply mesh
            result.Apply();

            return result;
        }
    }
}
