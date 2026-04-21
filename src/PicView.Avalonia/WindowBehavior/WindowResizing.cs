using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;
using PicView.Core.Sizing;

namespace PicView.Avalonia.WindowBehavior;

public static class WindowResizing
{
    #region Window Resize Handling

    public static bool KeepWindowSize(Window window, AvaloniaPropertyChangedEventArgs<Size> size)
    {
        if (!size.OldValue.HasValue || !size.NewValue.HasValue ||
            size.Sender != window || size.OldValue.Value.Width == 0 || size.OldValue.Value.Height == 0 ||
            size.NewValue.Value.Width == 0 || size.NewValue.Value.Height == 0)
        {
            return false;
        }
        
        var oldSize = size.OldValue.Value;
        var newSize = size.NewValue.Value;

        var x = (oldSize.Width - newSize.Width) / 2;
        var y = (oldSize.Height - newSize.Height) / 2;

        window.Position = new PixelPoint(window.Position.X + (int)x, window.Position.Y + (int)y);
        
        return true;
    }

    public static void HandleWindowResize(Window window, AvaloniaPropertyChangedEventArgs<Size> size)
    {
        if (!Settings.WindowProperties.AutoFit || window.DataContext is not MainViewModel vm)
        {
            return;
        }

        var isWindowResized = KeepWindowSize(window, size);
        if (!isWindowResized)
        {
            return;
        }
    }

    private static void RepositionCursorIfTriggered(
        MainViewModel vm,
        bool isTriggered,
        Action<bool> setTrigger,
        Func<Control?> controlProvider,
        Point offset)
    {
        if (!isTriggered)
        {
            return;
        }

        var control = controlProvider();
        if (control is not null)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var screenPoint = control.PointToScreen(offset);
                    vm.PlatformService?.SetCursorPos(screenPoint.X, screenPoint.Y);
                }, DispatcherPriority.Render);

            }
            else
            {
                var screenPoint = control.PointToScreen(offset);
                vm.PlatformService?.SetCursorPos(screenPoint.X, screenPoint.Y);
            }
        }

        setTrigger(false);
    }

    #endregion
    
    #region Set Window Size

    public static void SetSize(MainViewModel vm)
    {
        var size = GetSize(vm);

        if (size is null)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            SetSize(size.Value, vm);
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(() => SetSize(size.Value, vm));
        }
    }

    public static async Task SetSizeAsync(MainViewModel vm)
    {
        var size = GetSize(vm);

        if (size is null)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() => SetSize(size.Value, vm));
    }

    public static void SetSize(double width, double height, MainViewModel vm)
        => SetSize(width, height, 0, 0, vm.PicViewer.RotationAngle.CurrentValue, vm);

    public static void SetSize(double width, double height, double secondWidth, double secondHeight, double rotation,
        MainViewModel vm)
    {
        var size = GetSize(width, height, secondWidth, secondHeight, rotation, vm);

        if (size is null)
        {
            return;
        }

        SetSize(size.Value, vm);
    }

    public static void SetSize(ImageSize size, MainViewModel vm)
    {
        // vm.MainWindow.TitleMaxWidth.Value = size.TitleMaxWidth;
        // vm.PicViewer.ImageWidth.Value = size.Width;
        // vm.PicViewer.SecondaryImageWidth.Value = size.SecondaryWidth;
        // vm.PicViewer.ImageHeight.Value = size.Height;
        //
        // vm.PicViewer.ScrollViewerWidth.Value = size.ScrollViewerWidth;
        // vm.PicViewer.ScrollViewerHeight.Value = size.ScrollViewerHeight;
        //
        // vm.PicViewer.AspectRatio.Value = size.AspectRatio;
        //
        // if (vm.Gallery is not { } gallery)
        // {
        //     return;
        // }
        //
        // if (Settings.WindowProperties.AutoFit)
        // {
        //     if (Settings.WindowProperties.Fullscreen ||
        //         Settings.WindowProperties.Maximized)
        //     {
        //         vm.PicViewer.GalleryWidth.Value = double.NaN;
        //     }
        //     else
        //     {
        //         var scrollbarSize = Settings.Zoom.ScrollEnabled ? SizeDefaults.ScrollbarSize : 0;
        //         vm.PicViewer.GalleryWidth.Value = vm.PicViewer.RotationAngle.CurrentValue is 90 or 270
        //             ? Math.Max(size.Height + scrollbarSize, SizeDefaults.WindowMinSize + scrollbarSize)
        //             : Math.Max(size.Width + scrollbarSize, SizeDefaults.WindowMinSize + scrollbarSize);
        //     }
        // }
        // else
        // {
        //     vm.PicViewer.GalleryWidth.Value = double.NaN;
        // }
    }

    public static ImageSize? GetSize(MainViewModel vm)
    {
       

        return GetSize(0, 0, 0, 0, vm.PicViewer.RotationAngle.CurrentValue,
            vm);
    }

    public static ImageSize? GetSize(double width, double height, double secondWidth, double secondHeight,
        double rotation,
        MainViewModel vm)
    {
        width = width == 0 ? vm.PicViewer.ImageWidth.CurrentValue : width;
        height = height == 0 ? vm.PicViewer.ImageHeight.CurrentValue : height;

        var screenSize = ScreenHelper.ScreenSize;
        var (containerWidth, containerHeight) = GetContainerSize();

        if (double.IsNaN(width) || double.IsNaN(height))
        {
            return null;
        }

        // var (minWidth, minHeight) = MainWindowViewModel.GetAndSetWindowMinSize(vm);
        
        ImageSize size;
        if (Settings.ImageScaling.ShowImageSideBySide && secondWidth > 0 && secondHeight > 0)
        {
            size = ImageSizeCalculationHelper.GetSideBySideImageSize(
                width,
                height,
                secondWidth,
                secondHeight,
                screenSize,
                0,
                0,
                vm.PlatformWindowService.CombinedTitleButtonsWidth,
                rotation,
                screenSize.Scaling,
                0,
                0,
                0,
                containerWidth,
                containerHeight);
        }
        else
        {
            size = ImageSizeCalculationHelper.GetImageSize(
                width,
                height,
                screenSize,
                0,
                0,
                vm.PlatformWindowService.CombinedTitleButtonsWidth,
                rotation,
                screenSize.Scaling,
                0,
                0,
                0,
                containerWidth,
                containerHeight);
        }

        return size;

        (double containerWidth, double containerHeight) GetContainerSize()
        {
            return Dispatcher.UIThread.CheckAccess() ? Get() : Dispatcher.UIThread.Invoke(Get, DispatcherPriority.Send);

            (double containerWidth, double containerHeight) Get()
            {
                var mainView = UIHelper.GetMainView;

                if (mainView is null)
                {
                    return default;
                }

                containerWidth = mainView.Bounds.Width;
                containerHeight = mainView.Bounds.Height;

                if (double.IsNaN(containerWidth))
                {
                    containerWidth = mainView.Bounds.Width;
                }

                if (double.IsNaN(containerHeight))
                {
                    containerHeight = mainView.Bounds.Height;
                }

                return (containerWidth, containerHeight);
            }
        }
    }

    public static void SaveSize(Window window)
    {
        if (Settings.WindowProperties.Maximized || Settings.WindowProperties.Fullscreen)
        {
            return;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Set();
        }
        else
        {
            Dispatcher.UIThread.InvokeAsync(Set);
        }

        return;

        void Set()
        {
            var top = window.Position.Y;
            var left = window.Position.X;
            Settings.WindowProperties.Top = top;
            Settings.WindowProperties.Left = left;
            Settings.WindowProperties.Width = window.Width;
            Settings.WindowProperties.Height = window.Height;
        }
    }

    #endregion
}