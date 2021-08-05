using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace bajanetLauncher {
    public class UpdateRequestWindow : Window {
        private MainWindow mainWindow;

        public UpdateRequestWindow() {
            
        }
        public UpdateRequestWindow(MainWindow mainWindow) {
            InitializeComponent();
            this.Width = 300;
            this.mainWindow = mainWindow;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

        private void Nah_OnClick(object? sender, RoutedEventArgs e) {
            this.Close();
            
        }

        private void Ok_OnClick(object? sender, RoutedEventArgs e) {
            if(File.Exists("C:/tmp/bajasoftinstaller.exe")) File.Delete("C:/tmp/bajasoftinstaller.exe");
            File.Copy("Assets/bajasoftinstaller.exe", "C:/tmp/bajasoftinstaller.exe");
            Process.Start("C:/tmp/bajasoftinstaller.exe","--silent");
            mainWindow.Close();
            this.Close();
            
        }
    }
}