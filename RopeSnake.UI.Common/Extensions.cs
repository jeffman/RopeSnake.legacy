using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RopeSnake.UI.Common
{
    public static class Extensions
    {
        public static T GetDescendantByType<T>(this DependencyObject element)
            where T : DependencyObject
        {
            if (element == null)
                return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                var childT = child as T;
                if (childT != null)
                    return childT;

                var next = child.GetDescendantByType<T>();
                if (next != null)
                    return next;
            }

            return null;
        }
    }
}
