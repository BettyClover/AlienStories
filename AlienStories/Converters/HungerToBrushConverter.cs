using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AlienStories.Converters;

public class HungerToBrushConverter : IValueConverter
{
    public static readonly HungerToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int hunger)
        {
            if (hunger > 70) return new SolidColorBrush(Colors.Green);
            if (hunger > 30) return new SolidColorBrush(Colors.Orange);
            return new SolidColorBrush(Colors.Red);
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}