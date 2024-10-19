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

        protected virtual void RegisterSubSystems()
        {
            // Register for draw
            if (this is IGameDraw) 
                Scene.sceneDrawCalls.Add((IGameDraw)this);

            // Register for update
            if (this is IGameUpdate) 
                Scene.sceneUpdateCalls.Add((IGameUpdate)this);
        }
        protected virtual void UnregisterSubSystems()
        {
            // Unregister draw
            if (this is IGameDraw) 
                Scene.sceneDrawCalls.Remove((IGameDraw)this);

            // Unregister update
            if (this is IGameUpdate) 
                Scene.sceneUpdateCalls.Remove((IGameUpdate)this);
        }


        #region HierarchyEvents
        internal static void DoComponentSceneInitialize(Component component, GameObject gameObject)
        {
            if(component != null)
            {
                component.gameObject = gameObject;
            }
        }

        internal static void DoComponentEnabledEvents(Component component, bool enabled, bool forceUpdate = false)
        {
            // Store current enabled state
            bool currentEnabledState = component.enabled;

            // Change enabled state
            component.enabled = enabled;

            // Check for disabled in hierarchy
            if (component.Scene.Enabled == false || component.EnabledInHierarchy == false || (currentEnabledState == enabled && forceUpdate == false))
                return;

            // Trigger enabled event for component
            OnComponentEnabledEvent(component, enabled);
        }

        internal static void OnComponentEnabledEvent(Component component, bool enabled)
        {
            // Handle event
            if (component is IGameEnable)
            {
                // Trigger event
                try
                {
                    if (enabled == true)
                    {
                        // Call enable
                        ((IGameEnable)component).OnEnable();
                    }
                    else
                    {
                        // Call disable
                        ((IGameEnable)component).OnDisable();
                    }
                }
                catch (Exception e) { Debug.LogException(e); }
            }

            // Register component
            if(enabled == true)
            {
                // Register component with required sub systems/modules
                component.RegisterSubSystems();
            }
            else
            {
                // Unregister component with required sub systems/modules
                component.UnregisterSubSystems();
            }
        }
        #endregion
    }
}
