using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media.Imaging;
using JetBrains.Annotations;

namespace bajanetLauncher {
    public class MainWindowModel : INotifyPropertyChanged {

        private string _appdata_AppName;
        private string _appdata_AppVersion;
        private string _appdata_AppDescription;
        private Bitmap _appdata_Icon;
        private string _appdata_AppSystems;
        private StoreAppDBViewModel _applist;
        private int _selectedApp;
        private string _connectionStatus;
        
        private Bitmap _bajaLogo;

        private string _welcomeMessage = "Welcome to bajanet! It looks like you are offline, but if you have apps installed you still should be able to view them!";

        public Bitmap BajaLogo {
            get => _bajaLogo;
            set {
                if (value != _bajaLogo) {
                    _bajaLogo = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Appdata_AppName {
            get => _appdata_AppName;
            set {
                if (value != _appdata_AppName) {
                    _appdata_AppName = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string WelcomeMessage {
            get => _welcomeMessage;
            set {
                if (value != _welcomeMessage) {
                    _welcomeMessage = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string Appdata_AppDescription {
            get => _appdata_AppDescription;
            set {
                if (value != _appdata_AppDescription) {
                    _appdata_AppDescription = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string Appdata_AppVersion {
            get => _appdata_AppVersion;
            set {
                if (value != _appdata_AppName) {
                    _appdata_AppVersion = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ConnectionStatus {
            get => _connectionStatus;
            set {
                if (value != _connectionStatus) {
                    _connectionStatus = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public string Appdata_AppSystems {
            get => _appdata_AppSystems;
            set {
                if (value != _appdata_AppSystems) {
                    _appdata_AppSystems = value;
                    OnPropertyChanged();
                }
            }
        }

        public Bitmap Appdata_Icon {
            get => _appdata_Icon;
            set {
                if (value != _appdata_Icon) {
                    _appdata_Icon = value;
                    OnPropertyChanged();
                }
            }
        }

        public StoreAppDBViewModel Applist {
            get => _applist;
            set {
                if (value != _applist) {
                    _applist = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedApp {
            get => _selectedApp;
            set {
                if (value != _selectedApp) {
                    _selectedApp = value;
                    OnPropertyChanged();
                }
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}