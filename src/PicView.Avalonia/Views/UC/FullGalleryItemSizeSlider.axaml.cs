using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;

namespace PicView.Avalonia.Views.UC;

public partial class FullGalleryItemSizeSlider : UserControl
{
    public FullGalleryItemSizeSlider()
    {
        InitializeComponent();
    }
    private void FullGallery_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.CurrentValue == e.NewValue)
        {
            return;
        }
        vm.Gallery.GalleryItem.ExpandedGalleryItemHeight.Value = e.NewValue;
        if (GalleryFunctions.IsFullGalleryOpen)
        {
            vm.Gallery.GalleryItem.ItemHeight.Value = e.NewValue;
            WindowResizing.SetSize(vm);
        }
        // Binding to height depends on timing of the update. Maybe find a cleaner mvvm solution one day
        
        // Maybe save this on close or some other way
        Settings.Gallery.ExpandedGalleryItemSize = e.NewValue;
    }
}