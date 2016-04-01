using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Xml;

namespace Carpet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string ConfigFile = "config.json";

        private IList<CarpetManager> managers;

        private static string BaseDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        private NotifyIcon _sysTrayIcon;

        public MainWindow()
        {
            InitializeComponent();

            this.StateChanged += MainWindow_StateChanged;

            managers = new List<CarpetManager>();

            //LoadFromFile();
            CreateIcon();

            InitializeAvalon();
        }

        private void MainWindow_StateChanged(object sender, System.EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
            }
        }

        private void LoadFromFile()
        {
            if (File.Exists(ConfigFile) == false)
            {
                return;
            }

            var configs = JsonConvert.DeserializeObject(System.IO.File.ReadAllText(ConfigFile), typeof(IEnumerable<CarpetWatchInfo>),
                   new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All }) as IEnumerable<CarpetWatchInfo>;

            foreach (var config in configs)
            {
                var mananger = new CarpetManager(config);
                mananger.StartWatch();
                managers.Add(mananger);
            }
        }


        public void CreateIcon()
        {
            _sysTrayIcon = new NotifyIcon();
            _sysTrayIcon.Text = @"Carpet";
            _sysTrayIcon.Icon = new System.Drawing.Icon(Path.Combine(BaseDir, @"carpet.ico"), 40, 40);
            _sysTrayIcon.Visible = true;
            _sysTrayIcon.DoubleClick += _sysTrayIcon_DoubleClick;
        }

        private void _sysTrayIcon_DoubleClick(object sender, System.EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = true;
                this.WindowState = WindowState.Normal;
            }
        }


        private void InitializeAvalon()
        {
            XmlTextReader loXmlTextReader = new XmlTextReader(File.OpenRead("CSharp-Mode.xshd"));
            AeteSourceCode.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(loXmlTextReader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            AeteSourceCode.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(AeteSourceCode.Options);
        }
    }
}
