using System;
using System.Collections.Generic;
using System.IO;

namespace Carpet
{
    public class Watcher
    {
        private readonly IList<FileSystemWatcher> _watchers;
        private readonly Shortcut _shortcut;

        private readonly Func<CarperFileInfo, CarpetDirectoryInfo, bool> _trigger;
        private readonly Func<CarperFileInfo, CarpetDirectoryInfo, string> _destination;
        private readonly CarpetWatchInfo _watchInfo;

        public Watcher(Func<CarperFileInfo, CarpetDirectoryInfo, bool> trigger, Func<CarperFileInfo, CarpetDirectoryInfo, string> destination, CarpetWatchInfo watchInfo)
        {
            _trigger = trigger;
            _destination = destination;
            _watchInfo = watchInfo;
            _shortcut = new Shortcut();

            foreach (var watch in _watchers)
            {
                var watcher = new FileSystemWatcher
                {
                    Path = watch.Path,
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName,
                    Filter = "*.*"
                };

                watcher.Error += _watcher_Error;

                watcher.Deleted += Watcher_Deleted;
                watcher.Created += WatcherOnCreated;
                watcher.Renamed += Watcher_Renamed;

                watcher.IncludeSubdirectories = false;
                watcher.EnableRaisingEvents = true;

                _watchers.Add(watcher);
            }
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            throw e.GetException();
        }

        private string Destination(string path)
        {
            var fileInfo = new CarperFileInfo(path);
            var dirInfo = new CarpetDirectoryInfo(path);

            if (_trigger(fileInfo, dirInfo) == false)
            {
                return null;
            }
            return _destination(fileInfo, dirInfo);
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            _shortcut.Rename(e.OldFullPath, Path.Combine(_watchInfo.DestBaseDir, Destination(e.FullPath)));
        }

        private void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            _shortcut.Create(e.FullPath, Path.Combine(_watchInfo.DestBaseDir, Destination(e.FullPath)));
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            _shortcut.Delete(e.FullPath);
        }

    }
}
