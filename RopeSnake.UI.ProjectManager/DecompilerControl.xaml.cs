using RopeSnake.Core;
using RopeSnake.Mother3;
using System;
using System.Collections.Generic;
using System.Linq;
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
using NLog;
using SharpFileSystem;

namespace RopeSnake.UI.ProjectManager
{
    /// <summary>
    /// Interaction logic for DecompilerControl.xaml
    /// </summary>
    public partial class DecompilerControl : UserControl
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public static readonly DependencyProperty ProjectProperty =
            DependencyProperty.Register("Project", typeof(Mother3Project), typeof(DecompilerControl));

        public static DependencyProperty FileSystemProperty =
            DependencyProperty.Register("FileSystem", typeof(IFileSystemWrapper), typeof(DecompilerControl));

        public static DependencyProperty ProjectPathProperty =
            DependencyProperty.Register("ProjectPath", typeof(FileSystemPath), typeof(DecompilerControl));

        public static DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool), typeof(DecompilerControl));

        public static DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(IProgress<ProgressPercent>), typeof(DecompilerControl));

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

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            set { SetValue(IsBusyProperty, value); }
        }

        public IProgress<ProgressPercent> Progress
        {
            get { return (IProgress<ProgressPercent>)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public event EventHandler Decompiled;

        public DecompilerControl()
        {
            InitializeComponent();
        }

        private async void decompileButton_Click(object sender, RoutedEventArgs e)
        {
            await Decompile();
        }

        private async Task Decompile()
        {
            IsBusy = true;

            try
            {
                string baseRom = baseRomPath.Path;
                string romConfig = romConfigPath.Path;
                string outputFolder = outputFolderPath.Path;
                var progress = Progress;

                var project = await Task.Run(() => Mother3Project.CreateNew(baseRom, romConfig, outputFolder, progress));
                var fileSystem = new PhysicalFileSystemWrapper(outputFolder);
                await Task.Run(() => project.Decompile(fileSystem, progress));

                var projectPath = Mother3Project.DefaultProjectFile;
                await Task.Run(() => project.Save(fileSystem, projectPath));

                Project = project;
                FileSystem = fileSystem;
                ProjectPath = projectPath;

                OnDecompiled();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was an error while decompiling. Reason:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                _log.Error(ex, "Decompiler error");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void OnDecompiled()
        {
            Decompiled?.Invoke(this, null);
        }
    }
}
