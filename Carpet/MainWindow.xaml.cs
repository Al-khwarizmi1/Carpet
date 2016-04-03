using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace Carpet
{
    public partial class MainWindow : Window
    {
        private readonly SystemTray _systemTray;
        private readonly ConfigManager _configManager;

        private IList<CarpetManager> managers;

        private CarpetWatchInfo _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _systemTray = new SystemTray(this);
            _configManager = new ConfigManager();

            managers = new List<CarpetManager>();

            //LoadFromFile();

            var list = new ObservableCollection<ComboBoxItem>();

            var configs = _configManager.Load();

            foreach (var config in configs)
            {
                list.Add(new ComboBoxItem { Content = config.Name, DataContext = config });
            }

            WatchInfoCombo.ItemsSource = list;
            WatchInfoCombo.SelectionChanged += WatchInfoCombo_SelectionChanged;


            InitializeAvalon();

            _viewModel = new CarpetWatchInfo
            {
                DestBaseDir = @"c:\Destination",
                IncludeSubdirectories = true,
                Name = "My watch",
                Dirs = new[] { @"c:\Program files", @"c:\Windows" },
                DirDest = "\treturn null;",
                FileDest = "\treturn null;"
            };

            UpdateViewModel(_viewModel);
        }

        private void WatchInfoCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _viewModel = ((ComboBoxItem)WatchInfoCombo.SelectedItem).DataContext as CarpetWatchInfo;
            UpdateViewModel(_viewModel);
        }

        public void UpdateViewModel(CarpetWatchInfo info)
        {
            this.DataContext = new CarpetWatchInfoEditViewModel
            {
                Name = info.Name,
                DestBaseDir = info.DestBaseDir,
                IncludeSubdirectories = info.IncludeSubdirectories,
            };

            GenerateCode(info.Dirs, info.FileDestFunc, info.DirDestFunc);
        }

        private void LoadFromFile()
        {
            foreach (var config in _configManager.Load())
            {
                var mananger = new CarpetManager(config);
                mananger.StartWatch();
                managers.Add(mananger);
            }
        }

        private void InitializeAvalon()
        {
            XmlTextReader loXmlTextReader = new XmlTextReader(File.OpenRead("CSharp-Mode.xshd"));
            CodeEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(loXmlTextReader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            CodeEditor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(CodeEditor.Options);

            //var segments = CodeEditor.TextArea.ReadOnlySectionProvider.GetDeletableSegments(new TextSegment
            //{
            //    StartOffset = 0,
            //    Length = CodeEditor.TextArea.Document.TextLength
            //});


            CodeEditor.TextArea.TextEntering += CodeEditor_TextEntering;
            CodeEditor.TextArea.TextEntered += CodeEditor_TextEntered;
        }

        private void GenerateCode(IEnumerable<string> dirs, CustomFunction<CarpetFileInfo> fileFunction, CustomFunction<CarpetDirectoryInfo> dirFunction)
        {
            CodeEditor.Clear();

            var directoriesToWatch = @"
// Directories to watch, one per line
";

            CodeEditor.TextArea.Document.Text = directoriesToWatch;

            var a3 = new TextSegment()
            {
                StartOffset = 0,
                Length = CodeEditor.TextArea.Document.Text.Length
            };

            CodeEditor.TextArea.Document.Text += string.Join("\n", dirs);


            var p = new TextSegmentReadOnlySectionProviderIgnoreWrapper<TextSegment>(CodeEditor.Document);

            foreach (var readonlysegment in AddFunctionToEditor(fileFunction, CodeEditor).Union(AddFunctionToEditor(dirFunction, CodeEditor)))
            {
                p.Segments.Add(readonlysegment);
            }

            p.Segments.Add(a3);

            CodeEditor.TextArea.ReadOnlySectionProvider = p;
        }

        private IList<TextSegment> AddFunctionToEditor<T>(CustomFunction<T> function, TextEditor editor)
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

        void CodeEditor_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                IList<ICompletionData> data = new List<ICompletionData>();

                if (IsAutocompleteForVariable(_viewModel.FileDestFunc.Parameter.Name))
                {
                    data = _viewModel.FileDestFunc.Parameter.CompletionData;
                }
                else if (IsAutocompleteForVariable(_viewModel.DirDestFunc.Parameter.Name))
                {
                    data = _viewModel.DirDestFunc.Parameter.CompletionData;
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

        void CodeEditor_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

        private void Reset_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateViewModel(_viewModel);
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
