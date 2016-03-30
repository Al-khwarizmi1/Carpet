using Newtonsoft.Json;
using System.Collections.Generic;
using System.Windows;

namespace Carpet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ConfigFile = "config.json";

        public MainWindow()
        {
            InitializeComponent();
            Deserialize();
        }


        private void Deserialize()
        {
            if (System.IO.File.Exists(ConfigFile) == false)
            {
                return;
            }

            var configs = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(ConfigFile), typeof(IEnumerable<CarpetWatchInfo>),
                   new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }) as IEnumerable<CarpetWatchInfo>;
            foreach (var config in configs)
            {
                new CarpetManager(config).InitialScan();
            }
        }

    }
}
