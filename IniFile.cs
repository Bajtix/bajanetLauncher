using System;
using System.Collections.Generic;

namespace bajanetLauncher {
    public class IniFile {
        private Dictionary<string, Dictionary<string, string>> file = new();

        public IniFile(string text) {
            Parse(text);
        }

        private void Parse(string text) {
            string currentHeader = "<global>";
            file.Add(currentHeader, new Dictionary<string, string>());
            
            string[] lines = text.Split(Environment.NewLine);

            foreach (var line in lines) {
                if (line.StartsWith('[')) {
                    //its a header
                    currentHeader = line.Replace("[","").Replace("]","");
                    file.Add(currentHeader, new Dictionary<string, string>());
                    continue;
                }

                string varname = line.Split('=')[0];
                string valname = line.Split('=')[1];
                file[currentHeader].Add(varname,valname);
            }
        }

        public string Get(string variable, string header = "<global>") {
            try {
                return file[header][variable];
            }
            catch {
                return "";
            }
        }
    }
}