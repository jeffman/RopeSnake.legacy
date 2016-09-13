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
using RopeSnake.Core;
using NLog;
using NLog.Config;
using NLog.Layouts;
using System.Windows.Controls.Primitives;
using System.Text.RegularExpressions;
using System.ComponentModel;

namespace RopeSnake.UI.Common
{
    /// <summary>
    /// Interaction logic for LogViewer.xaml
    /// </summary>
    public partial class LogViewer : UserControl
    {
        public static MemoryTarget Target { get; }
        private static readonly string LogName = "logViewer";

        static LogViewer()
        {
            if (LogManager.Configuration == null)
            {
                LogManager.Configuration = new LoggingConfiguration();
            }

            Target = new MemoryTarget
            {
                Name = LogName
            };

            var rule = new LoggingRule("*", LogLevel.Debug, Target);
            LogManager.Configuration.LoggingRules.Add(rule);
            LogManager.Configuration.AddTarget(Target);
            LogManager.Configuration.Reload();
        }

        public static readonly DependencyProperty FiltersProperty =
            DependencyProperty.Register("Filters", typeof(ICollection<LogLevel>), typeof(LogViewer));

        public ICollection<LogLevel> Filters
        {
            get { return (ICollection<LogLevel>)GetValue(FiltersProperty); }
            set { SetValue(FiltersProperty, value); }
        }

        public ICollectionView FiltersView => CollectionViewSource.GetDefaultView(grid.ItemsSource);

        public LogViewer()
        {
            InitializeComponent();
            grid.LoadingRow += (s, e) => grid.ScrollIntoView(e.Row.Item);
        }

        private void collectionViewSource_Filter(object sender, FilterEventArgs e)
        {
            if (Filters == null)
                e.Accepted = true;

            e.Accepted = Filters.Contains(((LogEventInfo)e.Item).Level);
        }
    }
}
