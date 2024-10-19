using SharpityEngine.Graphics;

namespace SharpityEditor.UI.Property
{
    public sealed class UIVector3Drawer : UIPropertyDrawer
    {
        // Private
        private UIBind<float> xBind = 0f;
        private UIBind<float> yBind = 0f;
        private UIBind<float> zBind = 0f;

        // Constructor
        public UIVector3Drawer()
        {
            xBind.OnChanged.AddListener(OnXChanged);
            yBind.OnChanged.AddListener(OnYChanged);
            zBind.OnChanged.AddListener(OnZChanged);
        }

        // Methods
        public override void OnBuildUI(UIBuilder builder)
        {
            // Create group
            UIBuilder horizontal = builder.LayoutView(UIOrientation.Horizontal);

            // Build label
            horizontal.Label("Vector3");


            // X
            horizontal.Label("X");
            horizontal.NumberField(xBind, float.MinValue, float.MaxValue);

            // Y
            horizontal.Label("Y");
            horizontal.NumberField(yBind, float.MinValue, float.MaxValue);

            // Z
            horizontal.Label("Z");
            horizontal.NumberField(zBind, float.MinValue, float.MaxValue);
        }

        private void OnXChanged(float value)
        {

        }

        private void OnYChanged(float value)
        {

        }

        private void OnZChanged(float value)
        {

        }
    }
}
