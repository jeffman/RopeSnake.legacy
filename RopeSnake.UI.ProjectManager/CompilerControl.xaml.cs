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
using RopeSnake.Mother3;
using RopeSnake.Core;
using SharpFileSystem;
using NLog;

namespace RopeSnake.UI.ProjectManager
{
    /// <summary>
    /// Interaction logic for CompilerControl.xaml
    /// </summary>
    public partial class CompilerControl : UserControl
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        public static readonly DependencyProperty ProjectProperty =
            DependencyProperty.Register("Project", typeof(Mother3Project), typeof(CompilerControl));

        public static DependencyProperty FileSystemProperty =
            DependencyProperty.Register("FileSystem", typeof(IFileSystemWrapper), typeof(CompilerControl));

        public static DependencyProperty IsBusyProperty =
            DependencyProperty.Register("IsBusy", typeof(bool), typeof(CompilerControl));

        public static DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(IProgress<ProgressPercent>), typeof(CompilerControl));

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

        public CompilerControl()
        {
            InitializeComponent();
        }

        private async void ValidateHandler(object sender, RoutedEventArgs e)
        {
            await Validate();
        }

        private async void CompileHandler(object sender, RoutedEventArgs e)
        {
            bool result = await Validate();
            if (!result)
                return;

            await Compile();
        }

        private async Task<bool> Validate()
        {
            IsBusy = true;

            try
            {
                var project = Project;

                bool result = await Task.Run(() => project.Validate());

                if (!result)
                {
                    MessageBox.Show("There were errors during validation. Check the log for errors, fix them, and reload the project before retrying.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    return false;
                }

                return true;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task Compile()
        {
            IsBusy = true;

            try
            {
                bool useCache = cacheBox.IsChecked.Value;
                int maxThreads = maxThreadsBox.Value.Value;
                var fileSystem = FileSystem;
                var project = Project;
                var progress = Progress;

                await Task.Run(() => project.Compile(fileSystem, useCache, maxThreads, progress));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"There was an error while compiling. Reason:{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                _log.Error(ex, "Compiler error");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
