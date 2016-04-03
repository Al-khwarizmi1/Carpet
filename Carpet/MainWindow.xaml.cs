using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Carpet
{
    public partial class MainWindow : Window
    {
        private readonly SystemTray _systemTray;
        private readonly ConfigManager _configManager;

        private IList<CarpetManager> managers;

        private CarpetWatchInfo _model;
        private CarpetWatchInfoEditViewModel _viewModel;

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


            _model = new CarpetWatchInfo
            {
                DestBaseDir = @"c:\",
                IncludeSubdirectories = true,
                Name = "My watch",
                Dirs = new[] { @"c:\Program files", @"c:\Windows" },
                DirDest = "\treturn null;",
                FileDest = "\treturn null;"
            };

            UpdateViewModel(_model);
        }

        private void WatchInfoCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _model = ((ComboBoxItem)WatchInfoCombo.SelectedItem).DataContext as CarpetWatchInfo;
            UpdateViewModel(_model);
        }

        public void UpdateViewModel(CarpetWatchInfo info)
        {
            _viewModel = new CarpetWatchInfoEditViewModel
            {
                Name = info.Name,
                DestBaseDir = info.DestBaseDir,
                IncludeSubdirectories = info.IncludeSubdirectories,
            };
            this.DataContext = _viewModel;

            CodeEditor.GenerateCode(info.Dirs, info.FileDestFunc, info.DirDestFunc);
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

        private void Reset_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateViewModel(_model);
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            var segments = CodeEditor.GetDeletableSegments().ToArray();
            var watchDirs = segments[0].Split('\n').Select(_ => _.Trim());

            var error = AreDirectoriesValid(_viewModel.DestBaseDir, watchDirs);
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK);
                return;
            }

            error = IsNameValid(_viewModel.Name);
            if (error != null)
            {
                MessageBox.Show(error, "Error", MessageBoxButton.OK);
                return;
            }

            try
            {
                var f = new CustomFunction<CarpetFileInfo>("GetFilePath", PredefinedCustomFunctionParameter.File, segments[1]).Test();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Compilation error: GetFilePath", MessageBoxButton.OK);
                return;
            }

            try
            {
                var f = new CustomFunction<CarpetFileInfo>("GetDirPath", PredefinedCustomFunctionParameter.Dir, segments[2]).Test();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Compilation error:GetDirPath", MessageBoxButton.OK);
                return;
            }

        }

        private string IsNameValid(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "Name required";
            }

            if (managers.FirstOrDefault(_ => _.Info.Name == name) != null)
            {
                return "Name must be unique";
            }
            return null;
        }

        private string AreDirectoriesValid(string baseDir, IEnumerable<string> watchDirs)
        {
            if (Directory.Exists(baseDir) == false)
            {
                return "Invalid base directory.";
            }

            var invalidDir = watchDirs.FirstOrDefault(_ => Directory.Exists(_) == false);
            if (invalidDir != null)
            {
                return $"Invalid watch directory'{invalidDir}'.";
            }

            if (watchDirs.Count() == 0)
            {
                return "At least one watch directory required";
            }

            var disks = new[] { baseDir[0] }.Concat(watchDirs.Select(_ => _[0])).GroupBy(_ => _);
            if (disks.Count() > 1)
            {
                return "All directories must be on same disk";
            }

            return null;
        }
    }
}
