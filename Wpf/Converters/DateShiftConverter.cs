using System;
using System.Globalization;
using System.Windows.Data;

namespace Wpf.Converters
{
    public class DateShiftConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                if (int.TryParse(parameter?.ToString(), out var shift))
                    return dt.AddDays(shift);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? Binding.DoNothing!;
        }
    }
}
