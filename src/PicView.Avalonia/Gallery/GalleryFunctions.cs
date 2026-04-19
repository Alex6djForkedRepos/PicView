using Avalonia.Layout;
using Avalonia.Svg.Skia;
using Avalonia.Threading;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Core.DebugTools;
using PicView.Core.Gallery;
using PicView.Core.Localization;
using PicView.Core.Sizing;
using GalleryItem = PicView.Avalonia.Views.Gallery.GalleryItem;

namespace PicView.Avalonia.Gallery;

// TODO deprecated, delete
public static class GalleryFunctions
{
    public static double GetGalleryHeight(MainViewModel vm)
    {
        return 0;
    }

    public static bool RenameGalleryItem(int oldIndex, int newIndex, string newFileLocation, string newName)
    {
        return false;
    }

    public static bool RemoveGalleryItem(int index, MainViewModel? vm)
    {
       return false;
    }

    public static async Task<bool> AddGalleryItem(int index, FileInfo fileInfo, MainViewModel? vm,
        DispatcherPriority? priority = null)
    {
            return false;
    }

    public static void CenterGallery(MainViewModel vm)
    {
    }

    #region Gallery toggle

    public static bool IsFullGalleryOpen { get; private set; }

    public static void ToggleGallery(MainViewModel vm)
    {
    }

    public static void OpenCloseBottomGallery(MainViewModel vm)
    {
    }

    public static void OpenBottomGallery(MainViewModel vm)
    {
    }

    public static void CloseGallery(MainViewModel vm)
    {
    }

    #endregion
}