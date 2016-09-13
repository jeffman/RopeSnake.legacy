using NLog;
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
using System.Globalization;

namespace RopeSnake.UI.Common
{
    /// <summary>
    /// Interaction logic for LogLevelIcon.xaml
    /// </summary>
    public partial class LogLevelIcon : UserControl
    {
        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register("Level", typeof(LogLevel), typeof(LogLevelIcon));

        public LogLevel Level
        {
            get { return (LogLevel)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        public LogLevelIcon()
        {
            InitializeComponent();
        }
    }

    public class LevelIconConverter : IValueConverter
    {
        private static Dictionary<LogLevel, ImageSource> _levelToIcon;

        static LevelIconConverter()
        {
            _levelToIcon = new Dictionary<LogLevel, ImageSource>();

            _levelToIcon[LogLevel.Trace] = null;
            _levelToIcon[LogLevel.Debug] = new BitmapImage(new Uri("pack://application:,,,/RopeSnake.UI.Common;component/Resources/information-white.png"));
            _levelToIcon[LogLevel.Info] = new BitmapImage(new Uri("pack://application:,,,/RopeSnake.UI.Common;component/Resources/information.png"));
            _levelToIcon[LogLevel.Warn] = new BitmapImage(new Uri("pack://application:,,,/RopeSnake.UI.Common;component/Resources/exclamation.png"));
            _levelToIcon[LogLevel.Error] = new BitmapImage(new Uri("pack://application:,,,/RopeSnake.UI.Common;component/Resources/cross-circle.png"));
            _levelToIcon[LogLevel.Fatal] = new BitmapImage(new Uri("pack://application:,,,/RopeSnake.UI.Common;component/Resources/cross-circle.png"));
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            return _levelToIcon[value as LogLevel];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
