using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusApp.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If True (Focus Active) -> Show "Stop"
            // If False (Focus Inactive) -> Show "Start"
            if (value is bool isActive && isActive)
            {
                return "Stop Focus";
            }
            return "Start Focus";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
