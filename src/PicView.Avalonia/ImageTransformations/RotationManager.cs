using Avalonia.Controls;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.ImageTransformations;

public static class RotationManager
{
    public static void ResetZoomAndRotations(MainWindowViewModel vm)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.CurrentView.CurrentValue is ImageViewer2 imageViewer)
        {
            imageViewer.ResetZoomSlim();
            imageViewer.Rotate(0);
        }
        
        if (Settings.WindowProperties.AutoFit)
        {
            WindowResizing2.SetSize(vm, WindowResizeReason.Layout);
        }
    }
}