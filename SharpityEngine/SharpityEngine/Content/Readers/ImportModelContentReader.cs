using Assimp;

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
    internal sealed class ImportModelContentReader : IContentReader
    {
        // Properties
        public bool RequireStreamSeeking => false;

        // Methods
        public Task<object> ReadContentAsync(Stream readStream, IContentReader.ContentReadContext context, CancellationToken cancelToken)
        {
            // Create import context
            AssimpContext importContext = new AssimpContext();

            // Configure
            importContext.SetConfig(new Assimp.Configs.FBXPreservePivotsConfig(true));

            // Post process flags
            PostProcessSteps importFlags = PostProcessSteps.Triangulate
                | PostProcessSteps.MakeLeftHanded
                | PostProcessSteps.FlipWindingOrder;

            // Import the asset
            Assimp.Scene scene = importContext.ImportFileFromStream(readStream, importFlags, context.ContentExtension);

            // Get result
            object result = null;

            // Check for mesh or not specified
            if (context.HintType == null || context.HintType == typeof(Graphics.Mesh))
            {
                result = AssimpMeshElementContentReader.ImportContentAsSingleMesh(scene);
            }

            // Check for loaded
            if(result != null)
                return Task.FromResult(result);

            // Fallback to game object import
            return Task.FromResult<object>(null);
        }
    }
}
