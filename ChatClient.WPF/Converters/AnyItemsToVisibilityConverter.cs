using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ChatClient.WPF.Converters;

public class AnyItemsToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!(value is int count))
            return Visibility.Collapsed;

        return count > 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}