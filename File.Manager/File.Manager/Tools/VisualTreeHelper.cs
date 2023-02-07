using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace File.Manager.Tools
{
    public static class VisualTreeTools
    {
        public static T VisualUpwardSearch<T>(DependencyObject source)
            where T : FrameworkElement
        {
            while (source != null && !(source is T))
            {
                if (source is Visual)
                    source = VisualTreeHelper.GetParent(source);
                else if (source is FrameworkContentElement element)
                {
                    source = element.Parent;
                }
            }

            return source as T;
        }
    }
}
