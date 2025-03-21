using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ChatClient.WPF.Behaviors;

namespace ChatClient.WPF.Converters;

public class PasswordStrengthToGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var password = value as string;
        if (string.IsNullOrWhiteSpace(password))
        {
            return new GridLength(0, GridUnitType.Star);
        }
        var passwordStrength = PasswordDifficulty.CheckDifficultyPassword(password);
        switch (passwordStrength)
        {
            case DifficultyPassword.Hard:
                return new GridLength(100, GridUnitType.Star);
            case DifficultyPassword.Medium:
                return new GridLength(60, GridUnitType.Star);
            case DifficultyPassword.Easy:
            default:
                return new GridLength(25, GridUnitType.Star);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}