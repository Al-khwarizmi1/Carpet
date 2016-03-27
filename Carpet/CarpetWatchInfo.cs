using System.Collections.Generic;

namespace Carpet
{
    public class CarpetWatchInfo
    {
        public string Name { get; set; }
        public IEnumerable<string> Dirs { get; set; }
        public bool WatchFiles { get; set; }
        public bool WatchDirs { get; set; }
        public IEnumerable<string> Triggers { get; set; }
        public string DestName { get; set; }
        public string DestBaseDir { get; set; }
        public bool IncludeSubdirectories { get; set; }
    }
}
