
using SharpityEngine;

namespace SharpityEditor.UI
{
    public enum UIOrientation
    {
        Horizontal,
        Vertical
    }

    public abstract class UIBuilder
    {
        // Methods
        public abstract void Label(UIBind<string> label = null);

        public abstract void Button(UIBind<string> label = null, UIBindCommand clicked = null);

        public abstract void ToggleButton(UIBind<string> label = null, UIBind<bool> selected = null);

        public abstract void Toggle(UIBind<bool> selected = null);

        public abstract void TextField(UIBind<string> text = null);

        public abstract void TextArea(UIBind<string> text = null);

        public abstract void NumberField(UIBind<int> value, int min = int.MinValue, int max = int.MaxValue, int increment = 0);

        public abstract void NumberField(UIBind<float> value, float min = float.MinValue, float max = float.MaxValue, float increment = 0f);

        public abstract void Slider(UIBind<int> value, int min, int max);

        public abstract void Slider(UIBind<float> value, float min, float max);

        public abstract void DropDown(UIBindCollection<string> options);

        public abstract void DropDown<T>(UIBindCollectionEnum<T> options) where T : struct, Enum;

        public abstract void List(UIBindCollection<string> options);

        public abstract void Tree();

        #region Layout
        public abstract UIBuilder LayoutView(UIOrientation orientation);

        public abstract UIBuilder FlowLayoutView(UIOrientation orientation);

        public abstract (UIBuilder, UIBuilder) SplitLayoutView();

        public abstract UIBuilder ScrollLayoutView(UIOrientation orientation, UIBind<Vector2> scroll = null);
        #endregion
    }
}
