using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace bajanetLauncher {
    public class StoreAppDB {
        public List<StoreApp> apps = new List<StoreApp>() {
        };

        public StoreAppDB() {
            
        }
        
        [Serializable]
        struct SStoreApp {
            public int id;
            public string name;
            public string icon;
            public string description;
            public string version;
            public string updatedate;
            public string changelog;
            public string windows_build;
            public string linux_build;
            public string mac_build;
            public string android_build;
            public SStoreApp(int id, string name, string icon, string description, string version, string updatedate, string changelog, string windowsBuild, string linuxBuild, string macBuild, string androidBuild) {
                this.id = id;
                this.name = name;
                this.icon = icon;
                this.description = description;
                this.version = version;
                this.updatedate = updatedate;
                this.changelog = changelog;
                windows_build = windowsBuild;
                linux_build = linuxBuild;
                mac_build = macBuild;
                android_build = androidBuild;
            }
        }
        
        [Serializable]
        struct SStoreDb {
            public SStoreApp[] software;
        }
        
        public StoreAppDB(string json) {
            // temp fix for bug in backend
            if (json == String.Empty || json == "{}[]") return;
            dynamic d = JsonConvert.DeserializeObject(json)!;
            apps = new List<StoreApp>();
            if (d.software == null) return;
            string os = MainWindow.GetOS();
            foreach (var i in d.software) {
                string url = i.windows_build; //TODO: add cross platform support
                apps.Add(new StoreApp(int.Parse(i.id.ToString()), i.name.ToString(), i.icon.ToString(), i.description.ToString(), i.version.ToString(), i.updatedate.ToString(), i.changelog.ToString(), url, "Windows"));
            }
        }

        public string ToJSON() {
            List<SStoreApp> software = new();
            // TODO: add cross-platform support
            foreach (var item in apps) {
                software.Add(new SStoreApp(item.Id, item.Name, StoreApp.ToBase64(item.AppIcon), item.Description, item.Version, item.VersionDate.ToString(), item.Changelog, item.BuildUrl, "","",""));
            }

            SStoreDb d = new() {
                software = software.ToArray()
            };

            return JsonConvert.SerializeObject(d);
        }

        public IEnumerable<StoreApp> GetItems() => apps;
    }
}