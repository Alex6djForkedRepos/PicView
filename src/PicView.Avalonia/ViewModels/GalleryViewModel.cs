using Avalonia;
using PicView.Avalonia.Functions;
using PicView.Avalonia.Gallery;
using PicView.Core.Gallery;
using PicView.Core.Models;
using R3;

namespace PicView.Avalonia.ViewModels;

public class GalleryViewModel : IDisposable
{
    public GalleryItemViewModel GalleryItem { get; } = new();

    public BindableReactiveProperty<Thickness> GalleryMargin { get; } = new();

    public BindableReactiveProperty<double> GalleryWidth { get; } = new(0);

    public BindableReactiveProperty<bool> IsBottomGalleryShown { get; } = new(Settings.Gallery.IsBottomGalleryShown);

    public BindableReactiveProperty<bool> IsBottomGalleryShownInHiddenUI { get; } =
        new(Settings.Gallery.ShowBottomGalleryInHiddenUI);

    public BindableReactiveProperty<GalleryMode> GalleryMode { get; } = new(Core.Gallery.GalleryMode.Closed);

    public BindableReactiveProperty<CommonModels.Stretch> GalleryStretch { get; } = new();
    public BindableReactiveProperty<CommonModels.VerticalAlignment> GalleryVerticalAlignment { get; } = new();
    public BindableReactiveProperty<CommonModels.Orientation> GalleryOrientation { get; } = new();

    public BindableReactiveProperty<bool> IsGalleryExpanded { get; } = new();
    
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

    #region Commands
    public required ReactiveCommand ToggleGalleryCommand { get; init; } = new(ToggleGallery);
    public required ReactiveCommand ToggleBottomGalleryCommand { get; init; } = new(ToggleBottomGallery);
    public required ReactiveCommand CloseGalleryCommand { get; init; } = new(CloseGallery);
    public required ReactiveCommand<string> GalleryItemStretchCommand { get; init; } = new(GalleryItemStretch);

    private static void ToggleGallery(Unit unit) => FunctionsMapper.ToggleGallery();
    private static void ToggleBottomGallery(Unit unit) => FunctionsMapper.OpenCloseBottomGallery();
    private static void CloseGallery(Unit unit) => FunctionsMapper.CloseGallery();
    private static void GalleryItemStretch(string value) => GalleryHelper.SetGalleryItemStretch(value);
    
    #endregion

    public void Dispose()
    {
        Disposable.Dispose(GalleryItem,
            GalleryMargin,
            GalleryWidth,
            IsBottomGalleryShown,
            IsBottomGalleryShownInHiddenUI,
            GalleryMode,
            GalleryStretch,
            GalleryVerticalAlignment,
            GalleryOrientation,
            IsGalleryExpanded,
            ToggleGalleryCommand,
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
}