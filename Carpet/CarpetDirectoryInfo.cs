namespace Carpet
{
    public class CarpetDirectoryInfo
    {
        public CarpetDirectoryInfo(string path)
        {
            if (System.IO.Directory.Exists(path) == false)
            {
                return;
            }
        }
    }
}
