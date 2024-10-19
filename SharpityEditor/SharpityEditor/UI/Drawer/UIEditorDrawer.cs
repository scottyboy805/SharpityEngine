using SharpityEditor.Content;

namespace SharpityEditor.UI.Drawer
{
    public abstract class UIEditorDrawer
    {
        // Internal
        internal GameEditor editor = null;
        internal SerializedElement element = null;

        // Properties
        public GameEditor Editor
        {
            get { return editor; }
        }

        public SerializedElement Element
        {
            get { return element; }
        }

        // Methods
        public abstract void OnBuildUI(UIBuilder builder);
    }
}
