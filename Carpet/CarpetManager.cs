using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using System.IO;

namespace Carpet
{
    public class CarpetManager
    {
        private readonly CarpetWatchInfo _info;
        private FileSystemWatcherWrapper _watcher;

        private readonly ScriptRunner<string> _fileDest;
        private readonly ScriptRunner<string> _dirDest;

        private readonly Shortcut _shortcut;

        public CarpetManager(CarpetWatchInfo info)
        {
            _info = info;

            _dirDest = CSharpScript.Create<string>(info.DirDest, ScriptOptions.Default, typeof(CarpetDirectoryInfo)).CreateDelegate();
            _fileDest = CSharpScript.Create<string>(info.FileDest, ScriptOptions.Default, typeof(CarpetFileInfo)).CreateDelegate();

            _shortcut = new Shortcut();
        }

        public void InitialScan()
        {
            if (Directory.Exists(_info.DestBaseDir))
            {
                Directory.Delete(_info.DestBaseDir, true);
            }

            foreach (var dirToWatch in _info.Dirs)
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
            _watcher = new FileSystemWatcherWrapper(_info.Dirs, _info.IncludeSubdirectories, CreateFile, CreateDir);
        }

        public void StopWatch()
        {
            _watcher.Stop();
        }

        public void CreateFile(string path)
        {
            var fileInfo = new CarpetFileInfo(path);

            var dest = _fileDest(fileInfo).Result;
            if (dest == null)
            {
                return;
            }

            _shortcut.Create(_info.DestBaseDir + dest, path);
        }

        public void CreateDir(string path)
        {
            var dirInfo = new CarpetDirectoryInfo(path);

            var dest = _dirDest(dirInfo).Result;

            if (dest == null)
            {
                return;
            }

            _shortcut.Create(_info.DestBaseDir + dest, path);
        }

    }
}
