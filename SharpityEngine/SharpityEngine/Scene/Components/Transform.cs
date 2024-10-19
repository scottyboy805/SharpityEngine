using System.Runtime.Serialization;

namespace SharpityEngine
{
    public sealed class Transform : Component
    {
        // Private
        [DataMember(Name = "LocalPosition")]
        private Vector3 localPosition = Vector3.Zero;
        [DataMember(Name = "LocalRotation")]
        private Quaternion localRotation = Quaternion.Identity;
        [DataMember(Name = "LocalScale")]
        private Vector3 localScale = Vector3.One;

        // Internal
        internal Matrix4 worldMatrix = Matrix4.Identity;        
        internal Transform parent = null;
        internal Transform root = null;
        internal List<Transform> children = null;

        // Properties        
        public Transform Parent
        {
            get { return parent; }
        }

        public Transform Root
        {
            get { return root; }
        }

        public IReadOnlyList<Transform> Children
        {
            get { return children; }
        }

        public bool IsRoot
        {
            get { return parent != null; }
        }

        public int Depth
        {
            get
            {
                int depth = 0;
                Transform current = this;

                // Move up the hierarchy
                while(current.parent != null)
                {
                    current = current.parent;
                    depth++;
                }
                return depth;
            }
        }

        public bool HasChildren
        {
            get { return children != null && children.Count > 0; }
        }

        public Vector3 LocalPosition
        {
            get { return localPosition; }
            set
            {
                localPosition = value;
                RebuildTransform();
            }
        }

        public Quaternion LocalRotation
        {
            get { return localRotation; }
            set
            {
                localRotation = value;
                RebuildTransform();
            }
        }

        public Vector3 LocalEulerAngle
        {
            get { return localRotation.EulerAngles; }
            set
            {
                localRotation.EulerAngles = value;
                RebuildTransform();
            }
        }

        public Vector3 LocalScale
        {
            get { return localScale; }
            set
            {
                localScale = value;
                RebuildTransform();
            }
        }

        public Vector3 WorldPosition
        {
            get;
            set;
        }

        public Quaternion WorldRotation
        {
            get;
            set;
        }

        public Vector3 Forward
        {
            get { return WorldRotation * Vector3.Forward; }
            //set { WorldRotation = Quaternion.LookRotation(value); }
        }

        // Constructor
        private Transform() { }

        internal Transform(GameObject go)
        {
            this.gameObject = go;
        }

        // Methods
        private void RebuildTransform()
        {
            // Check for static
            if (GameObject.IsStatic == true)
                return;

            // Translation
            Matrix4 translate = Matrix4.Translate(localPosition);

            // Rotation
            Matrix4 rotationX = Matrix4.RotationX(localRotation.EulerAngles.X);
            Matrix4 rotationY = Matrix4.RotationY(localRotation.EulerAngles.Y);
            Matrix4 rotationZ = Matrix4.RotationZ(localRotation.EulerAngles.Z);

            // Scale
            Matrix4 scale = Matrix4.Scale(localScale);

            // Create TRS
            worldMatrix = translate * (rotationX * rotationY * rotationZ) * scale;

            // Check for parent
            if (parent != null)
                worldMatrix = parent.worldMatrix * worldMatrix;

            // Rebuild children
            if(children != null && children.Count > 0)
            {
                for (int i = 0; i < children.Count; i++)
                    children[i].RebuildTransform();
            }
        }
    }
}
