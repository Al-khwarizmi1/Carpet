using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using System.IO;

namespace Carpet
{
    public class CarpetManager
    {
        private readonly CarpetWatchInfo _info;
        private FileSystemWatcherWrapper _watcher;
        private readonly ScriptRunner<bool> _fileTrigger;
        private readonly ScriptRunner<bool> _dirTrigger;

        private readonly ScriptRunner<string> _fileDest;
        private readonly ScriptRunner<string> _dirDest;

        private readonly Shortcut _shortcut;

        public CarpetManager(CarpetWatchInfo info)
        {
            _info = info;

            if (info.WatchDirs)
            {
                _dirTrigger = CSharpScript.Create<bool>(info.DirTrigger, ScriptOptions.Default, typeof(CarpetDirectoryInfo)).CreateDelegate();
                _dirDest = CSharpScript.Create<string>(info.DirDest, ScriptOptions.Default, typeof(CarpetDirectoryInfo)).CreateDelegate();
            }

            if (info.WatchFiles)
            {
                _fileDest = CSharpScript.Create<string>(info.FileDest, ScriptOptions.Default, typeof(CarpetFileInfo)).CreateDelegate();
                _fileTrigger = CSharpScript.Create<bool>(info.FileTrigger, ScriptOptions.Default, typeof(CarpetFileInfo)).CreateDelegate();
            }

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
                if (_info.WatchDirs)
                {
                    var directories = Directory.GetDirectories(dirToWatch);

                    foreach (var dir in directories)
                    {
                        Create(dir);
                    }
                }

                if (_info.WatchFiles)
                {
                    var files = Directory.GetFiles(dirToWatch);

                    foreach (var f in files)
                    {
                        Create(f);
                    }
                }
            }
        }

        public void StartWatch()
        {
            _watcher = new FileSystemWatcherWrapper(this, _info);
        }

        public void Create(string path)
        {
            if (File.Exists(path) && _info.WatchFiles)
            {
                CreateFile(path);
            }
            else if (_info.WatchDirs)
            {
                CreateDir(path);
            }
        }

        private void CreateFile(string path)
        {
            var fileInfo = new CarpetFileInfo(path);
            if (_fileTrigger(fileInfo).Result == false)
            {
                return;
            }

            var dest = _fileDest(fileInfo).Result;

            _shortcut.Create(_info.DestBaseDir + dest, path);
        }

        private void CreateDir(string path)
        {
            var dirInfo = new CarpetDirectoryInfo(path);
            if (_dirTrigger(dirInfo).Result == false)
            {
                return;
            }

            var dest = _dirDest(dirInfo).Result;

            _shortcut.Create(_info.DestBaseDir + dest, path);
        }

    }
}
