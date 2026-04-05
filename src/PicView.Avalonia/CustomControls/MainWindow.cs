using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using PicView.Avalonia.Interfaces;
using PicView.Avalonia.Views.UC;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.DebugTools;
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
            .Subscribe(HandleWindowResize, static result =>
            {
#if DEBUG
                if (result is { IsFailure: true, Exception: not null })
                {
                    DebugHelper.LogDebug(nameof(MainWindow), nameof(HandleWindowResize), result.Exception);
                }
#endif
            })
            .AddTo(Disposables);
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

    #region Sizing
    
    // Window has been resized
    private void WindowSizeChanged(object? sender, WindowResizedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        
        SharedBottomBar.ResponsiveNavigationBtnSize(e.ClientSize);

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
    
    // Window is being resized
    private void HandleWindowResize(AvaloniaPropertyChangedEventArgs<Size> size)
    {
        if (IsChangingWindowState || WindowState != WindowState.Normal)
        {
            return;
        }
        WindowResizing2.HandleWindowResize(this, size);
    }
    
    #endregion
}