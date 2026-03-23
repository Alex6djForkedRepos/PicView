using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using PicView.Core.Config;
using PicView.Core.Gallery;

namespace PicView.Avalonia.Converters;

public class DockConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GalleryDockPosition position)
        {
            return (Dock)(int)position;
        }
        return Dock.Bottom;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Dock dock)
        {
             return (GalleryDockPosition)(int)dock;
        }
        return GalleryDockPosition.Bottom;
    }
}
