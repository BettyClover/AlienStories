using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AlienStories.Converters;

public class AlbumColorConverter : IValueConverter
{
    public static readonly AlbumColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isCaptured && isCaptured)
        {
            return "#FFA500";
        }
        return "#1a1a2e";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}