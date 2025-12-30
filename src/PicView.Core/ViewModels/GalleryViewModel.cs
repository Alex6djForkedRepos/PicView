using PicView.Core.Gallery;
using R3;

namespace PicView.Core.ViewModels;

public class GalleryViewModel : IDisposable
{
    public GalleryItemViewModel GalleryItem { get; } = new();

    public BindableReactiveProperty<object> GalleryMargin { get; } = new();

    public BindableReactiveProperty<GalleryMode> GalleryMode { get; } = new(Core.Gallery.GalleryMode.Closed);

    public BindableReactiveProperty<object> GalleryStretch { get; } = new();
    public BindableReactiveProperty<object> GalleryVerticalAlignment { get; } = new();
    public BindableReactiveProperty<object> GalleryOrientation { get; } = new();

    public BindableReactiveProperty<bool> IsGalleryExpanded { get; } = new();
    
    public BindableReactiveProperty<bool> IsBottomGalleryShown { get; } = new(Settings.Gallery.IsBottomGalleryShown);
    public BindableReactiveProperty<bool> IsBottomGalleryShownInHiddenUI { get; } =
        new(Settings.Gallery.ShowBottomGalleryInHiddenUI);

    public void Dispose()
    {
        Disposable.Dispose(GalleryItem,
            GalleryMargin,
            IsBottomGalleryShown,
            IsBottomGalleryShownInHiddenUI,
            GalleryMode,
            GalleryStretch,
            GalleryVerticalAlignment,
            GalleryOrientation,
            IsGalleryExpanded
            );
    }
}