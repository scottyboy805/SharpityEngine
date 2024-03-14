using SharpityEngine.Scene;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    public abstract class Component : GameElement
    {
        // Private
        [DataMember(Name = "Enabled")]
        private bool enabled = true;

        // Internal
        internal GameObject gameObject = null;

        // Properties
        public bool Enabled
        {
            get { return enabled; }
            set 
            {
                // Trigger enable with event
                DoComponentEnabledEvents(this, value);
            }
        }

        public bool EnabledInHierarchy
        {
            get { return enabled == true && gameObject.EnabledInHierarchy == true; }
        }

        public GameObject GameObject
        {
            get { return gameObject; }
        }

        public GameScene Scene
        {
            get { return gameObject.Scene; }
        }

        // Constructor
        protected Component()
        {
        }

        // Methods
        public bool CompareTag(string tag)
        {
            return string.Compare(gameObject.Tag, tag, StringComparison.OrdinalIgnoreCase) == 0;
        }


        #region HierarchyEvents
        internal static void DoComponentEnabledEvents(Component component, bool enabled, bool forceUpdate = false)
        {
            // Store current enabled state
            bool currentEnabledState = component.enabled;

            // Change enabled state
            component.enabled = enabled;

            // Check for disabled in hierarchy
            if (component.Scene.Enabled == false || component.EnabledInHierarchy == false || (currentEnabledState == enabled && forceUpdate == false))
                return;

            // Handle event
            if (component is IGameEnable)
            {
                // Trigger event
                try
                {
                    if (enabled == true) ((IGameEnable)component).OnEnable();
                    else ((IGameEnable)component).OnDisable();
                }
                catch (Exception e) { Debug.LogException(e); }
            }
        }
        #endregion
    }
}
