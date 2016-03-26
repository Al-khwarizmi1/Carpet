namespace Carpet
{
    public class CarperFileInfo
    {
        public CarperFileInfo(string path)
        {
            if (System.IO.File.Exists(path) == false)
            {
                return;
            }

        }
    }
}
