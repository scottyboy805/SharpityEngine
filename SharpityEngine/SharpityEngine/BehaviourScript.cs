
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

        #region CreateComponent
        public Component CreateComponent(Type componentType)
        {
            return GameObject.CreateComponent(componentType);
        }

        public T CreateComponent<T>() where T : Component
        {
            return GameObject.CreateComponent<T>();
        }

        public void CreateComponent(Component existingComponent)
        {
            GameObject.CreateComponent(existingComponent);
        }
        #endregion

        #region GetComponent
        public Component GetComponent(Type type, bool includeDisabled = false)
        {
            return GameObject.GetComponent(type, includeDisabled);
        }

        public T GetComponent<T>(bool includeDisabled = false) where T : class
        {
            return GameObject.GetComponent<T>(includeDisabled);
        }    

        public T[] GetComponents<T>(bool includeDisabled = false) where T : class
        {
            return GameObject.GetComponents<T>(includeDisabled);
        }

        public int GetComponents<T>(IList<T> results, bool includeDisabled = false) where T : class
        {
            return GameObject.GetComponents<T>(results, includeDisabled);
        }

        public T GetComponentInChildren<T>(bool includeDisabled = false, string tag = null) where T : class
        {
            return GameObject.GetComponentInChildren<T>(includeDisabled);
        }

        public T[] GetComponentsInChildren<T>(bool includeDisabled = false, string tag = null) where T : class
        {
            return GameObject.GetComponentsInChildren<T>(includeDisabled, tag);
        }

        public int GetComponentsInChildren<T>(IList<T> results, bool includeDisabled = false, string tag = null) where T : class
        {
            return GameObject.GetComponentsInChildren<T>(results, includeDisabled, tag);
        }

        public T GetComponentInParent<T>(bool includeDisabled = false, string tag = null) where T : class
        {
            return GameObject.GetComponentInParent<T>(includeDisabled, tag);
        }

        public T[] GetComponentsInParent<T>(bool includeDisabled = false, string tag = null) where T : class
        {
            return GameObject.GetComponentsInParent<T>(includeDisabled, tag);
        }

        public int GetComponentsInParent<T>(IList<T> results, bool includeDisabled = false, string tag = null) where T : class
        {
            return GameObject.GetComponentsInParent<T>(results, includeDisabled, tag);
        }
        #endregion
    }
}
