using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;

namespace Carpet
{
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
            CodeEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(loXmlTextReader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            CodeEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(CodeEditor.Options);

            var startSection =
@"
// Comments about possible values
// Return null to skip file
string GetFilePah(CarpetFileInfo file)
{
";
            var intersection = @"
}

// Comments about possible values
// Return null to skip directory
string GetDirectoryPah(CarpetDirectoryInfo dir)
{
";
            var endingSection = @"
}";


            var getFilePathFunction = @"
    return null;
";
            var getDirPathFunction = @"
    return null;
";

            CodeEditor.TextArea.Document.Text = startSection;
            var a = new TextSegment()
            {
                StartOffset = 0,
                Length = CodeEditor.TextArea.Document.Text.Length
            };

            CodeEditor.TextArea.Document.Text += getFilePathFunction;

            var a1 = new TextSegment()
            {
                StartOffset = CodeEditor.TextArea.Document.Text.Length,
                Length = intersection.Length
            };
            CodeEditor.TextArea.Document.Text += intersection;
            CodeEditor.TextArea.Document.Text += getDirPathFunction;

            var a2 = new TextSegment()
            {
                StartOffset = CodeEditor.TextArea.Document.Text.Length,
                Length = endingSection.Length
            };

            CodeEditor.TextArea.Document.Text += endingSection;



            var p = new TextSegmentReadOnlySectionProviderIgnoreStartAndEnd<TextSegment>(CodeEditor.Document);

            p.Segments.Add(a);
            p.Segments.Add(a1);
            p.Segments.Add(a2);

            CodeEditor.TextArea.ReadOnlySectionProvider = p;

            var segments = CodeEditor.TextArea.ReadOnlySectionProvider.GetDeletableSegments(new TextSegment
            {
                StartOffset = 0,
                Length = CodeEditor.TextArea.Document.TextLength
            });


            CodeEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            CodeEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
        }

        CompletionWindow completionWindow;

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                //Magic 5 - 'dir.'(4chars), 'file.'(5chars), just take 5 chars and check which one triggered autocomplete
                var trigger = CodeEditor.TextArea.Document.GetText(CodeEditor.TextArea.Caret.Offset - 5, 5);

                if (trigger.EndsWith("file."))
                {
                    // Open code completion after the user has pressed dot:
                    completionWindow = new CompletionWindow(CodeEditor.TextArea);
                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                    data.Add(new AutoCompletionData("file1"));
                    data.Add(new AutoCompletionData("file2"));
                    data.Add(new AutoCompletionData("file3"));
                    completionWindow.Show();
                    completionWindow.Closed += delegate
                    {
                        completionWindow = null;
                    };
                }
                else if (trigger.EndsWith("dir."))
                {
                    // Open code completion after the user has pressed dot:
                    completionWindow = new CompletionWindow(CodeEditor.TextArea);
                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                    data.Add(new AutoCompletionData("dir1"));
                    data.Add(new AutoCompletionData("dir2"));
                    data.Add(new AutoCompletionData("dir3"));
                    completionWindow.Show();
                    completionWindow.Closed += delegate
                    {
                        completionWindow = null;
                    };
                }

            }
        }

        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }
    }
}
