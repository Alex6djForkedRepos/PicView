using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using PicView.Core.Config;
using PicView.Core.ViewModels;
using PicView.Core.Gallery;
using PicView.Core.Sizing;
using R3;

namespace PicView.Avalonia.CustomControls;

public class GalleryAnimationControl : UserControl
{
    #region Cleanup

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        if (Parent is Control parent)
        {
            parent.SizeChanged -= ParentSizeChanged;
        }

        Loaded -= OnControlLoaded;
        RemoveHandler(PointerPressedEvent, PreviewPointerPressedEvent);
    }

    #endregion

    #region Fields and Properties

    private TabViewModel? ViewModel => DataContext as TabViewModel;

    private readonly CompositeDisposable _disposables = new();
    private AutoScrollViewer? _scrollViewer;

    public static readonly AvaloniaProperty<GalleryMode?> GalleryModeProperty =
        AvaloniaProperty.Register<GalleryAnimationControl, GalleryMode?>(nameof(GalleryMode));

    public GalleryMode GalleryMode
    {
        get => GetValue(GalleryModeProperty) as GalleryMode? ?? GalleryMode.Closed;
        set => SetValue(GalleryModeProperty, value);
    }

    #endregion

    #region Constructors

    public GalleryAnimationControl()
    {
        Loaded += OnControlLoaded;
    }

    private void OnControlLoaded(object? sender, RoutedEventArgs e)
    {
        AddHandler(PointerPressedEvent, PreviewPointerPressedEvent, RoutingStrategies.Tunnel);

        _scrollViewer = this.FindControl<AutoScrollViewer>("GalleryScrollViewer");

        if (ViewModel == null)
        {
            return;
        }

        ViewModel.Gallery.IsGalleryExpanded.Subscribe(_ => UpdateGalleryState()).AddTo(_disposables);
        ViewModel.Gallery.IsDockedGalleryVisible.Subscribe(_ => UpdateGalleryState()).AddTo(_disposables);
        ViewModel.Gallery.GalleryDockPosition.Subscribe(_ => UpdateGalleryState()).AddTo(_disposables);

        if (Parent is Control parent)
        {
            parent.SizeChanged += ParentSizeChanged;
        }
    }

    private void UpdateGalleryState()
    {
        if (ViewModel == null)
        {
            return;
        }

        var visible = ViewModel.Gallery.IsDockedGalleryVisible.Value;

        if (!visible)
        {
            IsVisible = false;
            // Ensure dimension doesn't block layout if visible state lags or is ignored by DockPanel (though IsVisible should suffice)
            Width = double.NaN;
            Height = double.NaN;
            return;
        }

        IsVisible = true;
        var dock = ViewModel.Gallery.GalleryDockPosition.Value;
        var expanded = ViewModel.Gallery.IsGalleryExpanded.Value;

        if (expanded)
        {
            // Full / Expanded Logic
            if (dock == GalleryDockPosition.Top || dock == GalleryDockPosition.Bottom)
            {
                Height = (Parent as Control)?.Bounds.Height ?? double.NaN;
                Width = double.NaN;
            }
            else
            {
                Width = (Parent as Control)?.Bounds.Width ?? double.NaN;
                Height = double.NaN;
            }

            if (_scrollViewer != null)
            {
                _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }
        else
        {
            // Docked / Compact Logic
            var size = Settings.Gallery.BottomGalleryItemSize + SizeDefaults.ScrollbarSize;

            if (dock == GalleryDockPosition.Top || dock == GalleryDockPosition.Bottom)
            {
                Height = size;
                Width = double.NaN;

                if (_scrollViewer != null)
                {
                    _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                    _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
            }
            else
            {
                Width = size;
                Height = double.NaN;

                if (_scrollViewer != null)
                {
                    _scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    _scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                }
            }
        }
    }

    #endregion

    #region Events

    private void ParentSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (ViewModel?.Gallery.IsGalleryExpanded.Value == true)
        {
            UpdateGalleryState();
        }
    }

    private void PreviewPointerPressedEvent(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        // Disable right click selection, to not interfere with context menu
        e.Handled = true;
    }

    #endregion
}