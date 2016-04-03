using Newtonsoft.Json;
using System.Collections.Generic;

namespace Carpet
{
    public class CarpetWatchInfo
    {
        public CarpetWatchInfo()
        {
            FileDestFunc = new CustomFunction<CarpetFileInfo>("GetFilePath", new CustomFunctionParameter(typeof(CarpetFileInfo), "file"), string.Empty);
            DirDestFunc = new CustomFunction<CarpetDirectoryInfo>("GetDirPath", new CustomFunctionParameter(typeof(CarpetDirectoryInfo), "dir"), string.Empty);
        }

        public string Name { get; set; }
        public IEnumerable<string> Dirs { get; set; }
        public bool IncludeSubdirectories { get; set; }

        public string FileDest
        {
            get { return FileDestFunc.FunctionBody; }
            set { FileDestFunc.FunctionBody = value; }
        }

        public string DirDest
        {
            get { return DirDestFunc.FunctionBody; }
            set { DirDestFunc.FunctionBody = value; }
        }

        [JsonIgnore]
        public CustomFunction<CarpetFileInfo> FileDestFunc { get; set; }
        [JsonIgnore]
        public CustomFunction<CarpetDirectoryInfo> DirDestFunc { get; set; }

        public string DestBaseDir { get; set; }
    }

}
