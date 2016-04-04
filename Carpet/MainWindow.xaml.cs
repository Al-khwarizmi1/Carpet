using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace Carpet
{
    public partial class MainWindow : Window
    {
        private readonly SystemTray _systemTray;
        private readonly ConfigManager _configManager;

        private IList<CarpetManager> _managers;
        private IList<CarpetWatchInfo> _watchInfos;

        private CarpetWatchInfo _model;
        private CarpetWatchInfoEditViewModel _viewModel;

        ObservableCollection<ComboBoxItem> watchInfoList;


        public MainWindow()
        {
            InitializeComponent();

            _systemTray = new SystemTray(this);
            _configManager = new ConfigManager();
            _managers = new List<CarpetManager>();
            _watchInfos = _configManager.Load().ToList();

            watchInfoList = new ObservableCollection<ComboBoxItem>();

            LoadFromFile();

            foreach (var config in _managers)
            {
                watchInfoList.Add(new ComboBoxItem { Content = config.Info.Name, DataContext = config.Info });
            }

            WatchInfoCombo.ItemsSource = watchInfoList;
            WatchInfoCombo.SelectionChanged += WatchInfoCombo_SelectionChanged;


            var first = watchInfoList.FirstOrDefault();
            InitViewModel(first);
        }


        public void EnsureAppIsInStartup()
        {
            var registryName = "Carpet by Floatas";

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                object existingKey = key.GetValue(registryName, null);

                if (existingKey == null)
                {
                    key.SetValue(registryName, "\"" + System.Reflection.Assembly.GetExecutingAssembly().Location + "\" -silent");
                }
            }
        }


        private void InitViewModel(ComboBoxItem item)
        {
            if (item == null)
            {
                _model = CreateWatchInfoNew();
                UpdateViewModel(_model);
                WatchInfoCombo.SelectedItem = null;
            }
            else
            {
                _model = item.DataContext as CarpetWatchInfo;
                UpdateViewModel(_model);
                WatchInfoCombo.SelectedItem = item;
            }
        }

        private void WatchInfoCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (WatchInfoCombo.SelectedItem == null)
            {
                Delete.Visibility = Visibility.Collapsed;
                return;
            }
            else
            {
                Delete.Visibility = Visibility.Visible;
            }
            _model = (((ComboBoxItem)WatchInfoCombo.SelectedItem).DataContext as CarpetWatchInfo) ?? CreateWatchInfoNew();
            UpdateViewModel(_model);
        }

        public void UpdateViewModel(CarpetWatchInfo info)
        {
            //TODO: add automapper
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
            foreach (var config in _watchInfos)
            {
                var mananger = new CarpetManager(config);
                mananger.StartWatch();
                _managers.Add(mananger);
            }
        }

        private void Reset_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateViewModel(_model);
        }

        private void Save_OnClick(object sender, RoutedEventArgs e)
        {
            var segments = CodeEditor.GetDeletableSegments().ToArray();

            if (segments.Length != 3)
            {
                MessageBox.Show("GetFilePath, GetDirPath and at least one watch directory is required.", "Error", MessageBoxButton.OK);
                return;
            }

            var watchDirs = segments[0].Split('\n').Select(_ => _.Trim()).Where(_ => _.Length > 0);

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


            var existing = _managers.FirstOrDefault(_ => _.Info.Name == _viewModel.Name);
            if (existing != null)
            {
                Remove(_viewModel.Name);
            }


            //TODO: add automapper
            _model.Name = _viewModel.Name;
            _model.DestBaseDir = _viewModel.DestBaseDir;
            _model.Dirs = watchDirs.ToList();
            _model.FileDest = segments[1];
            _model.DirDest = segments[2];
            _model.IncludeSubdirectories = _viewModel.IncludeSubdirectories;

            var mananger = new CarpetManager(_model);
            _watchInfos.Add(_model);
            mananger.StartWatch();
            mananger.InitialScan();
            _managers.Add(mananger);

            var comboItem = new ComboBoxItem { Content = _model.Name, DataContext = _model };
            watchInfoList.Add(comboItem);
            WatchInfoCombo.SelectedItem = comboItem;

            _configManager.Save(_watchInfos);
        }

        private string IsNameValid(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "Name required";
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

            var disks = new[] { baseDir.ToLower()[0] }.Concat(watchDirs.Select(_ => _.ToLower()[0])).GroupBy(_ => _);
            if (disks.Count() > 1)
            {
                return "All directories must be on same disk";
            }

            return null;
        }

        private void NewWatch_OnClick(object sender, RoutedEventArgs e)
        {
            _model = CreateWatchInfoNew();

            UpdateViewModel(_model);
            WatchInfoCombo.SelectedItem = null;
        }


        private CarpetWatchInfo CreateWatchInfoNew()
        {
            return new CarpetWatchInfo
            {
                DestBaseDir = string.Empty,
                IncludeSubdirectories = false,
                Name = string.Empty,
                Dirs = new[] { string.Empty, },
                DirDest = "\treturn null;",
                FileDest = "\treturn null;"
            };
        }

        private void Remove(string name)
        {
            var manager = _managers.FirstOrDefault(_ => _.Info.Name == name);
            if (manager != null)
            {
                manager.StopWatch();
                _managers.Remove(manager);
            }

            var config = _watchInfos.FirstOrDefault(_ => _.Name == name);
            if (config != null)
            {
                _watchInfos.Remove(config);
            }

            if (WatchInfoCombo.SelectedItem != null)
            {
                var selected = ((CarpetWatchInfo)((ComboBoxItem)WatchInfoCombo.SelectedItem).DataContext);
                if (selected.Name == name)
                {
                    watchInfoList.Remove((ComboBoxItem)WatchInfoCombo.SelectedItem);
                    if (watchInfoList.Any())
                    {
                        WatchInfoCombo.SelectedItem = watchInfoList.First();
                    }
                    else
                    {
                        WatchInfoCombo.SelectedItem = null;
                    }
                }
            }

            _configManager.Save(_watchInfos);
        }

        private void DirPicker_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _viewModel.DestBaseDir = dialog.SelectedPath;
            }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            var confirmResult = MessageBox.Show("Are you sure to delete this item ?", "Confirm delete", MessageBoxButton.YesNo);
            if (confirmResult == MessageBoxResult.Yes)
            {
                Remove((string)((ComboBoxItem)WatchInfoCombo.SelectedItem).Content);
                InitViewModel(null);
            }
        }
    }
}
