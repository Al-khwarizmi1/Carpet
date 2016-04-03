using System.IO;

namespace Carpet
{
    public class CarpetManager
    {
        public CarpetWatchInfo Info { get; private set; }
        private FileSystemWatcherWrapper _watcher;

        private readonly Shortcut _shortcut;

        public CarpetManager(CarpetWatchInfo info)
        {
            Info = info;

            _shortcut = new Shortcut();
        }

        public void InitialScan()
        {
            foreach (var dirToWatch in Info.Dirs)
            {
                var directories = Directory.GetDirectories(dirToWatch);

                foreach (var dir in directories)
                {
                    CreateDir(dir);
                }

                var files = Directory.GetFiles(dirToWatch);

                foreach (var f in files)
                {
                    CreateFile(f);
                }
            }
        }

        public void StartWatch()
        {
            _watcher = new FileSystemWatcherWrapper(Info.Dirs, Info.IncludeSubdirectories, CreateFile, CreateDir);
        }

        public void StopWatch()
        {
            _watcher.Stop();
        }

        public void CreateFile(string path)
        {
            var fileInfo = new CarpetFileInfo(path);

            var dest = Info.FileDestFunc.Invoke(fileInfo);
            if (dest == null)
            {
                return;
            }

            _shortcut.Create(Info.DestBaseDir + dest, path);
        }

        public void CreateDir(string path)
        {
            var dirInfo = new CarpetDirectoryInfo(path);

            var dest = Info.DirDestFunc.Invoke(dirInfo);

            if (dest == null)
            {
                return;
            }

            _shortcut.Create(Info.DestBaseDir + dest, path);
        }

    }
}
