using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;

namespace bajanetLauncher {
    public class StoreAppDBView : UserControl {

        public StoreAppDBView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        
        

        // this part is a total mess. I have no idea why, but the ItemsPresenter just doesn't exist sometimes, so if we want to reference it's elements we need to store it somewhere.
        
        private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
            
            var itemParent = sender as Border;
            var window = ((MainWindow)itemParent.GetVisualRoot());
            window.LoadAppToContext(itemParent.DataContext as StoreApp);
            window.listPresenter = itemParent.GetVisualParent().VisualParent.VisualParent as ItemsPresenter;
        }

        public void SelectionVisual(int selected, ItemsPresenter presenter) { // Highlight the selected item
            foreach (var i in presenter.ItemContainerGenerator.Containers) {
                var cp = i.ContainerControl as ContentPresenter;
                
                cp.Child.Classes = Classes.Parse("NotSelected");
                if(i.Index == selected)
                    cp.Child.Classes = Classes.Parse("Selected");
            }
            
            this.InvalidateVisual();
            this.InvalidateStyles();
        }

        private void StyledElement_OnInitialized(object? sender, EventArgs e) {
            var itemParent = sender as Border;
            var window = ((MainWindow)itemParent.GetVisualRoot());
            window.listPresenter = itemParent.GetVisualParent().VisualParent.VisualParent as ItemsPresenter;
        }
    }
}