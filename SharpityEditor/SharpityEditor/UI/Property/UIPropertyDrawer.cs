using SharpityEditor.Content;

namespace SharpityEditor.UI.Property
{
    public abstract class UIPropertyDrawer
    {
        // Internal
        internal GameEditor editor = null;
        internal SerializedProperty property = null;

        // Properties
        public GameEditor Editor
        {
            get { return editor; }
        }

        public SerializedProperty Property
        {
            get { return property; }
        }

        // Methods
        public abstract void OnBuildUI(UIBuilder builder);
    }
}
