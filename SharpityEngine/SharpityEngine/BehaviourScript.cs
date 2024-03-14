
namespace SharpityEngine
{
    public abstract class BehaviourScript : Component, IGameEnable, IGameUpdate
    {
        // Private
        private int priority = 0;
        private Transform transform = null;

        // Properties
        public int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public Transform Transform
        {
            get
            {
                // Get transform
                if (transform == null)
                    transform = GameObject.Transform;

                return transform;
            }
        }

        // Methods
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        public virtual void OnAwake() { }
        public virtual void OnStart() { }

        public virtual void OnUpdate(GameTime gameTime) { }
    }
}
