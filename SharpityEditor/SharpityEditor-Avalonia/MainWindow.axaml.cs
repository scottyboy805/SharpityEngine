using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using NP.Ava.UniDock;
using NP.Ava.UniDockService;
using SharpityEditor;
using SharpityEditor.UI;
using SharpityEditor.UI.Window;
using System;
using System.Collections.ObjectModel;

namespace NP.Demos.DefaultLayoutSaveDemo
{
    public partial class MainWindow : Window
    {
        // Private
        private GameEditor editor = null;
        private IUniDockService dockManager;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            dockManager = (DockManager) this.FindResource("TheDockManager")!;
            

            dockManager.DockItemsViewModels = new ObservableCollection<DockItemViewModelBase>();


            // Create editor
            editor = new GameEditor();


            // Hierarchy
            dockManager.DockItemsViewModels.Add(new AvaloniaUIEditorWindowHost(
                new UIHierarchyWindow(), editor, "Main", 1));

            // Game
            dockManager.DockItemsViewModels.Add(new AvaloniaUIEditorWindowHost(
                new UIGameWindow(), editor, "Main", 2));

            // Inspector
            dockManager.DockItemsViewModels.Add(new AvaloniaUIEditorWindowHost(
                new UIInspectorWindow(), editor, "Main", 3));

            // Content
            dockManager.DockItemsViewModels.Add(new AvaloniaUIEditorWindowHost(
                new UIContentWindow(), editor, "Bottom", 4));

        }

        private void _saveLayoutButton_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            dockManager.SaveToFile("DefaultLayout.xml");
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            editor.Dispose();
        }
    }
}
