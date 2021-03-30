using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using WPFMVVM.Converter;

namespace YouLessMonitor
{
    public class BoolToColorConverter : BaseConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? Brushes.White : Brushes.Red;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
    }
}