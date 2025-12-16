using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.ColorManagement;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;
using PicView.Core.FileHistory;
using PicView.Core.Localization;
using PicView.Core.Sizing;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.UC;

public partial class StartUpMenu : UserControl
{
    public StartUpMenu()
    {
        MinHeight= SizeDefaults.WindowMinSize;
        MinWidth = SizeDefaults.WindowMinSize;
        InitializeComponent();

        SizeChanged += (_, e) => ResponsiveSize(e.NewSize.Width, e.NewSize.Height);
        Loaded += StartUpMenu_Loaded;
    }

    private void StartUpMenu_Loaded(object? sender, RoutedEventArgs e)
    {
        FilePasteLabel.Content = TranslationManager.Translation.FilePaste ?? "Paste";
        OpenFileDialogLabel.Content = TranslationManager.Translation.OpenFileDialog ?? "Open File";
        OpenLastFileLabel.Content = TranslationManager.Translation.OpenLastFile ?? "Open Last File";
        
        SelectFileButton.PointerEntered += (_, _) =>
        {
            if (!this.TryFindResource("SelectFileBrush", Application.Current.RequestedThemeVariant, out var brush))
                return;

            var selectFileBrush = brush as SolidColorBrush;
            selectFileBrush.Color = ColorManager.PrimaryAccentColor;
        };

        SelectFileButton.PointerExited += (_, _) =>
        {
            if (!this.TryFindResource("SelectFileBrush", Application.Current.RequestedThemeVariant, out var brush))
                return;

            if (!this.TryFindResource("SecondaryTextColor", Application.Current.RequestedThemeVariant, out var color))
                return;

            var selectFileBrush = brush as SolidColorBrush;
            selectFileBrush.Color = color as Color? ?? default;
        };

        OpenLastFileButton.PointerEntered += (_, _) =>
        {
            if (!this.TryFindResource("OpenLastFileBrush", Application.Current.RequestedThemeVariant, out var brush))
                return;

            var selectFileBrush = brush as SolidColorBrush;
            selectFileBrush.Color = ColorManager.PrimaryAccentColor;
        };

        OpenLastFileButton.PointerExited += (_, _) =>
        {
            if (!this.TryFindResource("OpenLastFileBrush", Application.Current.RequestedThemeVariant, out var brush))
                return;

            if (!this.TryFindResource("SecondaryTextColor", Application.Current.RequestedThemeVariant, out var color))
                return;

            var selectFileBrush = brush as SolidColorBrush;
            selectFileBrush.Color = color as Color? ?? default;
        };

        PasteButton.PointerEntered += (_, _) =>
        {
            if (!this.TryFindResource("PasteBrush", Application.Current.RequestedThemeVariant, out var brush))
                return;

            var selectFileBrush = brush as SolidColorBrush;
            selectFileBrush.Color = ColorManager.PrimaryAccentColor;
        };

        PasteButton.PointerExited += (_, _) =>
        {
            if (!this.TryFindResource("PasteBrush", Application.Current.RequestedThemeVariant, out var brush))
                return;
            
            if (!this.TryFindResource("SecondaryTextColor", Application.Current.RequestedThemeVariant, out var color))
                return;
            
            var pasteBrush = brush as SolidColorBrush;
            pasteBrush.Color = color as Color? ?? default;
        };

        if (UIHelper.GetMainView.DataContext is MainViewModel vm)
        {
            TitleManager.SetNoImageTitle(vm);
            vm.Tabs.ActiveTab.Value.TabTitle.Value = TranslationManager.Translation.NoImage ?? "No Image";
        }

        SelectFileButton.Click += async delegate { await SelectFileButtonOnClick(); };
        OpenLastFileButton.Click += async delegate { await OpenLastButtonOnClick(); };
        
        UIHelper.GetHoverBar?.IsVisible = false;
    }
    
    private async ValueTask SelectFileButtonOnClick()
    {
        var file = await FilePicker.SelectFile().ConfigureAwait(false);
        if (file is null)
        {
            return;
        }

        var vm = await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (DataContext is not TabViewModel { ParentWindowContext: MainViewModel vm })
            {
                return null;
            }
            vm.Tabs.ActiveTab.Value.CurrentView.Value = new ImageViewer2();
            return vm;
        });
        if (vm is not null)
        {
            var tab = await Dispatcher.UIThread.InvokeAsync(() => DataContext as TabViewModel);
            await vm.Tabs.LoadFromFileAsync(file, tab).ConfigureAwait(false);
        }
    }
    
    private async ValueTask OpenLastButtonOnClick()
    {
        var lastFile = Settings.StartUp.LastFile;
        try
        {
            if (!string.IsNullOrEmpty(lastFile))
            {
                var vm = Dispatcher.UIThread.Invoke(() =>
                {
                    if (DataContext is not TabViewModel tab)
                    {
                        return null;
                    }
                    tab.CurrentView.Value = new ImageViewer2();
                    return tab.ParentWindowContext as MainViewModel;
                });
                await vm.Tabs.LoadFromStringAsync(lastFile, DataContext as TabViewModel);
            }
            else
            {
                var lastEntry = FileHistoryManager.GetLastEntry();
                if (lastEntry != null)
                {
                    var vm = await ChangeToImageViewer();
                    await vm.Tabs.LoadFromStringAsync(lastEntry, DataContext as TabViewModel);
                }
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(StartUpMenu), nameof(OpenLastButtonOnClick), e);
            await ChangeToStartUpMenu();
        }

        return;
        
        async ValueTask<MainViewModel?> ChangeToImageViewer()
        {
            return await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var vm = Dispatcher.UIThread.Invoke(() =>
                {
                    if (DataContext is not TabViewModel tab)
                    {
                        return null;
                    }
                    return tab.ParentWindowContext as MainViewModel;
                });
                vm.Tabs.ActiveTab.Value.CurrentView.Value = new ImageViewer2();
                return vm;
            });
        }
        
        async ValueTask ChangeToStartUpMenu(){
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (DataContext is not TabViewModel { ParentWindowContext: MainViewModel vm })
                {
                    return;
                }
                vm.Tabs.ActiveTab.Value.CurrentView.Value = new StartUpMenu();
            });
        }}
    }

    public void ResponsiveSize(double width, double height)
    {
        const int breakPoint = 900;
        const int bottomMargin = 16;
        const int logoWidth = 350;
        
        LogoViewbox.Height = double.NaN;
        
        if (DataContext is not MainViewModel vm)
            return;
        
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return;
        
        if (Settings.WindowProperties.Fullscreen || Settings.WindowProperties.Maximized)
        {
            ShowFullLogo();
        }
        else if (Settings.WindowProperties.AutoFit)
        {
            ShowIcon();
            vm.MainWindow.TitleMaxWidth.Value = logoWidth;
            return;
        }

        switch (width)
        {
            case < breakPoint:
                ShowIcon();
                break;
            case > breakPoint:
                ShowFullLogo();
                break;
        }

        var titleMaxWidth = ImageSizeCalculationHelper.GetTitleMaxWidth(vm.PicViewer.RotationAngle.CurrentValue, width,
            height,
            desktop.MainWindow.MinWidth, desktop.MainWindow.MinHeight, vm.PlatformWindowService.CombinedTitleButtonsWidth,
            desktop.MainWindow.Width);

        if (Settings.Zoom.ScrollEnabled)
        {
            vm.MainWindow.TitleMaxWidth.Value = titleMaxWidth - SizeDefaults.ScrollbarSize;
        }
        else
        {
            vm.MainWindow.TitleMaxWidth.Value = titleMaxWidth;
        }
        
        return;

        void ShowIcon()
        {
            if (this.TryFindResource("LogoImage", Application.Current.RequestedThemeVariant, out var icon))
                Logo.Source = icon as DrawingImage;
            LogoViewbox.Width = logoWidth;
            Buttons.Margin = new Thickness(0, 0, 0, bottomMargin);
            Buttons.VerticalAlignment = VerticalAlignment.Bottom;
        }

        void ShowFullLogo()
        {
            if (this.TryFindResource("LogoFullImage", Application.Current.RequestedThemeVariant, out var logo))
                Logo.Source = logo as DrawingImage;
            LogoViewbox.Width = double.NaN;
            if (Settings.WindowProperties.Fullscreen || Settings.WindowProperties.Maximized)
            {
                Buttons.Margin = new Thickness(0, 0, 0, bottomMargin + SizeDefaults.WindowMinSize / 2);
                Buttons.VerticalAlignment = VerticalAlignment.Bottom;
            }
            else
            {
                Buttons.Margin = new Thickness(0, 220, 25, bottomMargin - 100);
                Buttons.VerticalAlignment = VerticalAlignment.Center;
            }
        }
    }
}