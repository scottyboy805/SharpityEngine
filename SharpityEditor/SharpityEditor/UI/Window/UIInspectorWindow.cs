
using SharpityEditor.UI.Drawer;
using SharpityEditor.UI.Property;
using SharpityEngine.Graphics;

namespace SharpityEditor.UI.Window
{
    public sealed class UIInspectorWindow : UIEditorWindow
    {
        UIBindCollectionEnum<TextureFormat> a = TextureFormat.RG16Float;

        // Constructor
        public UIInspectorWindow()
        {
            title = "Inspector";
        }

        // Methods
        public override void OnBuildUI(UIBuilder builder)
        {
            new UIVector3Drawer().OnBuildUI(builder);
            builder.Button("Hello World");
            builder.Label("Test label");
        }
    }
}
