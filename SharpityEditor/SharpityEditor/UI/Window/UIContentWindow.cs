
namespace SharpityEditor.UI.Window
{
    public sealed class UIContentWindow : UIEditorWindow
    {
        // Constructor
        public UIContentWindow()
        {
            title = "Content";
        }

        // Methods
        public override void OnBuildUI(UIBuilder builder)
        {
            // Create split
            (UIBuilder left, UIBuilder right) = builder.SplitLayoutView();

            // Build main UI
            OnBuildHierarchyUI(left);
            OnBuildContentUI(right);
        }

        private void OnBuildHierarchyUI(UIBuilder builder)
        {
            builder.Label("Content tree here");
        }

        private void OnBuildContentUI(UIBuilder builder)
        {
            // Content top bar
            UIBuilder topBuilder = builder.LayoutView(UIOrientation.Horizontal);
            {
                topBuilder.Button("Path");
                topBuilder.Button("next");
            }



            builder.Label("Content section here");
        }
    }
}
