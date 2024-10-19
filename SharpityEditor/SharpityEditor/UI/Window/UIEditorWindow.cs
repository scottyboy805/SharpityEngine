
namespace SharpityEditor.UI.Window
{
    public abstract class UIEditorWindow
    {
        // Internal
        internal GameEditor editor = null;

        // Protected
        protected string title = "Window";

        // Properties
        public GameEditor Editor
        {
            get { return editor; }
        }

        public string Title
        {
            get { return title; }
        }

        // Methods
        public abstract void OnBuildUI(UIBuilder builder);
    }
}
