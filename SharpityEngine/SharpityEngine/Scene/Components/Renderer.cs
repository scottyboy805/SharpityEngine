using SharpityEngine.Graphics;
using System.Runtime.Serialization;

namespace SharpityEngine.Scene
{
    public abstract class Renderer : Component, IGameDraw, IGameEnable
    {
        // Private
        private static Material errorMaterial = null;

        // Protected
        [DataMember(Name = "DrawOrder")]
        protected int drawOrder = 0;
        [DataMember(Name = "Materials")]
        protected Material[] materials = new Material[0];

        // Properties
        public abstract Bounds Bounds { get; }

        public int DrawOrder
        {
            get { return drawOrder; }
        }

        public Material Material
        {
            get { return materials.Length > 0 ? materials[0] : null; }
            set
            {
                // Ensure array size
                if(materials.Length < 1)
                    Array.Resize(ref materials, 1);

                // Store materials
                materials[0] = value;
            }
        }

        public Material[] Materials
        {
            get { return materials; }
            set { materials = value != null ? value : new Material[0]; }
        }

        internal Material ErrorMaterial
        {
            get
            {
                if (errorMaterial == null)
                    errorMaterial = Game.Content.Load<Material>("Error.wgsl");

                return errorMaterial;
            }
        }

        // Methods
        public virtual void OnAfterDraw() { }

        public virtual void OnBeforeDraw() { }

        public virtual void OnEnable()
        {
            // Submit renderer
            Scene.sceneDrawCalls.Add(this);
        }

        public virtual void OnDisable()
        {
            // Remove renderer
            Scene.sceneDrawCalls.Remove(this);
        }

        public abstract void OnDraw(BatchRenderer batchRenderer);
    }
}
