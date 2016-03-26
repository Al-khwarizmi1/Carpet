using IWshRuntimeLibrary;

namespace Carpet
{
    public class Shortcut
    {
        public void Create(string fromPath, string toPath)
        {
            var shell = new WshShell();
            var shortcut = (IWshShortcut)shell.CreateShortcut(toPath);

            shortcut.TargetPath = fromPath;
            shortcut.Save();
        }

        public void Delete(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        public void Rename(string fromPath, string toPath)
        {
            Delete(fromPath);
            Create(fromPath, toPath);
        }
    }
}
