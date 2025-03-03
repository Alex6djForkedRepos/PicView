using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PicView.Avalonia.Crop;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.Views.UC.PopUps;
using PicView.Avalonia.WindowBehavior;

namespace PicView.Avalonia.UI;

/// <summary>
/// Provides UI-related helper methods and properties
/// </summary>
public static class UIHelper
{
    
    public static bool IsDialogOpen { get; set; }

    public static MainView? GetMainView { get; private set; }
    public static Control? GetTitlebar { get; private set; }
    public static EditableTitlebar? GetEditableTitlebar { get; private set; }
    public static GalleryAnimationControlView? GetGalleryView { get; private set; }
    public static BottomBar? GetBottomBar { get; private set; }
    public static ToolTipMessage? GetToolTipMessage { get; private set; }

    /// <summary>
    /// Sets up control references from the main desktop application
    /// </summary>
    public static void SetControls(IClassicDesktopStyleApplicationLifetime desktop)
    {
        GetMainView = desktop.MainWindow?.FindControl<MainView>("MainView");
        GetTitlebar = desktop.MainWindow?.FindControl<Control>("Titlebar");
        GetEditableTitlebar = GetTitlebar?.FindControl<EditableTitlebar>("EditableTitlebar");
        GetGalleryView = GetMainView?.MainGrid.GetControl<GalleryAnimationControlView>("GalleryView");
        GetBottomBar = desktop.MainWindow?.FindControl<BottomBar>("BottomBar");
        GetToolTipMessage = GetMainView?.MainGrid.FindControl<ToolTipMessage>("ToolTipMessage");
    }

    #region Navigation buttons

    /// <summary>
    /// Navigates to the next image using the bottom navigation button
    /// </summary>
    public static void NextButtonNavigation(MainViewModel vm)
    {
        SetButtonIntervalAndNavigate(GetBottomBar?.NextButton, true, false, vm);
    }
    
    /// <summary>
    /// Navigates to the previous image using the bottom navigation button
    /// </summary>
    public static void PreviousButtonNavigation(MainViewModel vm)
    {
        SetButtonIntervalAndNavigate(GetBottomBar?.PreviousButton, false, false, vm);
    }
    
    /// <summary>
    /// Navigates to the next image using the arrow button
    /// </summary>
    public static void NextArrowButtonNavigation(MainViewModel vm)
    {
        SetButtonIntervalAndNavigate(GetMainView?.ClickArrowRight?.PolyButton, true, true, vm);
    }
    
    /// <summary>
    /// Navigates to the previous image using the arrow button
    /// </summary>
    public static void PreviousArrowButtonNavigation(MainViewModel vm)
    {
        SetButtonIntervalAndNavigate(GetMainView?.ClickArrowLeft?.PolyButton, false, true, vm);
    }

    private static void SetButtonIntervalAndNavigate(RepeatButton? button, bool isNext, bool isArrow, MainViewModel vm)
    {
        if (button != null)
        {
            button.Interval = (int)TimeSpan.FromSeconds(Settings.UIProperties.NavSpeed).TotalMilliseconds;
        }

        Task.Run(() => NavigationManager.NavigateAndPositionCursor(isNext, isArrow, vm));
    }

    #endregion
    
    #region Dialog Operations
    
    /// <summary>
    /// Handles close action based on current application state
    /// </summary>
    public static async Task Close(MainViewModel vm)
    {
        // Handle open menus
        if (MenuManager.IsAnyMenuOpen(vm))
        {
            MenuManager.CloseMenus(vm);
            return;
        }

        // Handle cropping mode
        if (CropFunctions.IsCropping)
        {
            CropFunctions.CloseCropControl(vm);
            return;
        }

        // Handle slideshow
        if (Slideshow.IsRunning)
        {
            Slideshow.StopSlideshow(vm);
            return;
        }

        // Handle fullscreen
        if (Settings.WindowProperties.Fullscreen)
        {
            await WindowFunctions.MaximizeRestore();
            return;
        }
        
        // Handle window close
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (Settings.UIProperties.ShowConfirmationOnEsc)
            {
                GetMainView?.MainGrid.Children.Add(new CloseDialog());
            }
            else
            {
                desktop.MainWindow?.Close();
            }
        });
    }

    #endregion
}