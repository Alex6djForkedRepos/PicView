using PicView.Core.ViewModels;

namespace PicView.Core.Gallery;

public static class GalleryStretchService
{
    public static void SetStretch(GalleryViewModel gallery, string mode)
    {
        Settings.Gallery.BottomGalleryStretchMode = mode;
        
        // Determine the effective Stretch value for the Image control
        string stretchValue;
        var isSquare = false;

        if (string.Equals(mode, "Square", StringComparison.OrdinalIgnoreCase))
        {
            stretchValue = "Uniform";
            isSquare = true;
        }
        else if (string.Equals(mode, "FillSquare", StringComparison.OrdinalIgnoreCase))
        {
            stretchValue = "Fill";
            isSquare = true;
        }
        else
        {
            stretchValue = mode;
        }

        gallery.GalleryStretch.Value = stretchValue;

        // Update width for all existing items
        if (gallery.GalleryItems.Value is not { } items)
        {
            return;
        }

        foreach (var item in items)
        {
            item.ItemWidth.Value = isSquare ? item.ItemHeight.Value : double.NaN;
        }
    }
}
