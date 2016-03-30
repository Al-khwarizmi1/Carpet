using System.Collections.Generic;

namespace Carpet
{
    public class CarpetWatchInfo
    {
        public string Name { get; set; }
        public IEnumerable<string> Dirs { get; set; }
        public bool IncludeSubdirectories { get; set; }

        public bool WatchFiles { get; set; }
        public bool WatchDirs { get; set; }

        public string FileTrigger { get; set; }
        public string DirTrigger { get; set; }

        public string FileDest { get; set; }
        public string DirDest { get; set; }

        public string DestBaseDir { get; set; }
    }
}
