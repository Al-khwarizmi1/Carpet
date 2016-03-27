using System.IO;

namespace Carpet
{
    public class CarpetManager
    {
        private readonly CarpetWatchInfo _info;

        public CarpetManager(CarpetWatchInfo info)
        {
            _info = info;
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

        public void Start()
        {

        }


        public void Create(string path)
        {

        }


    }
}
