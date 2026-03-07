using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.WindowBehavior;

public static class WindowResizing2
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
        if (!Settings.WindowProperties.AutoFit)
        {
            return;
        }

        var isWindowResized = KeepWindowSize(window, size);
        if (!isWindowResized)
        {
            return;
        }
        
        if (window.DataContext is not MainWindowViewModel mainWindowVm)
        {
            return;
        }

        RepositionCursorIfTriggered(mainWindowVm.IsNavigationButtonLeftClicked,
            clicked => mainWindowVm.IsNavigationButtonLeftClicked = clicked,
            () => UIHelper2.GetBottomBar.GetControl<Button>("PreviousButton"),
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.IsNavigationButtonRightClicked,
            clicked => mainWindowVm.IsNavigationButtonRightClicked = clicked,
            () => UIHelper2.GetBottomBar.GetControl<Button>("NextButton"),
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonNextClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonNextClicked = clicked,
            () => UIHelper2.GetHoverBar.GetControl<Button>("NextButton"),
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonPreviousClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverNavigationButtonPreviousClicked = clicked,
            () => UIHelper2.GetHoverBar.GetControl<Button>("PreviousButton"),
            new Point(50, 10));

        RepositionCursorIfTriggered(mainWindowVm.IsClickArrowLeftClicked,
            clicked => mainWindowVm.IsClickArrowLeftClicked = clicked,
            () => UIHelper2.GetMainView.GetControl<UserControl>("ClickArrowLeft"),
            new Point(15, 95));

        RepositionCursorIfTriggered(mainWindowVm.IsClickArrowRightClicked,
            clicked => mainWindowVm.IsClickArrowRightClicked = clicked,
            () => UIHelper2.GetMainView.GetControl<UserControl>("ClickArrowRight"),
            new Point(65, 95));

        RepositionCursorIfTriggered(mainWindowVm.IsBottomToolbarRotationClicked,
            clicked => mainWindowVm.IsBottomToolbarRotationClicked = clicked,
            () => UIHelper2.GetBottomBar.GetControl<IconButton>("RotateRightButton"),
            new Point(11, 7));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateRightClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateRightClicked = clicked,
            () => UIHelper2.GetHoverBar.GetControl<IconButton>("RotateRightButton"),
            new Point(11, 7));

        RepositionCursorIfTriggered(mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateLeftClicked,
            clicked => mainWindowVm.WindowTabs.ActiveTab.CurrentValue.Hoverbar.IsHoverRotateLeftClicked = clicked,
            () => UIHelper2.GetHoverBar.GetControl<IconButton>("RotateLeftButton"),
            new Point(11, 7));
        
        RepositionCursorIfTriggered(mainWindowVm.IsTitlebarRotationClicked,
            clicked => mainWindowVm.IsTitlebarRotationClicked = clicked,
            () => UIHelper2.GetTitlebar.GetControl<IconButton>("RotateRightButton"),
            new Point(11, 7));
    }

    private static void RepositionCursorIfTriggered(
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
        if (control is not null && Application.Current.DataContext is CoreViewModel core)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var screenPoint = control.PointToScreen(offset);
                    core.PlatformService.SetCursorPos(screenPoint.X, screenPoint.Y);
                }, DispatcherPriority.Render);

            }
            else
            {
                var screenPoint = control.PointToScreen(offset);
                core.PlatformService.SetCursorPos(screenPoint.X, screenPoint.Y);
            }
        }

        setTrigger(false);
    }

    #endregion
}