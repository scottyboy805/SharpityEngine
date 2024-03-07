using System.Reflection.Metadata.Ecma335;
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

        private Transform parent = null;
        private List<Transform> children = null;

        // Properties
        
        public Transform Parent
        {
            get { return parent; }
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
        }

        // Methods
        private void RebuildTransform()
        {

        }
    }
}
