using System;
using System.Collections.Generic;
using System.Globalization;
using FocusApp.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusApp.Converters
{
    public class TypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ItemType type)
            {
                // Material Symbols: e80b = Public (Globe), e325 = Smartphone
                return type == ItemType.Website ? "\ue80b" : "\ue325";
            }
            return "\ue888"; // Question mark
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
