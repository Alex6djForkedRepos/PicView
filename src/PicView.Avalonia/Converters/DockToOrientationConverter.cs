using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using PicView.Core.Config;
using PicView.Core.Gallery;

namespace PicView.Avalonia.Converters;

public class DockToOrientationConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GalleryDockPosition dock)
        {
            return (dock == GalleryDockPosition.Left || dock == GalleryDockPosition.Right) 
                ? Orientation.Vertical 
                : Orientation.Horizontal;
        }
        return Orientation.Horizontal;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
