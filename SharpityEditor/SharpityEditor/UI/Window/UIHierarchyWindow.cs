
namespace SharpityEditor.UI.Window
{
    public sealed class UIHierarchyWindow : UIEditorWindow
    {
        // Constructor
        public UIHierarchyWindow()
        {
            title = "Hierarchy";
        }

        // Methods
        public override void OnBuildUI(UIBuilder builder)
        {
            builder.Button("Hello World");
            builder.Label("Test label");
        }
    }
}
