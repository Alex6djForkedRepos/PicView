using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using PicView.Avalonia.DragAndDrop;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using ReactiveUI;

namespace PicView.Avalonia.Win32.Views;

public partial class WinMainWindow : Window
{
    public WinMainWindow()
    {
        InitializeComponent();
        
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }
        
        Loaded += delegate
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }

            // Keep window position when resizing
            ClientSizeProperty.Changed.Subscribe(size =>
            {
                WindowResizing.HandleWindowResize(this, size);
            });
            ScalingChanged += (_, _) =>
            {
                ScreenHelper.UpdateScreenSize(this);
                WindowResizing.SetSize(DataContext as MainViewModel);
            };
            PointerExited += (_, _) =>
            {
                DragAndDropHelper.RemoveDragDropView();
            };
            
            this.WhenAnyValue(x => x.WindowState).Subscribe(state =>
            {

                switch (state)
                {
                    case WindowState.FullScreen:
                        if (!Settings.WindowProperties.Fullscreen)
                        {
                            vm.PlatformWindowService.Fullscreen();
                        }
                        break;
                    case WindowState.Maximized:
                        if (!Settings.WindowProperties.Maximized)
                        {
                            vm.PlatformWindowService.Maximize();
                        }
                        break;
                    case WindowState.Normal:
                        if (Settings.WindowProperties.Fullscreen || Settings.WindowProperties.Maximized)
                        {
                            vm.PlatformWindowService.Restore();
                        }
                        break;
                }
            });
        };

        desktop.ShutdownRequested += async (_, e) =>
        {
            await WindowFunctions.WindowClosingBehavior(this);
        };
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        await WindowFunctions.WindowClosingBehavior(this);
        base.OnClosing(e);
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext == null)
        {
            return;
        }

        if (e is { HeightChanged: false, WidthChanged: false })
        {
            return;
        }

        if (Settings.WindowProperties.AutoFit)
        {
            return;
        }
        var wm = (MainViewModel)DataContext;
        WindowResizing.SetSize(wm);
    }
}