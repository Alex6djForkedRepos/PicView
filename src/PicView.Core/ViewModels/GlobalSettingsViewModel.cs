using R3;

namespace PicView.Core.ViewModels;

public class GlobalSettingsViewModel : IDisposable
{
    public BindableReactiveProperty<bool> IsBottomGalleryShown { get; } = new(Settings.Gallery.IsBottomGalleryShown);
    public BindableReactiveProperty<bool> ShowBottomGalleryInHiddenUI { get; } = new(Settings.Gallery.ShowBottomGalleryInHiddenUI);

    public void Dispose()
    {
        Disposable.Dispose(IsBottomGalleryShown);
    }
}