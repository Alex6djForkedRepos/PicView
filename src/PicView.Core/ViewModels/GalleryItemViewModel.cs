using PicView.Core.Gallery;
using R3;

namespace PicView.Core.ViewModels;

public class GalleryItemViewModel
{
    public BindableReactiveProperty<double> ItemWidth { get; } = new(0);
    public BindableReactiveProperty<double> ItemHeight { get; } = new(0);

    public BindableReactiveProperty<double> ItemMargin { get; } = new();

    public BindableReactiveProperty<double> ExpandedGalleryItemWidth { get; } = new(0);

    public double MaxExpandedGalleryItemHeight => GalleryDefaults.MaxFullGalleryItemHeight;
    public double MinExpandedGalleryItemHeight => GalleryDefaults.MinFullGalleryItemHeight;

    public double MaxGalleryItemHeight => GalleryDefaults.MaxBottomGalleryItemHeight;
    public double MinGalleryItemHeight => GalleryDefaults.MinBottomGalleryItemHeight;
}