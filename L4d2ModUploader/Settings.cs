using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace L4d2ModUploader
{
    public class Settings
    {
        private static Settings? _instance;
        public static Settings Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                if (!File.Exists(SettingsFilePath))
                {
                    File.WriteAllText(SettingsFilePath, JsonConvert.SerializeObject(new Settings()));
                }
                _instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsFilePath))!;
                return _instance;
            }
        }

        private static readonly string SettingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "L4d2ModUploaderConfig.json");


        public string SteamApiKey { get; set; } = "";
        public string SteamL4d2RootPath { get; set; } = "";
        public string ScpUser { get; set; } = "";
        public string ScpPassword { get; set; } = "";
        public string ScpHost { get; set; } = "";
        public string HostL4d2RootPath { get; set; } = "";
    }
}
