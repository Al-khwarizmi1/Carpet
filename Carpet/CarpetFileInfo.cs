using System;

namespace Carpet
{
    public class CarpetFileInfo
    {
        public string Extension;
        public string Path;
        public string Name;
        public DateTime CreationTime;
        public string FullName;

        public CarpetFileInfo(string path)
        {
            if (System.IO.File.Exists(path) == false)
            {
                return;
            }

            CreationTime = System.IO.File.GetCreationTime(path);
            Path = path;

            if (System.IO.Path.HasExtension(path))
            {
                Extension = System.IO.Path.GetExtension(path);
            }

            Name = System.IO.Path.GetFileNameWithoutExtension(path);

            FullName = System.IO.Path.GetFileName(path);
        }
    }
}
