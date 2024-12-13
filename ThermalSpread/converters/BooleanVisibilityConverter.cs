using System;
using System.Windows;
using System.Windows.Data;

namespace ThermalSpread.converters;

[ValueConversion(typeof(bool), typeof(bool))]
public class BooleanVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        return (bool)value ? Visibility.Visible : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter,
        System.Globalization.CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}