using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Carpet
{
    public class ConfigManager
    {
        private string ConfigFile = "config.json";

        public IEnumerable<CarpetWatchInfo> Load()
        {
            if (File.Exists(ConfigFile) == false)
            {
                return new List<CarpetWatchInfo>();
            }

            return JsonConvert.DeserializeObject(File.ReadAllText(ConfigFile), typeof(IEnumerable<CarpetWatchInfo>),
                     new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }) as IEnumerable<CarpetWatchInfo>;
        }

        public void Save(IEnumerable<CarpetWatchInfo> infos)
        {
            var serialized = JsonConvert.SerializeObject(infos, Formatting.Indented, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });

            File.WriteAllText(ConfigFile, serialized);
        }
    }
}
