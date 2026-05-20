using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.Animations;
using PicView.Avalonia.Clipboard;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Functions;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.UI;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
using PicView.Core.Localization;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Crop;

public static class CropFunctions
{
    public static bool IsCropping { get; private set; }
    
    private static object? _backUpView;

    /// <summary>
    /// Starts the cropping functionality by setting up the ImageCropperViewModel 
    /// and adding the CropControl to the main view.
    /// </summary>
    /// <param name="vm">The main view model instance containing image properties and state.</param>
    /// <remarks>
    /// This method checks if cropping can be enabled and if the image source is valid.
    /// If conditions are met, it configures the crop control with the appropriate dimensions
    /// and updates the view model's title and tooltip to reflect the cropping state.
    /// </remarks>
    public static async Task StartCropControlAsync(MainWindowViewModel vm)
    {
        if (!DetermineIfShouldBeEnabled(vm))
        {
            return;
        }
        
        _backUpView = vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.Value;
        var isDockedGalleryShown = Settings.Gallery.IsGalleryDocked;
        // Hide bottom gallery when entering crop mode
        if (isDockedGalleryShown)
        {
            // Reset setting before resizing
            Settings.Gallery.IsGalleryDocked = false;
            WindowResizing.SetSize(vm, WindowResizeReason.Application);
        }
        
        var size = new Size(vm.ImageWidth.CurrentValue, vm.ImageHeight.CurrentValue);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            vm.Crop = new CropViewModel
            {
                AspectRatio = vm.WindowTabs.ActiveTab.CurrentValue.InitialZoom.CurrentValue,
            };
            
            var cropControl = new CropControl
            {
                DataContext = vm,
                Width = size.Width,
                Height = size.Height,
                Margin = new Thickness(0)
            };
            vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.Value = cropControl;
        });

        IsCropping = true;
        vm.WindowTabs.ActiveTab.CurrentValue.Title.Value = TranslationManager.Translation.CropMessage!;
        vm.WindowTabs.ActiveTab.CurrentValue.TitleTooltip.Value = TranslationManager.Translation.CropMessage!;
        
        await FunctionsMapper.CloseMenus();
        
        if (isDockedGalleryShown)
        {
            Settings.Gallery.IsGalleryDocked = true;
        }
        
        vm.Crop.CloseCropCommand.Subscribe(_ =>
        {
            CloseCropControl(vm);
        },DebugHelper.LogError(nameof(CropFunctions), nameof(CloseCropControl)));
        
        vm.Crop.CopyCropImageCommand.SubscribeAwait(async (_, _) =>
        {
            await CopyCroppedImageAsync();
        },DebugHelper.LogError(nameof(CropFunctions), nameof(CloseCropControl)));
        
        vm.Crop.CropImageCommand.SubscribeAwait(async (_, _) =>
        {
            await PackAndSaveImage();
        },DebugHelper.LogError(nameof(CropFunctions), nameof(CloseCropControl)));
    }
    
    private static async ValueTask CopyCroppedImageAsync()
    {
        if (UIHelper.GetMainView.DataContext is not MainWindowViewModel vm ||
            vm.WindowTabs.ActiveTab.CurrentValue.Image.CurrentValue is not Bitmap sourceBitmap)
        {
            return;
        }

        var x = Convert.ToInt32(vm.Crop.SelectionX.CurrentValue / vm.Crop.AspectRatio);
        var y = Convert.ToInt32(vm.Crop.SelectionY.CurrentValue / vm.Crop.AspectRatio);
        var rect = new PixelRect(x, y, (int)vm.Crop.PixelSelectionWidth.CurrentValue, (int)vm.Crop.PixelSelectionHeight.CurrentValue);

        var croppedBitmap = new CroppedBitmap(sourceBitmap, rect);
        var bitmap = BitmapHelper.ConvertCroppedBitmapToBitmap(croppedBitmap);

        if (bitmap is not null)
        {
            await Task.WhenAll(ClipboardImageOperations.CopyImageToClipboard(bitmap), AnimationsHelper.CopyAnimation());
        }
    }
    
    private static (string fileName, FileInfo? fileInfo, Bitmap? bitmap) PrepareCropData(MainWindowViewModel vm)
        => vm.WindowTabs.ActiveTab.CurrentValue.FileInfo?.CurrentValue?.Exists ?? false
            ? CreateNewCroppedImage(vm, vm.Crop)
            : (vm.WindowTabs.ActiveTab.CurrentValue.FileInfo.Value.FullName, vm.WindowTabs.ActiveTab.CurrentValue.FileInfo.Value, null);

    private static (string fileName, FileInfo fileInfo, Bitmap bitmap) CreateNewCroppedImage(MainWindowViewModel vm, CropViewModel crop)
    {
        var fileName = $"{TranslationManager.Translation.Crop} {new Random().Next(9999)}.png";
        var x = Convert.ToInt32(crop.SelectionX.CurrentValue / crop.AspectRatio);
        var y = Convert.ToInt32(crop.SelectionY.CurrentValue / crop.AspectRatio);
        var width = (int)crop.PixelSelectionWidth.CurrentValue;
        var height = (int)crop.PixelSelectionHeight.CurrentValue;
        var croppedBitmap = new CroppedBitmap(vm.WindowTabs.ActiveTab.CurrentValue.Image.CurrentValue as Bitmap, new PixelRect(x, y, width, height));
        var bitmap = BitmapHelper.ConvertCroppedBitmapToBitmap(croppedBitmap);
        return (fileName, new FileInfo(fileName), bitmap);
    }

    
    public static void CloseCropControl(MainWindowViewModel vm)
    {
        if (Settings.Gallery.IsGalleryDocked)
        {
            
            WindowResizing.SetSize(vm, WindowResizeReason.Application);
        }
        
        vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.Value = _backUpView;
        IsCropping = false;
        vm.WindowTabs.ActiveTab.CurrentValue.UpdateTabTitle();
        
        // Reset image type to fix issue with animated images
        // switch (vm.PicViewer.ImageType.CurrentValue)
        // {
        //     case ImageType.AnimatedWebp:
        //         vm.PicViewer.ImageType.Value = ImageType.Bitmap;
        //         vm.PicViewer.ImageType.Value = ImageType.AnimatedWebp;
        //         break;
        //     case ImageType.AnimatedGif:
        //         vm.PicViewer.ImageType.Value = ImageType.Bitmap;
        //         vm.PicViewer.ImageType.Value = ImageType.AnimatedGif;
        //         break;
        // }
        
        vm.Crop = null;
    }

    private static async ValueTask PackAndSaveImage()
    {
        if (UIHelper.GetMainView.DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        
        var (fileName, fileInfo, bitmap) = PrepareCropData(vm);
        
        var saveFileDialog = await FilePicker.PickFileForSavingAsync(fileName);
        if (saveFileDialog == null)
        {
            return;
        }
        
        await SaveImage(vm, saveFileDialog, fileInfo, bitmap);
        
        CloseCropControl(vm);

        var tab = vm.WindowTabs.ActiveTab.CurrentValue;
        if (tab.FileInfo.Value.FullName == saveFileDialog)
        {
            await tab.ImageIterator.ReloadAsync(tab.GetTabCancellation());
        }
    }
    
    private static async ValueTask SaveImage(MainWindowViewModel vm, string saveFilePath, FileInfo fileInfo, Bitmap? bitmap)
    {
        if (bitmap != null)
        {
            bitmap.Save(saveFilePath);
            return;
        }

        await SaveWithMagickImage(vm.Crop, saveFilePath, fileInfo);
    }

    
    private static async ValueTask SaveWithMagickImage(CropViewModel crop, string saveFilePath, FileInfo fileInfo)
    {
        using var image = new MagickImage(fileInfo.FullName);
        var x = Convert.ToInt32(crop.SelectionX.CurrentValue / crop.AspectRatio);
        var y = Convert.ToInt32(crop.SelectionY.CurrentValue / crop.AspectRatio);
        var width = crop.PixelSelectionWidth.CurrentValue;
        var height = crop.PixelSelectionHeight.CurrentValue;
        var geometry = new MagickGeometry(x, y, width, height);

        image.Crop(geometry);
        await image.WriteAsync(saveFilePath);
    }

    public static bool DetermineIfShouldBeEnabled(MainWindowViewModel vm)
    {
        if (IsCropping)
        {
            return false;
        }
        
        if (vm.WindowTabs.ActiveTab.CurrentValue.Image.CurrentValue is not Bitmap || Settings.ImageScaling.ShowImageSideBySide)
        {
            vm.WindowTabs.ActiveTab.CurrentValue.ShouldCropBeEnabled.Value = false;
            return false;
        }
        
        if (vm.IsEditableTitlebarOpen.CurrentValue)
        {
            return false;
        }
        
        if (vm.WindowTabs.ActiveTab.CurrentValue.RotationAngle.CurrentValue is 0 && vm.WindowTabs.ActiveTab.CurrentValue.ScaleX.CurrentValue is 1)
        {
            vm.WindowTabs.ActiveTab.CurrentValue.ShouldCropBeEnabled.Value = true;
            return true;
        }
        
        vm.WindowTabs.ActiveTab.CurrentValue.ShouldCropBeEnabled.Value = false;
        return false;
    }
}