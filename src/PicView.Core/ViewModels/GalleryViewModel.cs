using PicView.Core.Gallery;
using PicView.Core.Models;
using R3;

namespace PicView.Core.ViewModels;

public class GalleryViewModel : IDisposable
{
    public GalleryItemViewModel GalleryItem { get; } = new();

    public BindableReactiveProperty<double> BottomGalleryMargin { get; } = new(0);

    public BindableReactiveProperty<double> GalleryWidth { get; } = new(0);

    public BindableReactiveProperty<bool> IsBottomGalleryShown { get; } = new(Settings.Gallery.IsBottomGalleryShown);

    public BindableReactiveProperty<bool> IsBottomGalleryShownInHiddenUI { get; } =
        new(Settings.Gallery.ShowBottomGalleryInHiddenUI);

    public BindableReactiveProperty<GalleryMode> GalleryMode { get; } = new(Gallery.GalleryMode.Closed);

    public BindableReactiveProperty<CommonModels.Stretch> GalleryStretch { get; } = new();
    public BindableReactiveProperty<CommonModels.VerticalAlignment> GalleryVerticalAlignment { get; } = new();
    public BindableReactiveProperty<CommonModels.Orientation> GalleryOrientation { get; } = new();

    public BindableReactiveProperty<bool> IsGalleryExpanded { get; } = new();

    public ReactiveCommand? ToggleBottomGalleryCommand { get; set; }
    public ReactiveCommand? CloseGalleryCommand { get; set; }
    public ReactiveCommand? GalleryItemStretchCommand { get; set; }

    public void Dispose()
    {
        Disposable.Dispose(BottomGalleryMargin,
            GalleryWidth,
            IsBottomGalleryShown,
            IsBottomGalleryShownInHiddenUI,
            GalleryMode,
            GalleryStretch,
            GalleryVerticalAlignment,
            GalleryOrientation,
            IsGalleryExpanded,
            ToggleBottomGalleryCommand,
            CloseGalleryCommand,
            GalleryItemStretchCommand,
            IsUniformBottomChecked,
            IsUniformFullChecked,
            IsUniformMenuChecked,
            IsUniformToFillBottomChecked,
            IsUniformToFillFullChecked,
            IsUniformToFillMenuChecked,
            IsFillBottomChecked,
            IsFillFullChecked,
            IsFillMenuChecked,
            IsNoneBottomChecked,
            IsNoneFullChecked,
            IsNoneMenuChecked,
            IsSquareBottomChecked,
            IsSquareFullChecked,
            IsSquareMenuChecked,
            IsFillSquareBottomChecked,
            IsFillSquareFullChecked,
            IsFillSquareMenuChecked
            );
    }

    #region Gallery Stretch IsChecked

    public BindableReactiveProperty<bool> IsUniformBottomChecked { get; } = new();

    public BindableReactiveProperty<bool> IsUniformFullChecked { get; } = new();

    public BindableReactiveProperty<bool> IsUniformMenuChecked { get; } = new();

    public BindableReactiveProperty<bool> IsUniformToFillBottomChecked { get; } = new();

    public BindableReactiveProperty<bool> IsUniformToFillFullChecked { get; } = new();

    public BindableReactiveProperty<bool> IsUniformToFillMenuChecked { get; } = new();

    public BindableReactiveProperty<bool> IsFillBottomChecked { get; } = new();

    public BindableReactiveProperty<bool> IsFillFullChecked { get; } = new();

    public BindableReactiveProperty<bool> IsFillMenuChecked { get; } = new();

    public BindableReactiveProperty<bool> IsNoneBottomChecked { get; } = new();

    public BindableReactiveProperty<bool> IsNoneFullChecked { get; } = new();

    public BindableReactiveProperty<bool> IsNoneMenuChecked { get; } = new();

    public BindableReactiveProperty<bool> IsSquareBottomChecked { get; } = new();

    public BindableReactiveProperty<bool> IsSquareFullChecked { get; } = new();

    public BindableReactiveProperty<bool> IsSquareMenuChecked { get; } = new();

    public BindableReactiveProperty<bool> IsFillSquareBottomChecked { get; } = new();

    public BindableReactiveProperty<bool> IsFillSquareFullChecked { get; } = new();

    public BindableReactiveProperty<bool> IsFillSquareMenuChecked { get; } = new();

    #endregion
}