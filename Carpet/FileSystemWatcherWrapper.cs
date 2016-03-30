using System.Collections.Generic;
using System.IO;

namespace Carpet
{
    public class FileSystemWatcherWrapper
    {
        private readonly IList<FileSystemWatcher> _watchers;

        private readonly CarpetWatchInfo _info;
        private readonly CarpetManager _manager;

        public FileSystemWatcherWrapper(
            CarpetManager manager,
            CarpetWatchInfo info)
        {
            _manager = manager;
            _info = info;

            CreateWatchers();
        }

        private void CreateWatchers()
        {
            foreach (var watchDir in _info.Dirs)
            {
                var watcher = new FileSystemWatcher
                {
                    Path = watchDir,
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName,
                    Filter = "*.*"
                };

                watcher.Error += Watcher_Error;

                watcher.Created += WatcherOnCreated;

                watcher.IncludeSubdirectories = _info.IncludeSubdirectories;
                watcher.EnableRaisingEvents = true;

                _watchers.Add(watcher);
            }
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            throw e.GetException();
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Created)
            {
                //TODO: Implement other change types
                return;
            }

            CreateIfFile(e.FullPath);

            CreateIfDirectory(e.FullPath);
        }


        private void CreateIfFile(string path)
        {

            if (File.Exists(path) == false ||
                _info.WatchFiles == false)
            {
                return;
            }

            _manager.Create(path);
        }

        private void CreateIfDirectory(string path)
        {
            if (Directory.Exists(path) == false ||
                _info.WatchDirs == false)
            {
                return;
            }

            _manager.Create(path);
        }

    }
}
