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
using SharpFileSystem;

namespace RopeSnake.UI.ProjectManager
{
    /// <summary>
    /// Interaction logic for ProjectSettingsEditor.xaml
    /// </summary>
    public partial class ProjectSettingsEditor : UserControl
    {
        public static readonly DependencyProperty SettingsProperty =
            DependencyProperty.Register("Settings", typeof(Mother3ProjectSettings), typeof(ProjectSettingsEditor));

        public Mother3ProjectSettings Settings
        {
            get { return (Mother3ProjectSettings)GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }

        public event EventHandler SettingsApplied;

        public ProjectSettingsEditor()
        {
            InitializeComponent();

            offsetModeBox.ItemsSource = Enum.GetValues(typeof(OffsetTableMode));
        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            FileSystemPath[] paths;

            if (ValidateFields(out paths))
                ApplySettings(paths);
        }

        private bool ValidateFields(out FileSystemPath[] paths)
        {
            bool isValid = true;

            FileSystemPath baseRomPath;
            if (!ValidateTextBox(baseRomBox, out baseRomPath))
                isValid = false;

            FileSystemPath outputRomPath;
            if (!ValidateTextBox(outputRomBox, out outputRomPath))
                isValid = false;

            FileSystemPath romConfigPath;
            if (!ValidateTextBox(romConfigBox, out romConfigPath))
                isValid = false;

            if (!isValid)
            {
                paths = null;
                return false;
            }

            paths = new[] { baseRomPath, outputRomPath, romConfigPath };
            return true;
        }

        private void ApplySettings(FileSystemPath[] paths)
        {
            Settings.BaseRomFile = paths[0].Path;
            Settings.OutputRomFile = paths[1].Path;
            Settings.RomConfigFile = paths[2].Path;
            Settings.OffsetTableMode = (OffsetTableMode)offsetModeBox.SelectedValue;

            OnSettingsApplied();
        }

        private bool ValidateTextBox(TextBox textBox, out FileSystemPath path)
        {
            if (!TryParse(textBox.Text, out path))
            {
                MarkInvalid(textBox);
                return false;
            }

            ClearInvalid(textBox);
            return true;
        }

        private void OnSettingsApplied()
        {
            SettingsApplied?.Invoke(this, null);
        }

        private static void MarkInvalid(TextBox textBox)
        {
            var binding = BindingOperations.GetBindingExpression(textBox, TextBox.TextProperty);
            var bindingBase = BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty);
            var validationError = new ValidationError(new ExceptionValidationRule(), binding);
            Validation.MarkInvalid(binding, validationError);
        }

        private static void ClearInvalid(TextBox textBox)
        {
            var bindingBase = BindingOperations.GetBindingExpressionBase(textBox, TextBox.TextProperty);
            Validation.ClearInvalid(bindingBase);
        }

        private static bool TryParse(string value, out FileSystemPath path)
        {
            if (!FileSystemPath.IsRooted(value))
                goto notValid;

            path = FileSystemPath.Parse(value);

            if (!path.IsFile)
                goto notValid;

            return true;

            notValid:

            path = default(FileSystemPath);
            return false;
        }
    }
}
