using PicView.Core.ViewModels;

namespace PicView.Core.Gallery;

public static class GallerySettingsConverter
{
    public static void UpdateDockPositionProperties(GallerySharedSettingsViewModel gallerySettings)
    {
        var pos = Settings.Gallery.DockPosition;
        gallerySettings.IsTopDocked.Value = pos == GalleryDockPosition.Top;
        gallerySettings.IsBottomDocked.Value = pos == GalleryDockPosition.Bottom;
        gallerySettings.IsLeftDocked.Value = pos == GalleryDockPosition.Left;
        gallerySettings.IsRightDocked.Value = pos == GalleryDockPosition.Right;
    }
}