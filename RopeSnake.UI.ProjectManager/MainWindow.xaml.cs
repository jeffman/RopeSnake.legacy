using Microsoft.Win32;
using NLog;
using RopeSnake.Core;
using RopeSnake.Mother3;
using SharpFileSystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace RopeSnake.UI.ProjectManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region DependencyProperties

        public static readonly DependencyProperty ProjectProperty =
            DependencyProperty.Register("Project", typeof(Mother3Project), typeof(MainWindow));

        public static readonly DependencyProperty FileSystemProperty =
            DependencyProperty.Register("FileSystem", typeof(IFileSystemWrapper), typeof(MainWindow));

        public static DependencyProperty ProjectPathProperty =
            DependencyProperty.Register("ProjectPath", typeof(FileSystemPath), typeof(MainWindow));

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register("IsLoading", typeof(bool), typeof(MainWindow));

        public static readonly DependencyProperty IsSavingProperty =
            DependencyProperty.Register("IsSaving", typeof(bool), typeof(MainWindow));

        public Mother3Project Project
        {
            get { return (Mother3Project)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public IFileSystemWrapper FileSystem
        {
            get { return (IFileSystemWrapper)GetValue(FileSystemProperty); }
            set { SetValue(FileSystemProperty, value); }
        }

        public FileSystemPath ProjectPath
        {
            get { return (FileSystemPath)GetValue(ProjectPathProperty); }
            set { SetValue(ProjectPathProperty, value); }
        }

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public bool IsSaving
        {
            get { return (bool)GetValue(IsSavingProperty); }
            set { SetValue(IsSavingProperty, value); }
        }

        #endregion

        private OpenFileDialog _openProjectDialog;

        private object _renderProgressLock = new object();
        private Progress<ProgressPercent> _progress;
        private bool _renderProgressRequested = false;

        public ObservableCollection<LogLevel> LogFilters { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeUI();

            DataContext = this;
        }

        private void InitializeUI()
        {
            _openProjectDialog = new OpenFileDialog();
            _openProjectDialog.Filter = "Project files (*.json)|*.json";

            Title = $"RopeSnake {Assembly.GetExecutingAssembly().GetName().Version}";

            _progress = new Progress<ProgressPercent>(ProgressHandler);
            progressBar.IsVisibleChanged += Progress_IsVisibleChanged;
            compilerControl.Progress = _progress;
            decompilerControl.Progress = _progress;

            LogFilters = new ObservableCollection<LogLevel>();

            foreach (var level in LogLevel.AllLoggingLevels
                .Where(l => l >= LogLevel.Debug)
                .OrderByDescending(l => l))
            {
                var levelMenu = new MenuItem();
                levelMenu.Name = level.ToString().ToLower() + "FilterMenu";
                levelMenu.Header = level.ToString();
                levelMenu.IsCheckable = true;
                levelMenu.Checked += (s, e) => LogFilterMenu_Checked(s, e, level);
                levelMenu.Unchecked += (s, e) => LogFilterMenu_UnChecked(s, e, level);

                if (level >= LogLevel.Info)
                {
                    levelMenu.IsChecked = true;
                }

                logFiltersMenu.Items.Add(levelMenu);
            }
        }

        #region Progress/status

        private void Progress_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool visible = (bool)e.NewValue;
            if (visible)
            {
                ResetStatus();
                ResetProgress();
                CompositionTarget.Rendering += StatusProgress_Render;
            }
            else
            {
                CompositionTarget.Rendering -= StatusProgress_Render;
                _renderProgressRequested = false;
            }
        }

        private void StatusProgress_Render(object sender, EventArgs e)
        {
            lock (_renderProgressLock)
            {
                _renderProgressRequested = true;
            }
        }

        private void ResetStatus()
        {
            SetStatus("");
        }

        private void ResetProgress()
        {
            SetProgress(0);
        }

        private void SetStatus(string message)
        {
            statusLabel.Content = message;
        }

        private void SetProgress(float percent)
        {
            progressBar.Value = percent;
        }

        private void ProgressHandler(ProgressPercent progress)
        {
            if (_renderProgressRequested)
            {
                _renderProgressRequested = false;
                SetStatus(progress.Message);
                SetProgress(progress.Percent);
            }
        }

        #endregion

        private void PrepareFileSystem(string physicalPath)
        {
            string fullPath = Path.GetFullPath(physicalPath);
            string directory = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileName(fullPath);

            FileSystem = new PhysicalFileSystemWrapper(directory);
            ProjectPath = FileSystemPath.Root.AppendFile(fileName);
        }

        private async Task<bool> LoadProject(string Ou)
        {
            char[] a=Ou.ToCharArray(0, Ou.LastIndexOf('\\'));
            string Out = new string(a);
            Out += "\\";
            try
            {
                var fileSystem = FileSystem;
                var projectPath = ProjectPath;
                Project = await Task.Run(() => Mother3Project.Load(fileSystem, projectPath, _progress, Out));
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was an error loading the project. Reason:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                _log.Error(ex, "Project load error");
                return false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SaveProjectSettings()
        {
            Project.ProjectSettings.Save(FileSystem, ProjectPath);
        }

        private void SelectTab(TabItem tab)
        {
            Dispatcher.Invoke(() => compileTab.IsSelected = true);
        }

        #region Command/event handlers

        private async void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_openProjectDialog.ShowDialog() == true)
            {
                PrepareFileSystem(_openProjectDialog.FileName);
                bool result = await LoadProject(_openProjectDialog.FileName);
                if (result)
                    SelectTab(compileTab);
            }
        }

        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = openMenu.IsEnabled;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Project = null;
            FileSystem = null;
            ProjectPath = default(FileSystemPath);
        }

        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = closeMenu.IsEnabled;
        }

        private void ProjectSettingsApplied(object sender, EventArgs e)
        {
            SaveProjectSettings();
        }

        private void ClearLogMenu_Clicked(object sender, RoutedEventArgs e)
        {
            Common.LogViewer.Target.Logs.Clear();
        }

        private void LogFilterMenu_Checked(object sender, EventArgs e, LogLevel level)
        {
            if (!LogFilters.Contains(level))
                LogFilters.Add(level);
            logViewer.FiltersView.Refresh();
        }

        private void LogFilterMenu_UnChecked(object sender, EventArgs e, LogLevel level)
        {
            if (LogFilters.Contains(level))
                LogFilters.Remove(level);
            logViewer.FiltersView.Refresh();
        }

        private void decompilerControl_Decompiled(object sender, EventArgs e)
        {
            SelectTab(compileTab);
        }

        private void aboutMenu_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        #endregion
    }
}
