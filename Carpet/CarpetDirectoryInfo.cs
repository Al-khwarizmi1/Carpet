using System;

namespace Carpet
{
    public class CarpetDirectoryInfo
    {
        public string Path;
        public string Name;
        public DateTime CreationTime;

        public CarpetDirectoryInfo(string path)
        {
            if (System.IO.Directory.Exists(path) == false)
            {
                return;
            }

            CreationTime = System.IO.Directory.GetCreationTime(path);
            Path = path;

            Name = System.IO.Path.GetDirectoryName(path);
        }
    }
}
