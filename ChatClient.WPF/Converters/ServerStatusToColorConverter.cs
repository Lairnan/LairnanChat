using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace ChatClient.WPF.Converters;

public class ServerStatusToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isConnected)
        {
            return isConnected ? Color.Green : Color.Red;
        }
        return Color.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => Binding.DoNothing;
}
