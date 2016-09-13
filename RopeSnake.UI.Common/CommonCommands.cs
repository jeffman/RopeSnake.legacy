using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RopeSnake.UI.Common
{
    public static class CommonCommands
    {
        public static RoutedCommand Open { get; } = new RoutedCommand();
        public static RoutedCommand Reopen { get; } = new RoutedCommand();
        public static RoutedCommand Close { get; } = new RoutedCommand();
        public static RoutedCommand Exit { get; } = new RoutedCommand();
    }
}
