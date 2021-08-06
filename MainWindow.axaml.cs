using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using Newtonsoft.Json;

namespace bajanetLauncher {
    public partial class MainWindow : Window {
        private MainWindowModel data;
        public static string appData;
        public static string appFolder;
        private static string localDb;

        private Button install, uninstall;
        private ProgressBar installing;

        private StoreAppDB localApps;
        
        public ItemsPresenter listPresenter;

        private bool isBusy = false;
        

        private bool isOnline = false;
#if DEBUG
        public static string apiUrl = "http://127.0.0.1/bajanet/listapps.php";
        public static string launcherApi = "http://127.0.0.1/bajanet/launcher.php";
#else
        
        public static string apiUrl = "http://mc.bajtix.xyz/bajanet/listapps.php";
        public static string launcherApi = "http://mc.bajtix.xyz/bajanet/launcher.php";
    #endif

        public const string currentLauncherVersion = "v0.75f-patch1";

        public MainWindow() {
            data = new MainWindowModel();
            this.DataContext = data;

            DoConfigs();
            
            InitializeComponent();
            
            
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void DoConfigs() {
            appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "bajtix",
                "bajanet");
            Directory.CreateDirectory(appData);
            localDb = Path.Combine(appData, "localdb.json");
            if(!File.Exists(localDb))
                File.WriteAllText(localDb,"{}");

            appFolder = Path.Combine(appData, "apps");

            data.BajaLogo = new Bitmap("Assets/bajaLogo.png");

            Directory.CreateDirectory(appFolder);
        }

        public static string GetOS() {
            if (OperatingSystem.IsLinux()) return "Linux";
            if (OperatingSystem.IsMacOS()) return "Mac"; 
            if (OperatingSystem.IsAndroid()) return "Android";
            return "Windows";
        }
        
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);

            install = this.Find<Button>("Install");
            uninstall = this.Find<Button>("Uninstall");
            installing = this.Find<ProgressBar>("Installing");
            ClientSizeProperty.Changed.Subscribe(args => WindowResized());
        }

        private void LoadDb(bool runSync = false) {
            string launcherVersion = currentLauncherVersion, dbResponse, localDbResponse = File.ReadAllText(localDb);
            dbResponse = localDbResponse;
            data.ConnectionStatus = "Loading";
            Task dbSync = new Task(() => {
    #if DEBUG
                Thread.Sleep(1000);
#endif
                try {
                    dbResponse = FetchDb(apiUrl);
                    launcherVersion = GetLauncherInfo(FetchDb(launcherApi));
                }
                catch (Exception e) {
                    dbResponse = localDbResponse; // pointless
                    launcherVersion = currentLauncherVersion;
                    data.WelcomeMessage = $"## Welcome to bajanet! \nIt looks like there were problems loading apps: `{e.Message}`, but if you have apps installed you still should be able to view them! (If the Connection Status says we are online, please report this bug)";
                }
                
            });
            if (!runSync) {
                dbSync.ContinueWith(task => {
                    if (launcherVersion != currentLauncherVersion) {
                        AskToUpdate();
                    }


                    localApps = new StoreAppDB(localDbResponse);
                    var apps = new StoreAppDB(dbResponse);
                    data.Applist = new StoreAppDBViewModel(apps.GetItems());

                    OnlineCheck(); // kinda wasting bandwidth
                }, TaskScheduler.FromCurrentSynchronizationContext());

                dbSync.Start();
            }
            else { // copied method. couldn't really figure out how to run the task correctly
                try {
                    dbResponse = FetchDb(apiUrl);
                    launcherVersion = GetLauncherInfo(FetchDb(launcherApi));
                }
                catch (Exception e) {
                    dbResponse = localDbResponse; // pointless
                    launcherVersion = currentLauncherVersion;
                    data.WelcomeMessage = $"## Welcome to bajanet! \nIt looks like there were problems loading apps: `{e.Message}`, but if you have apps installed you still should be able to view them! (If the Connection Status says we are online, please report this bug)";
                }


                localApps = new StoreAppDB(localDbResponse);
                var apps = new StoreAppDB(dbResponse);
                data.Applist = new StoreAppDBViewModel(apps.GetItems());

                OnlineCheck(); // kinda wasting bandwidth
            }
        }

        

        private string GetLauncherInfo(string json) {
            dynamic info = JsonConvert.DeserializeObject(json);
            data.WelcomeMessage = info.welcomescreen;
            return info.version;
        }

        private void AskToUpdate() {
#if !DEBUG
            var updateDialogue = new UpdateRequestWindow(this);
            
            updateDialogue.Focus();
            updateDialogue.Show();
#endif
        }

        private string FetchDb(string uri) {
            string result = "";
            using WebClient wc = new();
            result = wc.DownloadString(uri);

            return result;
        }
        
        public void RunClicked(object sender, RoutedEventArgs e) {
            var app = data.Applist.Items[data.SelectedApp];
            if (!IsInstalled(app))
                Install(app);
            else if (!IsUpToDate(app))
                Update(app);
            else
                Launch(app);

        }

        public void RemoveClicked(object sender, RoutedEventArgs e) {
            var app = data.Applist.Items[data.SelectedApp];
            if(IsInstalled(app))
                Uninstall(app);
            else {
                LoadAppToContext(app);
            }
        }

        private bool OnlineCheck() {
            try {
                FetchDb(apiUrl);
                isOnline = true;
            }
            catch (WebException we) {
                isOnline = false;
            }

            string status = isOnline ? "online" : "offline";
            data.ConnectionStatus = $"Connection status: {status}. {data.Applist.Items.Count} apps available.";
            Title = $"bajanet [{status}]";
            
            return isOnline;
        }

        private void Update(StoreApp app) {
            app = GetLocal(app);
            Uninstall(app);
            Task waiting = new Task(() => {
                while (isBusy) Thread.Sleep(100);
            });
            waiting.ContinueWith(task => {
                Install(GetGlobal(app));
            }, TaskScheduler.FromCurrentSynchronizationContext());
            waiting.Start();
        }

        private void Launch(StoreApp app) {
            //TODO: make cross platform
            app = GetLocal(app);
            string execname = "start.exe";
            if (File.Exists(Path.Combine(app.BuildUrl, "package.ini"))) {
                IniFile f = new IniFile(File.ReadAllText(Path.Combine(app.BuildUrl, "package.ini")));
                execname = f.Get("Exec", "Bajanet");
            }
            Process.Start(Path.Combine(app.BuildUrl, execname));
            Close();
        }

        public void Uninstall(StoreApp app) {
            app = GetLocal(app);
            
            installing.IsVisible = true;
            install.IsVisible = false;
            
            uninstall.IsEnabled = false;
            string folder = app.BuildUrl;
            
            isBusy = true;
            this.Find<StoreAppDBView>("AppListDisplay").IsEnabled = false;
            
            Task delete = new Task(() => {
                Directory.Delete(folder,true);
                localApps.apps.Remove(app);
                SaveDb();
                LoadDb(true); // already in task, can cause issues when not ran in sync
            });
            delete.ContinueWith(task => {
                
                installing.IsVisible = false;
                install.IsVisible = true;
                
                LoadAppToContext(app);
                
                isBusy = false;
                this.Find<StoreAppDBView>("AppListDisplay").IsEnabled = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
            
            delete.Start();
            
        }
        
        public void LoadAppToContext(StoreApp app) {
            
            
            app = GetGlobal(app);
            var list = this.Find<StoreAppDBView>("AppListDisplay") as StoreAppDBView;
            if (app == null) {
                this.Find<Panel>("WelcomeNote").IsVisible = true;
                this.Find<Border>("AppDetails").IsVisible = false;
                data.SelectedApp = -1;
                list.SelectionVisual(data.SelectedApp, listPresenter);
                return;
            }

            
            
            
            
            this.Find<Panel>("WelcomeNote").IsVisible = false;
            this.Find<Border>("AppDetails").IsVisible = true;
            data.Appdata_Icon = app.AppIcon;
            data.Appdata_AppName = app.Name;
            data.Appdata_AppVersion = $"{app.Version} [{app.VersionDate.ToString()}]";
            data.Appdata_AppDescription = app.Description;
            data.Appdata_AppChangelog = "# Changelog \n" + app.Changelog;
            data.SelectedApp = data.Applist.Items.IndexOf(GetGlobal(app));
            
            data.Appdata_AppSystems = "[Windows]";
            install.IsEnabled = app.BuildUrl != String.Empty;
            install.Content = RunButtonText(app);
            uninstall.IsEnabled = IsInstalled(app);
            
            list.SelectionVisual(data.SelectedApp, listPresenter);
        }

        private string RunButtonText(StoreApp app) {
            if (IsInstalled(app)) {
                if (!IsUpToDate(app)) {
                    return "Update";
                }
                else {
                    return "Launch";
                }
            }
            else {
                return "Install";
            }

        }
        
        

        private void Install(StoreApp app) {
            installing.IsVisible = true;
            install.IsVisible = false;
            isBusy = true;
            this.Find<StoreAppDBView>("AppListDisplay").IsEnabled = false;
            
            WebClient client = new();
            string downloadFolderPath = Path.Combine(appFolder, app.Id.ToString(), app.Id + ".zip");
            Directory.CreateDirectory(Path.GetDirectoryName(downloadFolderPath)!);
            client.DownloadProgressChanged += (sender, args) => {
                installing.IsIndeterminate = false;
                installing.Value = args.ProgressPercentage;
            };
            
            client.DownloadFileCompleted += (sender, args) => {
                if (args.Error != null) {
                    OnDownloadFinished(app);
                    return;
                }
                installing.IsIndeterminate = true;
                installing.Value = 0;

                Task unzip = new Task(() => {
                   ZipFile.ExtractToDirectory(downloadFolderPath, Path.GetDirectoryName(downloadFolderPath));

                });
                unzip.Start();
                app.BuildUrl = Path.GetDirectoryName(downloadFolderPath)!;
                localApps.apps.Add(app);
                unzip.ContinueWith(task => OnDownloadFinished(app), TaskScheduler.FromCurrentSynchronizationContext());

            };
            client.DownloadFileAsync(new Uri(app.BuildUrl), downloadFolderPath);
        }

        private void OnDownloadFinished(StoreApp app) {
            installing.IsVisible = false;
            install.IsVisible = true;
            
            

            SaveDb();
            LoadAppToContext(app);
            isBusy = false;
            this.Find<StoreAppDBView>("AppListDisplay").IsEnabled = true;
        }

        private void SaveDb() {
            File.WriteAllText(localDb,localApps.ToJSON());
        }

        public bool IsInstalled(StoreApp app) {
            return localApps.apps.Any(localApp => localApp.Id == app.Id);
        }
        
        public bool IsAvailable(StoreApp app) {
            return data.Applist.Items.Any(gapp => gapp.Id == app.Id);
        }

        public bool IsUpToDate(StoreApp app) {
            if (!IsInstalled(app)) return false;
            return GetLocal(app).Version == app.Version;
        }

        private StoreApp? GetLocal(StoreApp? app) {
            if (app == null) return null;
            if (!IsInstalled(app)) return null;
            return localApps.apps.First(localApp => localApp.Id == app.Id);
        }
        
        private StoreApp? GetGlobal(StoreApp? app) {
            if (app == null) return null;
            if (!IsAvailable(app)) return null;
            return data.Applist.Items.First(gapp => gapp.Id == app.Id);
        }

        private void RefreshDatabse(object? sender, RoutedEventArgs e) {
            SaveDb();
            LoadDb();
            LoadAppToContext(null);
        }

        private void StyledElement_OnInitialized(object? sender, EventArgs e) {
            LoadDb();
        }

        private void ShowStartupPanel(object? sender, PointerPressedEventArgs e) {
            LoadAppToContext(null!);
        }

        private void WindowResized() {
            if (Width <= 850) {
                this.Find<Grid>("AppinfoGridlayout").IsVisible = false;
                this.Find<TabControl>("AppinfoTablayout").IsVisible = true;
            }
            else {
                this.Find<Grid>("AppinfoGridlayout").IsVisible = true;
                this.Find<TabControl>("AppinfoTablayout").IsVisible = false;
            }
        }

        private void DebugTabItemSt(object? sender, GotFocusEventArgs eventArgs) {
            var w = sender as TabItem;
            var ob = w.GetVisualChildren().ToList()[0].VisualChildren[0].VisualChildren[1] as Border;
            ob.Background = new SolidColorBrush(Colors.WhiteSmoke);
        }
    }
}