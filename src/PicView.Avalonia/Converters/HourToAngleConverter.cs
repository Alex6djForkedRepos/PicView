using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace PicView.Avalonia.Converters;

public class HourToAngleConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int hour)
        {
            return 0d;
        }

        // Map 1 → 30°, 2 → 60°, ..., 12 → 360°
        var angle = hour % 12 * 30.0;

        if (parameter is string s && s.Equals("Inverse", StringComparison.OrdinalIgnoreCase))
            return -angle; // undo rotation to keep text upright

        return angle;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
