using PicView.Avalonia.Gallery;
using PicView.Avalonia.ViewModels;
using PicView.Core.Gallery;

namespace PicView.Avalonia.Navigation.Services;

public class AvaloniaGalleryService : IGalleryService
{
    private readonly MainViewModel _vm;

    public AvaloniaGalleryService(MainViewModel vm)
    {
        _vm = vm;
    }

    public Task InitialLoadAsync(string folderPath, CancellationToken ct)
    {
        return Task.Run(() => GalleryLoad.LoadGallery(_vm, folderPath), ct);
    }

    public Task ReloadAsync(string folderPath, CancellationToken ct)
    {
        // GalleryLoad.CheckAndReloadGallery handles logic, or direct LoadGallery
        return Task.Run(() => GalleryLoad.LoadGallery(_vm, folderPath), ct);
    }

    public void Close()
    {
        GalleryFunctions.CloseGallery(_vm);
    }

    public void OpenFullscreen()
    {
        if (!GalleryFunctions.IsFullGalleryOpen)
        {
            GalleryFunctions.ToggleGallery(_vm);
        }
    }

    public void OpenDocked()
    {
        GalleryFunctions.OpenBottomGallery(_vm);
    }

    public void Toggle()
    {
        GalleryFunctions.ToggleGallery(_vm);
    }

    public GalleryState State
    {
        get
        {
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                return GalleryState.Fullscreen;
            }

            if (Settings.Gallery.IsBottomGalleryShown)
            {
                return GalleryState.Docked;
            }

            return GalleryState.Closed;
        }
    }
}