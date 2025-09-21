using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using PicView.Avalonia.Crop;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Conversion;

namespace PicView.Avalonia.Win32.Views;

public partial class WinTitleBar : UserControl
{
    public WinTitleBar()
    {
        InitializeComponent();
        Loaded += (_, _) =>
        {
            if (Settings.Theme.GlassTheme)
            {
                ApplyGlassThemeStyles();
            }

            InitializeEventHandlers();
        };
    }

    // Extract method: centralize glass theme styling to remove duplication
    private void ApplyGlassThemeStyles()
    {
        ApplyTransparentStyle(TopWindowBorder);
        ApplyTransparentStyle(LogoBorder);
        ApplyTransparentStyle(EditableTitlebar);
        ApplyTransparentStyle(CloseButton);
        ApplyTransparentStyle(MinimizeButton);
        ApplyTransparentStyle(RestoreButton);
        ApplyTransparentStyle(FullscreenButton);
        ApplyTransparentStyle(GalleryButton);
        ApplyTransparentStyle(MenuButton);

        var glassForeground = UIHelper.GetBrush("SecondaryTextColor");
        EditableTitlebar.Foreground = glassForeground;
        CloseButton.Foreground = glassForeground;
        MinimizeButton.Foreground = glassForeground;
        RestoreButton.Foreground = glassForeground;
        GalleryButton.Foreground = glassForeground;
        MenuButton.Foreground = glassForeground;
    }

    private void ApplyTransparentStyle(UserControl control)
    {
        control.Background = Brushes.Transparent;
        control.BorderThickness = new Thickness(0);
    }

    private void ApplyTransparentStyle(Button button)
    {
        button.Background = Brushes.Transparent;
        button.BorderThickness = new Thickness(0);
    }

    private static void ApplyTransparentStyle(Border borderLike)
    {
        borderLike.Background = Brushes.Transparent;
        borderLike.BorderThickness = new Thickness(0);
    }

    private void InitializeEventHandlers()
    {
        PointerPressed += (_, e) => TryDragWindow(e);
        PointerExited += (_, _) => { DragAndDropHelper.RemoveDragDropView(); };
        MenuButton.Click += (_, _) => { ToggleMenu(); };
        MainMenu.Closed += (_, _) => { CloseMenu(); };
    }

    private void ToggleMenu()
    {
        if (MainMenu.IsOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    private void OpenMenu()
    {
        MainMenu.IsVisible = true;
        MainMenu.Open();
        EditableTitlebar.IsVisible = false;
        GalleryButton.IsVisible = false;
        MenuButton.IsVisible = false;

        if (DataContext is not MainViewModel vm)
        {
            return;
        }

        // Overflow buttons if the window is too small
        if (vm.MainWindow.TitleMaxWidth.CurrentValue < vm.PlatformWindowService.CombinedTitleButtonsWidth * 2)
        {
            WinBtnPanel.IsVisible = false;
        }
        else
        {
            WinBtnPanel.IsVisible = true;
        }

        FileMenuItem.Open();

        Task.Run(() =>
        {
            CropFunctions.DetermineIfShouldBeEnabled(vm);
            vm.PicViewer.ShouldOptimizeImageBeEnabled.Value =
                ConversionHelper.DetermineIfOptimizeImageShouldBeEnabled(vm.PicViewer.FileInfo?.CurrentValue);
        });
    }

    private void CloseMenu()
    {
        MainMenu.Close();
        MainMenu.IsVisible = false;
        EditableTitlebar.IsVisible = true;
        GalleryButton.IsVisible = true;
        MenuButton.IsVisible = true;
        WinBtnPanel.IsVisible = true;
    }

    private void TryDragWindow(PointerPressedEventArgs e)
    {
        if (VisualRoot is null || DataContext is not MainViewModel vm)
        {
            return;
        }

        if (vm.MainWindow.IsEditableTitlebarOpen.Value || MainMenu.IsOpen)
        {
            return;
        }

        WindowFunctions.WindowDragAndDoubleClickBehavior((Window)VisualRoot, e, vm.PlatformWindowService);
    }
}