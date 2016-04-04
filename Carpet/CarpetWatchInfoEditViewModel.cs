
using Carpet.Annotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Carpet
{
    public class CarpetWatchInfoEditViewModel : INotifyPropertyChanged
    {
        private string name;
        public string Name { get { return name; } set { name = value; OnPropertyChanged(); } }

        private bool includeSubdirectories;
        public bool IncludeSubdirectories { get { return includeSubdirectories; } set { includeSubdirectories = value; OnPropertyChanged(); } }

        private string destBaseDir;
        public string DestBaseDir { get { return destBaseDir; } set { destBaseDir = value; OnPropertyChanged(); } }

        private string code;
        public string Code { get { return code; } set { code = value; OnPropertyChanged(); } }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
