using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Core.Sizing;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Gallery;

public static class GalleryHelper
{
    public static void SetGalleryItemStretch(string value) => SetGalleryItemStretch(value, UIHelper.GetMainView.DataContext as MainViewModel);
    public static void SetGalleryItemStretch(string value, MainViewModel vm)
    {
        if (value.Equals("Square", StringComparison.OrdinalIgnoreCase))
        {
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                GalleryStretchMode.ChangeFullGalleryStretchSquare(vm);
            }
            else
            {
                GalleryStretchMode.ChangeBottomGalleryStretchSquare(vm);
            }

            return;
        }

        if (value.Equals("FillSquare", StringComparison.OrdinalIgnoreCase))
        {
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                GalleryStretchMode.ChangeFullGalleryStretchSquareFill(vm);
            }
            else
            {
                GalleryStretchMode.ChangeBottomGalleryStretchSquareFill(vm);
            }

            return;
        }

        if (Enum.TryParse<Stretch>(value, out var stretch))
        {
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                GalleryStretchMode.ChangeFullGalleryItemStretch(vm, stretch);
            }
            else
            {
                GalleryStretchMode.ChangeBottomGalleryItemStretch(vm, stretch);
            }
        }
    }
    
    public static (double width, double height) GetGallerySize(MainWindowViewModel main)
    {
        if (!Settings.Gallery.IsGalleryDocked || Slideshow.IsRunning || 
            !Settings.Gallery.ShowBottomGalleryInHiddenUI && !main.IsUIShown.CurrentValue)
        {
            return (0, 0);
        }

        Rect galleryBounds;
        if (main.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            galleryBounds = imageViewer.GalleryView.Bounds;
        }
        else
        {
            return (0, 0);
        }

        if (main.WindowTabs.ActiveTab.CurrentValue.Gallery.IsLeftDocked.CurrentValue || 
            main.WindowTabs.ActiveTab.CurrentValue.Gallery.IsRightDocked.CurrentValue)
        {
            return (galleryBounds.Width, 0);
        }

        return (0, galleryBounds.Height);
    }
}