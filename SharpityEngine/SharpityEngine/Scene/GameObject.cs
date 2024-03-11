using SharpityEngine.Scene;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    public sealed class GameObject : GameElement
    {
        // Private
        [DataMember(Name = "Enabled")]
        private bool enabled = true;
        [DataMember(Name = "IsStatic")]
        private bool isStatic = false;
        [DataMember(Name = "Layer")]
        private uint layer = 0;
        [DataMember(Name = "Tag")]
        private string tag = "";
        [DataMember(Name = "Transform")]
        private Transform transform = null;
        [DataMember(Name = "Components")]
        private List<Component> components = null;

        private GameScene scene = null;

        // Properties
        public bool EnabledSelf
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public bool EnabledInHierarchy
        {
            get { return false; }
        }

        public bool IsStatic
        {
            get { return isStatic; }
            set { isStatic = value; }
        }

        public uint Layer
        {
            get { return layer; }
            set { layer = value; }
        }

        public string Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        public Transform Transform
        {
            get { return transform; }
        }

        public GameScene Scene
        {
            get { return scene; }
        }

        // Methods
        public override string ToString()
        {
            string nameInfo = (string.IsNullOrEmpty(Name) == false) ? Name : "null";
            return string.Format("{0}({1})", GetType().FullName, nameInfo);
        }

        public Component GetComponent(Type type, bool includeDisabled = false, string tag = null)
        {
            // Get transform
            if (type == typeof(Transform))
                return transform;

            // Get components
            if(components != null)
            {
                foreach (Component component in components)
                {
                    if (type.IsAssignableFrom(component.elementType) == true && CheckComponent(component, includeDisabled, tag) == true)
                        return component;
                }
            }
            return null;
        }

        public T GetComponent<T>(bool includeDisabled = false, string tag = null) where T : class
        {
            // Get transform
            if (typeof(T) == typeof(Transform))
                return transform as T;
            
            // Get components
            if(components != null)
            {
                foreach(Component component in components)
                {
                    if (component is T match && CheckComponent(component, includeDisabled, tag) == true)
                        return match;
                }
            }
            return null;
        }

        public T[] GetComponents<T>(bool includeDisabled = false, string tag = null) where T : class
        {
            // Get transform
            if (typeof(T) == typeof(Transform))
                return new T[] { transform as T };

            // Check for none
            if (components == null || components.Count == 0)
                return Array.Empty<T>();

            // Get components
            return components
                .Where(c => CheckComponent(c, includeDisabled, tag) == true)
                .OfType<T>()                
                .ToArray();
        }

        public int GetComponents<T>(IList<T> results, bool includeDisabled = false, string tag = null) where T : class
        {
            // Get transform
            if (typeof(T) == typeof(Transform))
            {
                results.Add(transform as T);
                return 1;
            }

            // Track count
            int count = 0;

            // Get components
            if (components != null)
            {
                foreach (Component component in components)
                {
                    if (component is T match && CheckComponent(component, includeDisabled, tag) == true)
                    {
                        results.Add(match);
                        count++;
                    }
                }
            }
            // Get count
            return count;
        }

        public T GetComponentInChildren<T>(bool includeDisabled = false, string tag = null) where T : class
        {
            // Get transform
            if (typeof(T) == typeof(Transform))
                return transform as T;

            // Search for component
            return BFSSearchComponentChildren<T>(this, includeDisabled, tag);
        }

        public T[] GetComponentsInChildren<T>(bool includeDisabled = false, string tag = null) where T : class
        {
            // Check for transform
            bool isTransform = typeof(T) == typeof(Transform);

            // Search for components
            return BFSSearchComponentsChildren<T>(this, isTransform, includeDisabled, tag)
                .ToArray();
        }

        public int GetComponentsInChildren<T>(IList<T> results, bool includeDisabled = false, string tag = null) where T : class
        {
            // Check for transform
            bool isTransform = typeof(T) == typeof(Transform);

            // Search for components
            return BFSSearchComponentsChildren<T>(this, isTransform, results, includeDisabled, tag);
        }

        public T GetComponentInParent<T>(bool includeDisabled = false, string tag = null) where T : class
        {
            // Get transform
            if (typeof(T) == typeof(Transform))
                return transform as T;

            // Search for component
            return BFSSearchComponentParent<T>(this, false, includeDisabled, tag);
        }

        public T[] GetComponentsInParent<T>(bool includeDisabled = false, string tag = null) where T : class
        {
            // Check for transform
            bool isTransform = typeof(T) == typeof(Transform);

            // Search for components
            return BFSSearchComponentsParent<T>(this, isTransform, includeDisabled, tag)
                .ToArray();
        }

        public int GetComponentsInParent<T>(IList<T> results, bool includeDisabled = false, string tag = null) where T : class
        {
            // Check for transform
            bool isTransform = typeof(T) == typeof(Transform);

            // Search for components
            return BFSSearchComponentsParent<T>(this, isTransform, results, includeDisabled, tag);
        }

        #region SearchComponents(T)
        private static T BFSSearchComponentChildren<T>(GameObject current, bool includeDisabled, string tag) where T : class
        {
            // Check for any components
            if(current.components != null && current.components.Count > 0)
            {
                // Search all
                foreach(Component component in current.components)
                {
                    if(component is T match && CheckComponent(component, includeDisabled, tag) == true)
                        return match;
                }

                // Search deeper
                foreach(Component component in current.components)
                {
                    // Search inside child component
                    T result = BFSSearchComponentChildren<T>(component.GameObject, includeDisabled, tag);

                    // Check for match
                    if (result != null)
                        return result;
                }
            }
            // Not found
            return null;
        }

        private static IEnumerable<T> BFSSearchComponentsChildren<T>(GameObject current, bool isTransform, bool includeDisabled, string tag) where T : class
        {
            // Check for transform
            if(isTransform == true)
            {
                // Get transform
                yield return current.transform as T;
            }
            else
            {
                // Search all components
                foreach(Component component in current.components)
                {
                    // Check for match
                    if(component is T match && CheckComponent(component, includeDisabled, tag) == true)
                        yield return match;
                }
            }

            // Search deeper
            foreach (Component component in current.components)
            {
                // Search inside child components
                foreach (T result in BFSSearchComponentsChildren<T>(component.GameObject, isTransform, includeDisabled, tag))
                    yield return result;
            }
        }

        private static int BFSSearchComponentsChildren<T>(GameObject current, bool isTransform, IList<T> results, bool includeDisabled, string tag) where T : class
        {
            int count = 0;

            // Check for transform
            if (isTransform == true)
            {
                // Get transform
                results.Add(current.transform as T);
                count++;
            }
            else
            {
                // Search all components
                foreach (Component component in current.components)
                {
                    // Check for match
                    if (component is T match && CheckComponent(component, includeDisabled, tag) == true)
                    {
                        results.Add(match);
                        count++;
                    }
                }
            }

            // Search deeper
            foreach (Component component in current.components)
            {
                // Search inside child components
                count += BFSSearchComponentsChildren<T>(component.GameObject, isTransform, results, includeDisabled, tag);
            }
            return count;
        }

        private static T BFSSearchComponentParent<T>(GameObject current, bool isTransform, bool includeDisabled, string tag) where T : class
        {
            if(current.components != null && current.components.Count > 0)
            { 
                // Search all components
                foreach (Component component in current.components)
                {
                    // Check for match
                    if (component is T match && CheckComponent(component, includeDisabled, tag) == true)
                        return match;
                }
            }

            // Search parent
            if (current.transform.Parent != null)
            {
                // Search inside parent components
                T result = BFSSearchComponentParent<T>(current.transform.Parent.GameObject, isTransform, includeDisabled, tag);

                // Check for found
                if (result != null)
                    return result;
            }
            // Not found
            return null;
        }

        private static IEnumerable<T> BFSSearchComponentsParent<T>(GameObject current, bool isTransform, bool includeDisabled, string tag) where T : class
        {
            // Check for transform
            if (isTransform == true)
            {
                // Get transform
                yield return current.transform as T;
            }
            else
            {
                // Search all components
                foreach (Component component in current.components)
                {
                    // Check for match
                    if (component is T match && CheckComponent(component, includeDisabled, tag) == true)
                        yield return match;
                }
            }

            // Search parent
            if (current.transform.Parent != null)
            {
                // Search inside parent components
                foreach (T result in BFSSearchComponentsParent<T>(current.transform.Parent.GameObject, isTransform, includeDisabled, tag))
                    yield return result;
            }
        }

        private static int BFSSearchComponentsParent<T>(GameObject current, bool isTransform, IList<T> results, bool includeDisabled, string tag) where T : class
        {
            int count = 0;

            // Check for transform
            if(isTransform == true)
            {
                // Get transform
                results.Add(current.transform as T);
                count++;
            }
            else
            {
                // Search all components
                foreach (Component component in current.components)
                {
                    // Check for match
                    if (component is T match && CheckComponent(component, includeDisabled, tag) == true)
                    {
                        results.Add(match);
                        count++;
                    }
                }
            }

            // Search parent
            if (current.transform.Parent != null)
                count += BFSSearchComponentsParent<T>(current.transform.Parent.GameObject, isTransform, results, includeDisabled, tag);

            return count;
        }
        #endregion

        private static bool CheckComponent(Component component, bool includeDisabled, string tag)
        {
            return (includeDisabled == true || component.Enabled == true)
                && (tag == null || component.CompareTag(tag) == true);
        }
    }
}
