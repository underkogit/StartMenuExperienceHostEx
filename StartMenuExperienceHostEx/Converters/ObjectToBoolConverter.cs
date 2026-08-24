using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace StartMenuExperienceHostEx.Converters;

public class ObjectToBoolConverter : IValueConverter
{
    public static ObjectToBoolConverter Instance { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter?.ToString()?.ToLower() == "null")
        {
            return value == null;
        }


        return value != null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}