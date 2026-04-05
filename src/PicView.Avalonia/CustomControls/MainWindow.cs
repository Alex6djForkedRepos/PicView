using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.CustomControls;

public class MainWindow : Window, IMainWindow
{
    public CompositeDisposable Disposables { get; set; } = new();
    public bool IsChangingWindowState { get; set; } = false;
    public BottomBar2? SharedBottomBar { get; set; }
    public UserControl? SharedTitleBar { get; set; }

    public MainWindow()
    {
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // Keep window position when resizing
        ClientSizeProperty.Changed.ToObservable()
            .Subscribe(size =>
            {
                if (IsChangingWindowState || WindowState != WindowState.Normal)
                {
                    return;
                }
                WindowResizing2.HandleWindowResize(this, size);
                SharedBottomBar.ResponsiveNavigationBtnSize(size);
            }).AddTo(Disposables);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Resized += WindowSizeChanged;
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Resized -= WindowSizeChanged;
        Disposables?.Dispose();
        Loaded -= OnLoaded;
    }
    
    private void WindowSizeChanged(object? sender, WindowResizedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (e.Reason is WindowResizeReason.User)
        {
            // Reset to manual window
            Dispatcher.CurrentDispatcher.Post(() => WindowFunctions2.SetManualWindow(vm, this));
            return;
        }

        if (Settings.WindowProperties.AutoFit)
        {
            return;
        }

        WindowResizing2.SetSize(vm, e.Reason);
    }
}