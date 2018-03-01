using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NginxHelper
{
    class Config
    {
        public string ServiceName { get; }
        public string NginxDir { get; }
        public string ConfTemplate { get; }
        public string ConfSaveDir { get; }

        private string ConfigFile;
        private Dictionary<string, string> Dict;

        public Config(string iniFile)
        {
            ConfigFile = iniFile;
            Dict = new Dictionary<string, string>();

            try
            {
                using (var sw = new StreamReader(iniFile))
                {
                    string line;
                    while ((line = sw.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line == "") continue;
                        var c = line[0];
                        if (c == ';' || c == '[') continue;
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                            Dict.Add(parts[0].TrimEnd(), parts[1].TrimStart());
                    }
                }
            }
            catch (FileNotFoundException)
            {
                throw new AppException($"Cannot find the config file \"{ConfigFile}\"");
            }

            ServiceName = Get("ServiceName", "nginx");
            NginxDir = Get("NginxDir");
            ConfTemplate = Get("ConfTemplate");
            ConfSaveDir = Get("ConfSaveDir");
        }

        public string Get(string name, string defaultValue = null)
        {
            try
            {
                return Dict[name];
            }
            catch (KeyNotFoundException)
            {
                if (!String.IsNullOrEmpty(defaultValue)) return defaultValue;
                throw new AppException($"Missing `{name}` in the config file \"{ConfigFile}\"");
            }
        }
    }
}
