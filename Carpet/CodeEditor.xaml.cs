using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace Carpet
{
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl
    {
        CompletionWindow completionWindow;
        private IEnumerable<CustomFunctionParameter> Paramters = new List<CustomFunctionParameter> { PredefinedCustomFunctionParameter.File, PredefinedCustomFunctionParameter.Dir };

        public CodeEditor()
        {
            InitializeComponent();
            InitializeAvalon();
        }


        private void InitializeAvalon()
        {
            XmlTextReader loXmlTextReader = new XmlTextReader(File.OpenRead("CSharp-Mode.xshd"));
            Editor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(loXmlTextReader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            Editor.TextArea.IndentationStrategy = new ICSharpCode.AvalonEdit.Indentation.CSharp.CSharpIndentationStrategy(Editor.Options);

            Editor.TextArea.TextEntering += Editor_TextEntering;
            Editor.TextArea.TextEntered += Editor_TextEntered;
        }

        public IEnumerable<string> GetDeletableSegments()
        {
            var segments = Editor.TextArea.ReadOnlySectionProvider.GetDeletableSegments(new TextSegment
            {
                StartOffset = 0,
                Length = Editor.TextArea.Document.TextLength
            });

            return segments.Select(Editor.TextArea.Document.GetText);
        }

        public void GenerateCode(IEnumerable<string> dirs, CustomFunction<CarpetFileInfo> fileFunction, CustomFunction<CarpetDirectoryInfo> dirFunction)
        {
            Editor.Clear();

            var directoriesToWatch = @"
// Directories to watch, one per line
";

            Editor.TextArea.Document.Text = directoriesToWatch;

            var a3 = new TextSegment()
            {
                StartOffset = 0,
                Length = Editor.TextArea.Document.Text.Length
            };

            Editor.TextArea.Document.Text += string.Join("\n", dirs);


            var p = new TextSegmentReadOnlySectionProviderIgnoreWrapper<TextSegment>(Editor.Document);

            foreach (var readonlysegment in AddFunctionToEditor(fileFunction, Editor).Union(AddFunctionToEditor(dirFunction, Editor)))
            {
                p.Segments.Add(readonlysegment);
            }

            p.Segments.Add(a3);

            Editor.TextArea.ReadOnlySectionProvider = p;
        }

        private IList<TextSegment> AddFunctionToEditor<T>(CustomFunction<T> function, TextEditor editor)
        {
            var header = new TextSegment()
            {
                StartOffset = Editor.TextArea.Document.Text.Length,
                Length = function.FunctionHeader.Length
            };

            Editor.TextArea.Document.Text += function.FunctionHeader;

            Editor.TextArea.Document.Text += function.FunctionBody;

            var footer = new TextSegment()
            {
                StartOffset = Editor.TextArea.Document.Text.Length,
                Length = function.FunctionFooter.Length+1
            };

            Editor.TextArea.Document.Text += function.FunctionFooter;

            return new List<TextSegment> { header, footer };
        }

        private bool IsAutocompleteForVariable(string variable)
        {
            var trigger = Editor.TextArea.Document.GetText(Editor.TextArea.Caret.Offset - variable.Length - 1, variable.Length);

            return trigger == variable;
        }


        void Editor_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                IList<ICompletionData> data = new List<ICompletionData>();

                var parameter = Paramters.FirstOrDefault(_ => IsAutocompleteForVariable(_.Name));

                if (parameter != null)
                {
                    data = parameter.CompletionData;
                }

                if (data.Any())
                {
                    completionWindow = new CompletionWindow(Editor.TextArea);

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
                var line = Editor.TextArea.Document.GetLineByNumber(Editor.TextArea.Caret.Line);
                var lineText = Editor.TextArea.Document.GetText(line.Offset, line.Length);
                if (Directory.Exists(lineText))
                {
                    try
                    {
                        var dirs = Directory.GetDirectories(lineText);

                        completionWindow = new CompletionWindow(Editor.TextArea);
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

        void Editor_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }

    }
}
