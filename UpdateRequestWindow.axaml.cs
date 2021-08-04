using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace bajanetLauncher {
    public class UpdateRequestWindow : Window {
        public UpdateRequestWindow() {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }
    }
}