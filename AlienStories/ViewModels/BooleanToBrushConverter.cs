using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AlienStories.ViewModels;

public class BooleanToBrushConverter : IValueConverter
{
    public static readonly BooleanToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value is true) ? new SolidColorBrush(Color.Parse("#4a6fa5")) : new SolidColorBrush(Color.Parse("#2a2a3e"));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}