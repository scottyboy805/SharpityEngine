using Avalonia.Controls;
using NP.Ava.UniDockService;
using SharpityEditor.UI.Window;

namespace SharpityEditor.UI
{
    internal sealed class AvaloniaUIEditorWindowHost : DockItemViewModelBase
    {
        // Private
        private UIEditorWindow window = null;

        // Constructor
        public AvaloniaUIEditorWindowHost(UIEditorWindow window, GameEditor editor, string dockID, int id)
        {
            this.window = window;
            this.window.editor = editor;

            DockId = window.Title;
            Header = window.Title;
            DefaultDockGroupId = dockID;
            DefaultDockOrderInGroup = id;
            IsSelected = true;
            IsActive = true;
            
            
            // Build the window
            Build();
        }

        // Methods
        public void Build()
        {
            // Create root control
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Avalonia.Layout.Orientation.Vertical;
            base.Content = stackPanel;

            // Create builder
            AvaloniaUIBuilder builder = new AvaloniaUIBuilder(stackPanel);

            // Build window
            window.OnBuildUI(builder);
        }

    }
}
