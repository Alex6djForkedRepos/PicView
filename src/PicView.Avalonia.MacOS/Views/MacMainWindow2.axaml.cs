using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.MacOS.WindowImpl;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.Views.UC.Menus;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ViewModels;
using R3;
using R3.Avalonia;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacMainWindow2 : Window
{
    private readonly AvaloniaRenderingFrameProvider _frameProvider;

    public MacMainWindow2()
    {
        InitializeComponent();

        _frameProvider = new AvaloniaRenderingFrameProvider(GetTopLevel(this));
        UIHelper.SetFrameProvider(_frameProvider);

        Loaded += delegate
        {
            // Keep window position when resizing
            ClientSizeProperty.Changed.ToObservable()
                .Subscribe(size =>
                {
                    if (MacOSWindow.IsChangingWindowState || WindowState != WindowState.Normal)
                    {
                        return;
                    }
                    WindowResizing.HandleWindowResize(this, size);
                });
            if (DataContext is not MainViewModel vm)
            {
                return;
            }
            Observable.EveryValueChanged(this, x => x.WindowState, _frameProvider)
                .Skip(1)
                .SubscribeAwait(async (state, _) =>
            {
                switch (state)
                {
                    case WindowState.FullScreen:
                        if (!Settings.WindowProperties.Fullscreen)
                        {
                            await MacOSWindow2.Fullscreen(this, vm);
                        }

                        break;
                    case WindowState.Maximized:
                        if (!Settings.WindowProperties.Maximized && !Settings.WindowProperties.Fullscreen)
                        {
                            await MacOSWindow2.Maximize(this, vm);
                        }

                        break;
                    case WindowState.Normal:
                        if (Settings.WindowProperties.Maximized || Settings.WindowProperties.Fullscreen)
                        {
                            await MacOSWindow2.Restore(this, vm);
                        }
                        break;
                }
            });
            
            // Hide macOS buttons when interface is hidden
            Observable.EveryValueChanged(vm, x => x.MainWindow.IsTopToolbarShown.CurrentValue, _frameProvider).Subscribe(shown =>
            {
                if (Settings.WindowProperties.Fullscreen)
                {
                    SystemDecorations = SystemDecorations.Full;
                }
                else
                {
                    SystemDecorations = shown ? SystemDecorations.Full : SystemDecorations.None;
                }
            });
            Observable.EveryValueChanged(MainTabControl.Items, x => x.Count).Subscribe(count =>
            {
                vm.NavigationViewModel.IsTabPanelVisible.Value = count > 1;
            });
            
            MainTabControl.TabDetached += MainTabStripOnTabDetached;
            MainTabControl.TabCreated += MainTabStripOnTabCreated;

            var tabMenu = new TabMenu
            {
                Name = "TabsMenu",
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(3, 0, 3, 0),
                IsVisible = false,
                HorizontalAlignment = HorizontalAlignment.Right,
                ZIndex = 2
            };
            MainPanel.Children.Add(tabMenu);
            
            // Close tabMenu when clicking outside of it
            PointerPressed += (_, _) =>
            {
                if (!tabMenu.IsPointerOver)
                {
                    vm.MainWindow.IsTabMenuVisible.Value = false;
                }
            };
        };
    }

    private void MainTabStripOnTabCreated(object? sender, TabCreatedEventArgs e)
    {
        var startUpMenu = new StartUpMenu();
        if (e.CreatedItem is TabViewModel tabViewModel)
        {
            tabViewModel.CurrentView.Value = startUpMenu;
        }
    }

    private void MainTabStripOnTabDetached(object? sender, TabDetachEventArgs e)
    {
        // Create a new window with the detached tab
        var newWindow = new FloatingWindow
        {
            Position = new PixelPoint(e.ScreenPosition.X - 100, e.ScreenPosition.Y - 50),
            Width = Width,
            Height = Height,
            DataContext = DataContext
        };

        if (e.DetachedItem is TabViewModel tab)
        {
            var currentView = tab.CurrentView.Value;
            if (currentView is StartUpMenu startUpMenu)
            {
                newWindow.MainPanel.Children.Add(new ContentControl
                {
                    Content = startUpMenu
                });
            }
            else
            {
                if (currentView is ImageViewer2 imageViewer)
                {
                    imageViewer.DataContext = e.DetachedItem;
                    newWindow.MainPanel.Children.Add(new ContentControl
                    {
                        Content = currentView
                    });
                }
            }
            newWindow.Closed += async (_, _) => await tab.CloseTab();
        }
        
        newWindow.Show();
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
        var vm = (MainViewModel)DataContext;
        WindowResizing.SetSize(vm);
    }

    protected override async void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        await WindowFunctions.WindowClosingBehavior(this);
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        _frameProvider?.Dispose();
    }
}