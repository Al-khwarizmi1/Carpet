using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        private CustomFunction _filePathFunction;
        private CustomFunction _dirPathFunction;

        public MainWindow()
        {
            InitializeComponent();

            this.StateChanged += MainWindow_StateChanged;
            this.Closing += MainWindow_Closing;

            managers = new List<CarpetManager>();

            //LoadFromFile();
            CreateIcon();


            _filePathFunction = new CustomFunction("GetFilesPath", new CustomFunctionParameter(typeof(CarpetFileInfo), "file"), "\treturn null;");
            _dirPathFunction = new CustomFunction("GetDirPath", new CustomFunctionParameter(typeof(CarpetDirectoryInfo), "dir"), "\treturn null;");

            InitializeAvalon();

        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _sysTrayIcon.Dispose();
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


            var directoriesToWatch = @"
// Directories to watch, one per line
";

            CodeEditor.TextArea.Document.Text = directoriesToWatch;

            var a3 = new TextSegment()
            {
                StartOffset = 0,
                Length = CodeEditor.TextArea.Document.Text.Length
            };

            CodeEditor.TextArea.Document.Text += @"
C:\MyFolder\
";


            var p = new TextSegmentReadOnlySectionProviderIgnoreStartAndEnd<TextSegment>(CodeEditor.Document);

            foreach (var readonlysegment in AddFunctionToEditor(_filePathFunction, CodeEditor).Union(AddFunctionToEditor(_dirPathFunction, CodeEditor)))
            {
                p.Segments.Add(readonlysegment);
            }

            p.Segments.Add(a3);

            CodeEditor.TextArea.ReadOnlySectionProvider = p;


            var segments = CodeEditor.TextArea.ReadOnlySectionProvider.GetDeletableSegments(new TextSegment
            {
                StartOffset = 0,
                Length = CodeEditor.TextArea.Document.TextLength
            });


            CodeEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            CodeEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
        }

        private IList<TextSegment> AddFunctionToEditor(CustomFunction function, TextEditor editor)
        {
            var header = new TextSegment()
            {
                StartOffset = CodeEditor.TextArea.Document.Text.Length,
                Length = function.FunctionHeader.Length
            };

            CodeEditor.TextArea.Document.Text += function.FunctionHeader;

            CodeEditor.TextArea.Document.Text += function.FunctionBody;

            var footer = new TextSegment()
            {
                StartOffset = CodeEditor.TextArea.Document.Text.Length,
                Length = function.FunctionFooter.Length
            };

            CodeEditor.TextArea.Document.Text += function.FunctionFooter;

            return new List<TextSegment> { header, footer };
        }

        CompletionWindow completionWindow;


        private bool IsAutocompleteForVariable(string variable)
        {
            var trigger = CodeEditor.TextArea.Document.GetText(CodeEditor.TextArea.Caret.Offset - variable.Length - 1, variable.Length);

            return trigger == variable;
        }

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                IList<ICompletionData> data = new List<ICompletionData>();

                if (IsAutocompleteForVariable(_filePathFunction.Parameter.Name))
                {
                    data = _filePathFunction.Parameter.CompletionData;
                }
                else if (IsAutocompleteForVariable(_dirPathFunction.Parameter.Name))
                {
                    data = _dirPathFunction.Parameter.CompletionData;
                }

                if (data.Any())
                {
                    completionWindow = new CompletionWindow(CodeEditor.TextArea);

                    foreach (var completionData in data)
                    {
                        completionWindow.CompletionList.CompletionData.Add(completionData);
                    }
                    completionWindow.Show();
                    completionWindow.Closed += delegate
                    {
                        completionWindow = null;
                    };
                }

            }
            else if (e.Text == "\\")
            {
                var line = CodeEditor.TextArea.Document.GetLineByNumber(CodeEditor.TextArea.Caret.Line);
                var lineText = CodeEditor.TextArea.Document.GetText(line.Offset, line.Length);
                if (Directory.Exists(lineText))
                {
                    try
                    {
                        var dirs = Directory.GetDirectories(lineText);

                        completionWindow = new CompletionWindow(CodeEditor.TextArea);
                        IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;

                        foreach (var dir in dirs)
                        {
                            data.Add(new AutoCompletionData(dir.Split('\\').Last()));
                        }

                        completionWindow.Show();
                        completionWindow.Closed += delegate
                        {
                            completionWindow = null;
                        };
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
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
