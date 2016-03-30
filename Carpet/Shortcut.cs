using IWshRuntimeLibrary;
using System;
using System.Linq;

namespace Carpet
{
    public class Shortcut
    {
        public void Create(string placedIn, string linksTo)
        {
            if (System.IO.File.Exists(placedIn))
            {
                return;
            }
            else
            {
                var parts = placedIn.Split('/');
                var dir = String.Join("/", parts.Take(parts.Length - 1));
                if (System.IO.Directory.Exists(dir) == false)
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
            }

            var shell = new WshShell();
            var shortcut = (IWshShortcut)shell.CreateShortcut(placedIn + ".lnk");

            shortcut.TargetPath = linksTo;
            shortcut.Save();
        }

        public void Delete(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        public void Rename(string shouldBePlacedIn, string shouldLinkTo, string wasPlacedIn)
        {
            Delete(wasPlacedIn);
            Create(shouldBePlacedIn, shouldLinkTo);
        }
    }
}
