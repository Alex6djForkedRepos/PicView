using Avalonia.Media;
using PicView.Avalonia.ViewModels;

namespace PicView.Avalonia.Gallery;
public static class GalleryStretchMode
{
    public static void DetermineStretchMode(MainViewModel vm)
    {
        // Reset all boolean properties
        vm.Gallery.IsUniformMenuChecked.Value = false;
        vm.Gallery.IsUniformBottomChecked.Value = false;
        vm.Gallery.IsUniformFullChecked.Value = false;
        
        vm.Gallery.IsUniformToFillMenuChecked.Value = false;
        vm.Gallery.IsUniformToFillBottomChecked.Value = false;
        vm.Gallery.IsUniformToFillFullChecked.Value = false;
        
        vm.Gallery.IsFillMenuChecked.Value = false;
        vm.Gallery.IsFillBottomChecked.Value = false;
        vm.Gallery.IsFillFullChecked.Value = false;
        
        vm.Gallery.IsNoneMenuChecked.Value = false;
        vm.Gallery.IsNoneBottomChecked.Value = false;
        vm.Gallery.IsNoneFullChecked.Value = false;
        
        vm.Gallery.IsSquareMenuChecked.Value = false;
        vm.Gallery.IsSquareBottomChecked.Value = false;
        vm.Gallery.IsSquareFullChecked.Value = false;
        
        vm.Gallery.IsFillSquareMenuChecked.Value = false;
        vm.Gallery.IsFillSquareBottomChecked.Value = false;
        vm.Gallery.IsFillSquareFullChecked.Value = false;

        if (Settings.Gallery.FullGalleryStretchMode.Equals("Square", StringComparison.OrdinalIgnoreCase))
        {
            vm.Gallery.IsSquareFullChecked.Value = true;
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                vm.Gallery.IsSquareMenuChecked.Value = true;
                SetSquareStretch(vm);
            }
        }
        else if (Settings.Gallery.FullGalleryStretchMode.Equals("FillSquare", StringComparison.OrdinalIgnoreCase))
        {
            vm.Gallery.IsFillSquareFullChecked.Value = true;
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                vm.Gallery.IsFillSquareMenuChecked.Value = true;
                SetSquareFillStretch(vm);
            }
        }
        else if (Enum.TryParse<Stretch>(Settings.Gallery.FullGalleryStretchMode, out var stretchMode))
        {
            SetStretchIsChecked(stretchMode, true);
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                SetGalleryStretch(vm, stretchMode);
            }
        }
        else
        {
            vm.Gallery.GalleryItem.ItemWidth.Value = double.NaN;
            if (GalleryFunctions.IsFullGalleryOpen)
            {
                vm.Gallery.IsUniformMenuChecked.Value = true;
                SetGalleryStretch(vm, Stretch.Uniform);
            }
            vm.Gallery.IsUniformFullChecked.Value = true;
        }
        

        if (Settings.Gallery.BottomGalleryStretchMode.Equals("Square", StringComparison.OrdinalIgnoreCase))
        {
            vm.Gallery.IsSquareBottomChecked.Value = true;
            if (!GalleryFunctions.IsFullGalleryOpen)
            {
                vm.Gallery.IsSquareMenuChecked.Value = true;
                SetSquareStretch(vm);
            }
        }
        else if (Settings.Gallery.BottomGalleryStretchMode.Equals("FillSquare", StringComparison.OrdinalIgnoreCase))
        {
            vm.Gallery.IsFillSquareBottomChecked.Value = true;
            if (!GalleryFunctions.IsFullGalleryOpen)
            {
                vm.Gallery.IsFillSquareMenuChecked.Value = true;
                SetSquareFillStretch(vm);
            }
        }
        else if (Enum.TryParse<Stretch>(Settings.Gallery.BottomGalleryStretchMode, out var stretchMode))
        {
            SetStretchIsChecked(stretchMode, false);
            if (!GalleryFunctions.IsFullGalleryOpen)
            {
                SetGalleryStretch(vm, stretchMode);
            }
        }
        else
        {
            vm.Gallery.IsUniformBottomChecked.Value = true;
            if (!GalleryFunctions.IsFullGalleryOpen)
            {
                vm.Gallery.IsUniformMenuChecked.Value = true;
                SetGalleryStretch(vm, Stretch.Uniform);
            }
        }
    
        
        return;

        void SetStretchIsChecked(Stretch stretchMode, bool isFullGallery)
        {
            switch (stretchMode)
            {
                case Stretch.Uniform:
                    if (GalleryFunctions.IsFullGalleryOpen)
                    {
                        vm.Gallery.IsUniformFullChecked.Value = true;
                        if (isFullGallery)
                        {
                            vm.Gallery.IsUniformMenuChecked.Value = true;
                        }
                    }
                    else
                    {
                        vm.Gallery.IsUniformBottomChecked.Value = true;
                        if (!isFullGallery)
                        {
                            vm.Gallery.IsUniformMenuChecked.Value = true;
                        }
                    }
                    break;
                case Stretch.UniformToFill:
                    if (GalleryFunctions.IsFullGalleryOpen)
                    {
                        vm.Gallery.IsUniformToFillFullChecked.Value = true;
                        if (isFullGallery)
                        {
                            vm.Gallery.IsUniformToFillMenuChecked.Value = true;
                        }
                    }
                    else
                    {
                        vm.Gallery.IsUniformToFillBottomChecked.Value = true;
                        if (!isFullGallery)
                        {
                            vm.Gallery.IsUniformToFillMenuChecked.Value = true;
                        }
                    }
                    break;
                case Stretch.Fill:
                    if (GalleryFunctions.IsFullGalleryOpen)
                    {
                        vm.Gallery.IsFillFullChecked.Value = true;
                        if (isFullGallery)
                        {
                            vm.Gallery.IsFillMenuChecked.Value = true;
                        }
                    }
                    else
                    {
                        vm.Gallery.IsFillBottomChecked.Value = true;
                        if (!isFullGallery)
                        {
                            vm.Gallery.IsFillMenuChecked.Value = true;
                        }
                    }
                    break;
                case Stretch.None:
                    if (GalleryFunctions.IsFullGalleryOpen)
                    {
                        vm.Gallery.IsNoneFullChecked.Value = true;
                        if (isFullGallery)
                        {
                            vm.Gallery.IsNoneMenuChecked.Value = true;
                        }
                    }
                    else
                    {
                        vm.Gallery.IsNoneBottomChecked.Value = true;
                        if (!isFullGallery)
                        {
                            vm.Gallery.IsNoneMenuChecked.Value = true;
                        }
                    }
                    break;
                default:
                    if (!GalleryFunctions.IsFullGalleryOpen)
                    {
                        vm.Gallery.IsUniformMenuChecked.Value = true;
                    }
                    vm.Gallery.IsUniformFullChecked.Value = true;
                    vm.Gallery.IsUniformBottomChecked.Value = true;
                    break;
            }
        }
    }
    
    public static void SetGalleryStretch(MainViewModel vm, Stretch stretch)
    {
        vm.Gallery.GalleryItem.ItemWidth.Value = double.NaN;
        vm.Gallery.GalleryStretch.Value = stretch;
    }

    public static void SetSquareStretch(MainViewModel vm)
    {
        vm.Gallery.GalleryItem.ItemWidth.Value  = vm.Gallery.GalleryItem.ItemHeight.Value;
        vm.Gallery.GalleryStretch.Value = Stretch.Uniform;
    }
    
    public static void SetSquareFillStretch(MainViewModel vm)
    {
        vm.Gallery.GalleryItem.ItemWidth.Value  = vm.Gallery.GalleryItem.ItemHeight.Value;;
        vm.Gallery.GalleryStretch.Value = Stretch.Fill;
    }

    public static void ChangeBottomGalleryItemStretch(MainViewModel vm, Stretch stretch)
    {
        SetGalleryStretch(vm, stretch);
        
        Settings.Gallery.BottomGalleryStretchMode = stretch.ToString();
    }
    
    public static void ChangeFullGalleryItemStretch(MainViewModel vm, Stretch stretch)
    {
        SetGalleryStretch(vm, stretch);
        
        Settings.Gallery.FullGalleryStretchMode = stretch.ToString();
    }
    
    public static void ChangeBottomGalleryStretchSquare(MainViewModel vm)
    {
        SetSquareStretch(vm);
        
        Settings.Gallery.BottomGalleryStretchMode = "Square";
    }
    
    public static void ChangeBottomGalleryStretchSquareFill(MainViewModel vm)
    {
        SetSquareFillStretch(vm);
        
        Settings.Gallery.BottomGalleryStretchMode = "FillSquare";
    }

    public static void ChangeFullGalleryStretchSquare(MainViewModel vm)
    {
        SetSquareStretch(vm);
        
        Settings.Gallery.FullGalleryStretchMode = "Square";
    }
    
    public static void ChangeFullGalleryStretchSquareFill(MainViewModel vm)
    {
        SetSquareFillStretch(vm);
        
        Settings.Gallery.FullGalleryStretchMode = "FillSquare";
    }
}
