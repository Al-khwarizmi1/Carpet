using System;
using System.Collections.Generic;
using System.IO;

namespace Carpet
{
    public class FileSystemWatcherWrapper
    {
        private readonly IEnumerable<string> _directoriesToWatch;
        private readonly bool _includeSubdirectories;
        private readonly Action<string> _createFile;
        private readonly Action<string> _createDir;
        private readonly IList<FileSystemWatcher> _watchers;

        public FileSystemWatcherWrapper(
            IEnumerable<string> directoriesToWatch,
            bool includeSubdirectories,
            Action<string> createFile,
            Action<string> createDir)
        {
            _directoriesToWatch = directoriesToWatch;
            _includeSubdirectories = includeSubdirectories;
            _createFile = createFile;
            _createDir = createDir;
            _watchers = new List<FileSystemWatcher>();

            CreateWatchers();
        }

        public void Stop()
        {
            foreach (var watcher in _watchers)
            {
                watcher.Dispose();
            }
        }

        private void CreateWatchers()
        {
            foreach (var watchDir in _directoriesToWatch)
            {
                var watcher = new FileSystemWatcher
                {
                    Path = watchDir,
                    NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName,
                    Filter = "*.*"
                };

                watcher.Error += Watcher_Error;

                watcher.Created += WatcherOnCreated;

                watcher.IncludeSubdirectories = _includeSubdirectories;
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
            if (File.Exists(path) == false)
            {
                return;
            }

            _createFile(path);
        }

        private void CreateIfDirectory(string path)
        {
            if (Directory.Exists(path) == false)
            {
                return;
            }

            _createDir(path);
        }

    }
}
