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
using Microsoft.Win32;

namespace RopeSnake.UI.Common
{
    /// <summary>
    /// Interaction logic for FilePickerControl.xaml
    /// </summary>
    public partial class FilePicker : UserControl
    {
        public string Path
        {
            get { return (string)fileBox.GetValue(TextBox.TextProperty); }
            set { fileBox.SetValue(TextBox.TextProperty, value); }
        }

        public string Filter
        {
            get { return _dialog.Filter; }
            set { _dialog.Filter = value; }
        }

        private OpenFileDialog _dialog;

        public FilePicker()
        {
            _dialog = new OpenFileDialog();
            InitializeComponent();
        }

        private void dialogButton_Click(object sender, RoutedEventArgs e)
        {
            if (_dialog.ShowDialog() == true)
            {
                Path = _dialog.FileName;
            }
        }
    }
}
