using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;

namespace PicView.Avalonia.Views.UC;

public partial class BottomGalleryItemSizeSlider : UserControl
{
    public BottomGalleryItemSizeSlider()
    {
        InitializeComponent();
    }
    private void BottomGallery_OnValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (DataContext is not MainViewModel vm )
        {
            return;
        }
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (vm.Gallery.GalleryItem.BottomGalleryItemHeight.CurrentValue == e.NewValue)
        {
            return;
        }
        vm.Gallery.GalleryItem.BottomGalleryItemHeight.Value = e.NewValue;
        
        if (Settings.Gallery.IsBottomGalleryShown && !GalleryFunctions.IsFullGalleryOpen)
        {
            vm.Gallery.GalleryItem.ItemHeight.Value= e.NewValue;
            UIHelper.GetGalleryView.Height = GalleryFunctions.GetGalleryHeight(vm);
            WindowResizing.SetSize(vm);
        }
        
        // Binding to height depends on timing of the update. Maybe find a cleaner mvvm solution one day
        // Maybe save this on close or some other way
        Settings.Gallery.BottomGalleryItemSize = e.NewValue;
    }
}