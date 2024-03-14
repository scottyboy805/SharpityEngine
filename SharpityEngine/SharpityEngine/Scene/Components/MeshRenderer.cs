using SharpityEngine.Graphics;
using System.Runtime.Serialization;

namespace SharpityEngine.Scene
{
    public enum ShadowMode
    {
        /// <summary>
        /// No shadows.
        /// </summary>
        None = 0,
        /// <summary>
        /// Shadows and meshes are both rendered.
        /// </summary>
        On,
        /// <summary>
        /// Only shadows are rendered.
        /// </summary>
        ShadowsOnly,
    }

    public class MeshRenderer : Renderer
    {
        // Private
        [DataMember(Name = "Mesh")]
        private Mesh mesh = null;
        [DataMember(Name = "ShadowMode")]
        private ShadowMode shadowMode = ShadowMode.On;

        private bool meshError = false;

        // Properties
        public override Bounds Bounds
        {
            get { return mesh != null ? mesh.Bounds : default; }
        }

        public Mesh Mesh
        {
            get { return mesh; }
            set 
            {
                if (mesh != value)
                {
                    mesh = value;
                    meshError = false;
                }
            }
        }

        public ShadowMode ShadowMode
        {
            get { return shadowMode; }
            set { shadowMode = value; }
        }

        // Methods
        public override void OnDraw(BatchRenderer batchRenderer)
        {
            // Check for mesh
            if (mesh == null)
                return;

            // Get number of elements
            int subMeshCount = mesh.SubMeshCount;

            // Draw all
            for (int i = 0; i < subMeshCount; i++)
            {
                // Get material
                Material mat = materials.Length >= i ? materials[i] : null;

                // Get sub mesh
                Mesh.SubMesh subMesh = mesh.SubMeshes[i];

                // Get mesh buffers
                GraphicsBuffer vertexBuffer = subMesh.vertexBuffer;
                GraphicsBuffer indexBuffer = subMesh.indexBuffer;

                // Check for vertex buffer required as a minimum
                if(vertexBuffer == null)
                {
                    // Check for error
                    if(meshError == false)
                    {
                        meshError = true;
                        Debug.LogError(LogFilter.Graphics, "Mesh changes have not been applied and will not be rendered: " + mesh.Name
                            + (subMeshCount > 1 ? " [SubMesh]:" + i : ""));
                    }

                    // No buffer to draw
                    return;
                }

                // Render mesh
                batchRenderer.DrawBatched(mat,
                    vertexBuffer, subMesh.Vertices.Count,
                    indexBuffer, subMesh.IndexCount, subMesh.indexFormat);
            }
        }
    }
}
