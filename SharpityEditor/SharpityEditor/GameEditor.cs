using SharpityEditor.Content;
using SharpityEngine.Content;
using SharpityEngine;
using System.Reflection;
using System.Runtime.CompilerServices;
using SharpityEditor.UI.Property;
using SharpityEditor.UI.Drawer;

[assembly:InternalsVisibleTo("SharpityEditor-Avalonia")]
[assembly:InternalsVisibleTo("SharpityEditor-Console")]

namespace SharpityEditor
{
    public sealed class GameEditor : IDisposable
    {
        // Private
        private string projectPath = null;
        private string projectFolder = null;
        private ContentDatabase contentDatabase = null;

        private Dictionary<Type, Type> propertyDrawers = new Dictionary<Type, Type>();
        private Dictionary<Type, Type> editorDrawers = new Dictionary<Type, Type>();

        // Properties
        public string ProjectPath
        {
            get { return projectPath; }
        }

        public string ProjectFolder
        {
            get { return projectFolder; }
        }

        public bool IsProjectOpen
        {
            get { return projectPath != null; }
        }

        public ContentDatabase ContentDatabase
        {
            get { return contentDatabase; }
        }

        // Constructor
        internal GameEditor()
        {
            LoadDrawers();
        }

        // Methods
        public void Dispose()
        {
            contentDatabase.Dispose();
        }

        public void OpenProject(string projectPath)
        {
            // Check for invalid
            if (string.IsNullOrEmpty(projectPath) == true)
                throw new ArgumentException("Project path cannot be null or empty");

            // Check for found
            if (File.Exists(projectPath) == false)
                throw new ArgumentException("Project file path does not exist");

            // Initialize project
            this.projectPath = projectPath;
            this.projectFolder = Directory.GetParent(projectPath).FullName;
            this.contentDatabase = new ContentDatabase(projectFolder);

            // Start loading assets
            contentDatabase.SyncContentOnDisk();
        }

        public UIPropertyDrawer CreatePropertyDrawer(Type forType, SerializedProperty property)
        {
            // Check for null
            if(forType == null)
                throw new ArgumentNullException(nameof(forType));

            if (property == null)
                throw new ArgumentNullException(nameof(property));

            // Get drawer type
            Type drawerType;
            if (propertyDrawers.TryGetValue(forType, out drawerType) == false)
                return null;

            // Create instance
            UIPropertyDrawer drawer = contentDatabase.TypeManager.CreateTypeInstanceAs<UIPropertyDrawer>(drawerType);

            // Initialize drawer
            drawer.editor = this;
            drawer.property = property;

            return drawer;
        }

        public UIEditorDrawer CreateEditorDrawer(Type forType, SerializedElement element)
        {
            // Check for null
            if (forType == null)
                throw new ArgumentNullException(nameof(forType));

            if (element == null)
                throw new ArgumentNullException(nameof(element));

            // Get drawer type
            Type drawerType;
            if (editorDrawers.TryGetValue(forType, out drawerType) == false)
                return null;

            // Create instance
            UIEditorDrawer drawer = contentDatabase.TypeManager.CreateTypeInstanceAs<UIEditorDrawer>(drawerType);

            // Initialize drawer
            drawer.editor = this;
            drawer.element = element;

            return drawer;
        }

        private void LoadDrawers()
        {
            // Get this assembly name
            AssemblyName thisAssembly = Assembly.GetExecutingAssembly().GetName();

            try
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // Check if we are scanning an external assembly
                    if (assembly != Assembly.GetExecutingAssembly())
                    {
                        // Check if the assembly references sharpity
                        AssemblyName[] referenceNames = assembly.GetReferencedAssemblies();
                        bool referenced = false;

                        foreach (AssemblyName assemblyName in referenceNames)
                        {
                            if (thisAssembly.FullName == assemblyName.FullName)
                            {
                                referenced = true;
                                break;
                            }
                        }

                        // Check for referenced
                        if (referenced == false)
                            continue;
                    }

                    foreach (Type type in assembly.GetTypes())
                    {
                        // Property drawers
                        foreach (PropertyDrawerAttribute attrib in type.GetCustomAttributes<PropertyDrawerAttribute>())
                        {
                            // Check for derived type
                            if (typeof(UIPropertyDrawer).IsAssignableFrom(type) == false)
                            {
                                Debug.LogError(LogFilter.Content, "Property drawer must derive from `UIPropertyDrawer`: " + type);
                                continue;
                            }

                            // Check for overwrite content reader
                            if (propertyDrawers.ContainsKey(attrib.DrawerType) == true)
                            {
                                Debug.LogErrorF(LogFilter.Content, "A property drawer already exists for type `{0}`: {1}", type, attrib.DrawerType);
                                continue;
                            }

                            // Store reader type
                            propertyDrawers[attrib.DrawerType] = type;
                        }

                        // Editor drawers
                        foreach (EditorDrawerAttribute attrib in type.GetCustomAttributes<EditorDrawerAttribute>())
                        {
                            // Check for derived type
                            if (typeof(UIPropertyDrawer).IsAssignableFrom(type) == false)
                            {
                                Debug.LogError(LogFilter.Content, "Editor drawer must derive from `UIEditorDrawer`: " + type);
                                continue;
                            }

                            // Check for overwrite content reader
                            if (editorDrawers.ContainsKey(attrib.DrawerType) == true)
                            {
                                Debug.LogErrorF(LogFilter.Content, "An editor drawer already exists for type `{0}`: {1}", type, attrib.DrawerType);
                                continue;
                            }

                            // Store reader type
                            editorDrawers[attrib.DrawerType] = type;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}