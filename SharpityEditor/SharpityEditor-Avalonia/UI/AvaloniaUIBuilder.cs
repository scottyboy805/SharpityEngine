using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using NP.Utilities;
using SharpityEngine;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;

namespace SharpityEditor.UI
{
    internal sealed class AvaloniaUIBuilder : UIBuilder
    {
        // Private
        private Panel root = null;

        // Constructor
        public AvaloniaUIBuilder(Panel root)
        {
            this.root = root;
        }

        // Methods
        public override void Label(UIBind<string> label = null)
        {
            // Create label
            Label text = new Label();
            
            root.Children.Add(text);

            // Bind label
            if (label != null)
            {
                // Set start value
                text.Content = label.Value;

                // Listen for user changed value
                label.OnUserChanged.AddListener(() => text.Content = label.Value);
            }
        }

        public override void Button(UIBind<string> label = null, UIBindCommand clicked = null)
        {            
            // Create button
            Button button = new Button();
            root.Children.Add(button);

            // Bind label
            if(label != null)
            {
                // Set start value
                button.Content = label.Value;

                // Listen for user changed value
                label.OnUserChanged.AddListener(() => button.Content = label.Value);
            }

            // Bind event
            if(clicked != null)
                button.Click += (_, _) => clicked();
        }

        public override void ToggleButton(UIBind<string> label = null, UIBind<bool> selected = null)
        {
            // Create button
            ToggleButton button = new ToggleButton();
            root.Children.Add(button);

            // Bind label
            if(label != null)
            {
                // Set start value
                button.Content = label.Value;

                // Listen for user changed value
                label.OnUserChanged.AddListener(() => button.Content = label.Value);
            }

            // Bind event
            if (selected != null)
                button.IsCheckedChanged += (_, _) => selected.Value = button.IsChecked.Value;
        }

        public override void Toggle(UIBind<bool> selected = null)
        {
            // Create toggle
            CheckBox toggle = new CheckBox();
            root.Children.Add(toggle);

            // Bind selected
            if (selected != null)
            {
                // Set start value
                toggle.IsChecked = selected.Value;

                // Listen for user changed value
                selected.OnUserChanged.AddListener(() => toggle.IsChecked = selected.Value);

                // Listen for UI changed value
                toggle.IsCheckedChanged += (_, _) => selected.UIChangedValue(toggle.IsChecked.Value);
            }
        }

        public override void TextField(UIBind<string> text = null)
        {
            // Create field
            TextBox textBox = new TextBox();
            root.Children.Add(textBox);

            // Bind text
            if(text != null)
            {
                // Set start value
                textBox.Text = text.Value;

                // Listen for user changed value
                text.OnUserChanged.AddListener(() => textBox.Text = text.Value);

                // Listen for UI changed value
                textBox.TextChanged += (_, _) => text.UIChangedValue(textBox.Text);
            }
        }

        public override void TextArea(UIBind<string> text = null)
        {
            // Create field
            TextBox textBox = new TextBox();
            textBox.AcceptsReturn = true;
            textBox.TextWrapping = Avalonia.Media.TextWrapping.Wrap;
            root.Children.Add(textBox);

            // Bind text
            if (text != null)
            {
                // Set start value
                textBox.Text = text.Value;

                // Listen for user changed value
                text.OnUserChanged.AddListener(() => textBox.Text = text.Value);

                // Listen for UI changed value
                textBox.TextChanged += (_, _) => text.UIChangedValue(textBox.Text);
            }
        }

        public override void NumberField(UIBind<int> value, int min = int.MinValue, int max = int.MaxValue, int increment = 0)
        {
            // Check for increment
            if (increment == 0)
            {
                // Create field
                TextBox textBox = new TextBox();
                root.Children.Add(textBox);

                // Bind value
                if (value != null)
                {
                    // Set start value
                    textBox.Text = value.Value.ToString();

                    // Listen for user changed value
                    value.OnUserChanged.AddListener(() => textBox.Text = value.Value.ToString());

                    // Listen for UI changed value
                    textBox.TextChanged += (_, _) => value.UIChangedValue(int.Parse(textBox.Text));
                }

                // TODO - Add validation for numbers
            }
            else
            {
                // Create numeric
                NumericUpDown numeric = new NumericUpDown();
                numeric.Minimum = min;
                numeric.Maximum = max;
                numeric.Increment = increment;
                root.Children.Add(numeric);

                // Bind value
                if(value != null)
                {
                    // Set start value
                    numeric.Value = value.Value;

                    // Listen for user changed value
                    value.OnUserChanged.AddListener(() => numeric.Value = value.Value);

                    // Listen for UI changed value
                    numeric.ValueChanged += (_, _) => value.UIChangedValue((int)numeric.Value);
                }
            }
        }

        public override void NumberField(UIBind<float> value, float min = float.MinValue, float max = float.MaxValue, float increment = 0)
        {
            // Check for increment
            if (increment == 0)
            {
                // Create field
                TextBox textBox = new TextBox();
                root.Children.Add(textBox);

                // Bind value
                if (value != null)
                {
                    // Set start value
                    textBox.Text = value.Value.ToString();

                    // Listen for user changed value
                    value.OnUserChanged.AddListener(() => textBox.Text = value.Value.ToString());

                    // Listen for UI changed value
                    textBox.TextChanged += (_, _) => value.UIChangedValue(float.Parse(textBox.Text));
                }

                // TODO - Add validation for numbers
            }
            else
            {
                // Create numeric
                NumericUpDown numeric = new NumericUpDown();
                numeric.Minimum = (decimal)min;
                numeric.Maximum = (decimal)max;
                numeric.Increment = (decimal)increment;
                root.Children.Add(numeric);

                // Bind value
                if (value != null)
                {
                    // Set start value
                    numeric.Value = (decimal)value.Value;

                    // Listen for user changed value
                    value.OnUserChanged.AddListener(() => numeric.Value = (decimal)value.Value);

                    // Listen for UI changed value
                    numeric.ValueChanged += (_, _) => value.UIChangedValue((float)numeric.Value);
                }
            }
        }

        public override void Slider(UIBind<int> value, int min, int max)
        {
            // Create slider
            Slider slider = new Slider();
            slider.Minimum = min;
            slider.Maximum = max;
            root.Children.Add(slider);

            // Bind value
            if(value != null)
            {
                // Set start value
                slider.Value = value.Value;

                // Listen for user changed value
                value.OnUserChanged.AddListener(() =>  slider.Value = value.Value);

                // Listen for UI changed value
                slider.ValueChanged += (_, _) => value.UIChangedValue((int)slider.Value);
            }
        }

        public override void Slider(UIBind<float> value, float min, float max)
        {
            // Create slider
            Slider slider = new Slider();
            slider.Minimum = min;
            slider.Maximum = max;
            root.Children.Add(slider);

            // Bind value
            if (value != null)
            {
                // Set start value
                slider.Value = value.Value;

                // Listen for user changed value
                value.OnUserChanged.AddListener(() => slider.Value = value.Value);

                // Listen for UI changed value
                slider.ValueChanged += (_, _) => value.UIChangedValue((float)slider.Value);
            }
        }

        public override void DropDown(UIBindCollection<string> options)
        {
            // Create drop down
            ComboBox dropDown = new ComboBox();
            root.Children.Add(dropDown);

            // Refresh items
            Action refreshItems = () =>
            {
                dropDown.Items.Clear();
                foreach (string option in options.Values)
                    dropDown.Items.Add(option);
            };

            // Bind value
            if(options != null)
            {
                // Set start values
                refreshItems();
                dropDown.SelectedIndex = options.Selected;

                // Listen for user changed value
                options.OnUserChanged.AddListener(() => dropDown.SelectedIndex = options.Selected);

                // Listen for UI changed value
                dropDown.SelectionChanged += (_, _) => options.UIChangedValue(dropDown.SelectedIndex);
            }
        }

        public override void DropDown<T>(UIBindCollectionEnum<T> options)
        {
            throw new NotImplementedException();
        }

        public override void List(UIBindCollection<string> options)
        {
            // Create list box
            ListBox listBox = new ListBox();
            root.Children.Add(listBox);

            // Refresh items
            Action refreshItems = () =>
            {
                listBox.Items.Clear();
                foreach (string option in options.Values)
                    listBox.Items.Add(option);
            };

            // Bind value
            if (options != null)
            {
                // Set start values
                refreshItems();
                listBox.SelectedIndex = options.Selected;

                // Listen for user changed value
                options.OnUserChanged.AddListener(() => listBox.SelectedIndex = options.Selected);

                // Listen for UI changed value
                listBox.SelectionChanged += (_, _) => options.UIChangedValue(listBox.SelectedIndex);
            }
        }

        public override void Tree()
        {
            // Create tree view
            TreeView treeView = new TreeView();
            
            root.Children.Add(treeView);
        }

        public override UIBuilder LayoutView(UIOrientation orientation)
        {
            // Create stack layout
            StackPanel panel = new StackPanel();
            panel.Orientation = orientation == UIOrientation.Horizontal
                ? Avalonia.Layout.Orientation.Horizontal
                : Avalonia.Layout.Orientation.Vertical;
            root.Children.Add(panel);

            // Create Avalonia builder
            return new AvaloniaUIBuilder(panel);
        }

        public override UIBuilder FlowLayoutView(UIOrientation orientation)
        {
            // Create wrap panel
            WrapPanel wrap = new WrapPanel();
            wrap.Orientation = orientation == UIOrientation.Horizontal
                ? Avalonia.Layout.Orientation.Horizontal
                : Avalonia.Layout.Orientation.Vertical;
            root.Children.Add(wrap);

            // Create Avalonia builder
            return new AvaloniaUIBuilder(wrap);
        }

        public override (UIBuilder, UIBuilder) SplitLayoutView()
        {
            // Create split
            SplitView splitView = new SplitView();
            root.Children.Add(splitView);

            // Create left
            StackPanel leftPanel = new StackPanel();
            leftPanel.Orientation = Avalonia.Layout.Orientation.Vertical;
            splitView.Pane = leftPanel;

            // Create right
            StackPanel rightPanel = new StackPanel();
            rightPanel.Orientation = Avalonia.Layout.Orientation.Vertical;
            splitView.Content = rightPanel;

            // Get builders
            return (new AvaloniaUIBuilder(leftPanel), new AvaloniaUIBuilder(rightPanel));
        }

        public override UIBuilder ScrollLayoutView(UIOrientation orientation, UIBind<Vector2> scroll = null)
        {
            // Create scroll
            ScrollViewer scrollView = new ScrollViewer();
            root.Children.Add(scrollView);

            //ScrollBar bar;
            //bar.Value
            //// Bind scroll
            //if(scroll != null)
            //{
            //    // Set start value
            //    scrollView
            //}

            // Create panel
            StackPanel scrollPanel = new StackPanel();
            scrollPanel.Orientation = orientation == UIOrientation.Horizontal
                ? Avalonia.Layout.Orientation.Horizontal
                : Avalonia.Layout.Orientation.Vertical;
            scrollView.Content = scrollPanel;

            // Create Avalonia builder
            return new AvaloniaUIBuilder(scrollPanel);
        }
    }
}
