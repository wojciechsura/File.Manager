using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace File.Manager.Common.Wpf.Converters
{
    public class RadioToEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is Enum))
                return Binding.DoNothing;

            if (Object.Equals(value, parameter))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool bValue && bValue)
                return parameter;
            else
                return Binding.DoNothing;
        }
    }
}
