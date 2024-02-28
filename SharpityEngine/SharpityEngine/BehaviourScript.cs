
namespace SharpityEngine
{
    public abstract class BehaviourScript : GameElement, IGameEnable, IGameUpdate
    {
        // Private
        private int priority = 0;

        // Properties
        public int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        // Methods
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        public virtual void OnAwake() { }
        public virtual void OnStart() { }

        public virtual void OnUpdate(GameTime gameTime) { }
    }
}
