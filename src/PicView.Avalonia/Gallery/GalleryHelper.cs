using PicView.Avalonia.Navigation;
using PicView.Avalonia.Views.UC;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Gallery;

public static class GalleryHelper
{
    public static (double width, double height) GetGallerySize(MainWindowViewModel vm)
    {
        var tabs = vm.WindowTabs;
        var tab = tabs.ActiveTab.CurrentValue;
        var gallery = tab.Gallery;
        if (!Settings.Gallery.IsGalleryDocked || Slideshow.IsRunning || gallery.IsGalleryExpanded.CurrentValue ||
            !Settings.Gallery.ShowDockedGalleryInHiddenUI && !vm.IsUIShown.CurrentValue)
        {
            return (0, 0);
        }
        
        if (gallery.IsLeftDocked.CurrentValue || gallery.IsRightDocked.CurrentValue)
        {
            return (vm.GallerySettings.DockedGalleryItemSize.CurrentValue, 0);
        }
        return (0, vm.GallerySettings.DockedGalleryItemSize.CurrentValue);
    }

    public static void CenterGallery(MainWindowViewModel main)
    {
        if (main.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is not ImageViewer imageViewer)
        {
            return;
        }

        imageViewer.GalleryView.GalleryItemsControl.ScrollToCenterOfCurrentItem();
    }
}