using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using PicView.Avalonia.CustomControls;

namespace PicView.Avalonia.Views.UC;

public partial class ZoomPreviewer : UserControl
{
    private ZoomPanControl? _zoomPanControl;
    private Control? _childControl;
    private Timer? _hideTimer;

    public ZoomPreviewer()
    {
        InitializeComponent();
        Focusable = false;
    }

    protected override void OnGotFocus(GotFocusEventArgs e)
    {
        // Don't call base to prevent focus
        e.Handled = true;
    }

    public void SetZoomPanControl(ZoomPanControl zoomPanControl)
    {
        _zoomPanControl = zoomPanControl;
        _childControl = zoomPanControl.Child;

        UpdateViewportRect();
    }

    public void UpdateVisibility()
    {
        if (_zoomPanControl == null)
        {
            IsVisible = false;
            return;
        }

        // Show when zoomed in or out (not at 1.0 scale)
        var shouldShow = _zoomPanControl.Scale > 1;
        IsVisible = shouldShow;

        if (shouldShow)
        {
            UpdateViewportRect();
        }

        // Reset the timer to hide the window after 1.5 seconds
        _hideTimer?.Dispose();
        _hideTimer = new Timer(_ => { Dispatcher.UIThread.Post(() => IsVisible = false); }, null,
            TimeSpan.FromSeconds(1.5), Timeout.InfiniteTimeSpan);
    }

    internal void UpdateViewportRect()
    {
        if (_zoomPanControl == null || _childControl == null)
        {
            return;
        }

        var viewportRect = GetCurrentViewportRect();

        // Update the viewport border rectangle
        Canvas.SetLeft(ViewportBorder, viewportRect.X);
        Canvas.SetTop(ViewportBorder, viewportRect.Y);
        ViewportBorder.Width = viewportRect.Width;
        ViewportBorder.Height = viewportRect.Height;
    }

    private Rect GetCurrentViewportRect()
    {
        if (_zoomPanControl == null || _childControl == null)
        {
            return new Rect();
        }

        // Get the viewport rectangle in normalized coordinates (0-1)
        var scale = _zoomPanControl.Scale;
        var translateX = _zoomPanControl.TranslateX;
        var translateY = _zoomPanControl.TranslateY;

        var controlBounds = _zoomPanControl.Bounds;
        var childBounds = _childControl.Bounds;

        if (controlBounds.Width == 0 || controlBounds.Height == 0 ||
            childBounds.Width == 0 || childBounds.Height == 0)
        {
            return new Rect();
        }

        // Calculate what portion of the child is visible in the control
        var visibleLeft = Math.Max(0, -translateX / scale) / childBounds.Width;
        var visibleTop = Math.Max(0, -translateY / scale) / childBounds.Height;
        var visibleRight = Math.Min(1, (controlBounds.Width - translateX) / scale / childBounds.Width);
        var visibleBottom = Math.Min(1, (controlBounds.Height - translateY) / scale / childBounds.Height);

        // Convert to preview window coordinates
        var previewWidth = OverlayCanvas.Bounds.Width;
        var previewHeight = OverlayCanvas.Bounds.Height;

        return new Rect(
            visibleLeft * previewWidth,
            visibleTop * previewHeight,
            (visibleRight - visibleLeft) * previewWidth,
            (visibleBottom - visibleTop) * previewHeight
        );
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        _hideTimer?.Dispose();
    }
}