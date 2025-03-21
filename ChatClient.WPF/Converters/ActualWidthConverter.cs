using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatClient.WPF.Converters;

public class ActualWidthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (double)value - 125d;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}