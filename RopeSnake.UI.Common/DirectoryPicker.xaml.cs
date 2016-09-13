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
using Ookii.Dialogs.Wpf;

namespace RopeSnake.UI.Common
{
    /// <summary>
    /// Interaction logic for DirectoryPicker.xaml
    /// </summary>
    public partial class DirectoryPicker : UserControl
    {
        public string Path
        {
            get { return (string)directoryBox.GetValue(TextBox.TextProperty); }
            set { directoryBox.SetValue(TextBox.TextProperty, value); }
        }

        private VistaFolderBrowserDialog _dialog;

        public DirectoryPicker()
        {
            _dialog = new VistaFolderBrowserDialog();
            _dialog.ShowNewFolderButton = true;

            InitializeComponent();
        }

        private void dialogButton_Click(object sender, RoutedEventArgs e)
        {
            if (_dialog.ShowDialog() == true)
            {
                Path = _dialog.SelectedPath;
            }
        }
    }
}
