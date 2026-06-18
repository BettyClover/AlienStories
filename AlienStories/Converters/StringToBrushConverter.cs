using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AlienStories.Converters;

public class StringToBrushConverter : IValueConverter
{
    public static readonly StringToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex)
            return new SolidColorBrush(Color.Parse(hex));
        return Brushes.Gray;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}