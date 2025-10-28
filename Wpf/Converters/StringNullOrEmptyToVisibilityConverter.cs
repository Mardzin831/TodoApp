using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Wpf.Converters
{
    // Konwerter: pusty tekst -> Visible, inaczej Collapsed
    public class StringNullOrEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as string;
            return string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
