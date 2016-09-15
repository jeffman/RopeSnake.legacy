using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using System.Reflection;

namespace RopeSnake.UI.ProjectManager
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        private ObservableCollection<ThirdPartyInfo> _thirdPartyList;

        public AboutWindow()
        {
            InitializeComponent();
            InitializeLabels();
            InitializeThirdParty();
        }

        private void InitializeLabels()
        {
            nameLabel.Content = $"RopeSnake {Versions.VersionInformation.RopeSnakeVersion}";
            copyrightLabel.Content = Versions.VersionInformation.Copyright;
            licenseLabel.Content = "MIT license";
            githubLabel.Text = "https://github.com/jeffman/RopeSnake";
        }

        private void InitializeThirdParty()
        {
            _thirdPartyList = new ObservableCollection<ThirdPartyInfo>();

            foreach(var type in new[] {
                typeof(NLog.Logger),
                typeof(Newtonsoft.Json.JsonConvert),
                typeof(SharpFileSystem.FileSystemPath),
                typeof(Xceed.Wpf.Toolkit.NumericUpDown<>),
                typeof(Ookii.Dialogs.Wpf.VistaOpenFileDialog) })
            {
                _thirdPartyList.Add(new ThirdPartyInfo(type));
            }

            _thirdPartyList.Add(new ThirdPartyInfo(
                "Fugue Icons", "(c) 2015 Yusuke Kamiyamane", "3.5.6", "http://p.yusukekamiyamane.com/", "Creative Commons Attribution License 3.0"));

            thirdPartyList.ItemsSource = _thirdPartyList;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static IEnumerable<AssemblyName> GetAssemblyNames(params Type[] types)
        {
            foreach (var type in types)
                yield return Assembly.GetAssembly(type).GetName();
        }
    }

    class ThirdPartyInfo
    {
        public string Title { get; }
        public string Copyright { get; }
        public string Version { get; }
        public string Url { get; }
        public string License { get; }

        public string Description => GenerateDescription();

        public ThirdPartyInfo(string title, string copyright, string version, string url, string license)
        {
            Title = title;
            Copyright = copyright;
            Version = version;
            Url = url;
            License = license;
        }

        public ThirdPartyInfo(Type typeFromAssembly)
        {
            var assembly = typeFromAssembly.Assembly;
            var assemblyName = assembly.GetName();

            Title = assemblyName.Name;
            Version = assemblyName.Version.ToString();
            Copyright = assembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
        }

        private string GenerateDescription()
        {
            var sb = new StringBuilder();

            if (Title != null)
            {
                sb.Append(Title);
                if (Version != null)
                    sb.Append(" " + Version);
                sb.AppendLine();
            }

            if (Copyright != null)
                sb.AppendLine(Copyright);

            if (License != null)
                sb.AppendLine(License);

            if (Url != null)
                sb.AppendLine(Url);

            return sb.ToString();
        }
    }
}
