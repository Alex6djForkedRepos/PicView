using Avalonia;
using PicView.Core.Gallery;
using R3;

namespace PicView.Avalonia.ViewModels;

public class GalleryItemViewModel : IDisposable
{
    public void Dispose()
    {
        Disposable.Dispose(ItemWidth,
            ItemHeight,
            ItemMargin,
            ExpandedGalleryItemWidth,
            ExpandedGalleryItemHeight,
            BottomGalleryItemWidth,
            BottomGalleryItemHeight);
    }

    public BindableReactiveProperty<double> ItemWidth { get; } = new(0);
    public BindableReactiveProperty<double> ItemHeight { get; } = new(0);

    public BindableReactiveProperty<Thickness> ItemMargin { get; } = new();

    public BindableReactiveProperty<double> ExpandedGalleryItemWidth { get; } = new(0);
    public BindableReactiveProperty<double> ExpandedGalleryItemHeight { get; } = new(0);
    
    public BindableReactiveProperty<double> BottomGalleryItemWidth { get; } = new(0);
    public BindableReactiveProperty<double> BottomGalleryItemHeight { get; } = new(0);

    public double MaxExpandedGalleryItemHeight => GalleryDefaults.MaxFullGalleryItemHeight;
    public double MinExpandedGalleryItemHeight => GalleryDefaults.MinFullGalleryItemHeight;

    public double MaxGalleryItemHeight => GalleryDefaults.MaxBottomGalleryItemHeight;
    public double MinGalleryItemHeight => GalleryDefaults.MinBottomGalleryItemHeight;
}