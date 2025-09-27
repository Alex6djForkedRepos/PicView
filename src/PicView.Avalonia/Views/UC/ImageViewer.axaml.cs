using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ImageTransformations;
using PicView.Avalonia.ImageTransformations.Rotation;
using PicView.Avalonia.Input;
using PicView.Avalonia.ViewModels;
using PicView.Core.Exif;

namespace PicView.Avalonia.Views.UC;


public partial class ImageViewer : UserControl
{
    private RotationTransformer? _imageTransformer;
    
    public ImageViewer()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        InitializeImageTransformer();
        AddHandler(PointerWheelChangedEvent, PreviewOnPointerWheelChanged, RoutingStrategies.Tunnel);
        AddHandler(Gestures.PointerTouchPadGestureMagnifyEvent, TouchMagnifyEvent, RoutingStrategies.Bubble);
        AddHandler(Gestures.PinchEvent, TouchMagnifyEvent, RoutingStrategies.Bubble);
        ImageControlHelper.TriggerScalingModeUpdate(MainImage, false);
        InitializeMouseInputHelper();
    }

    public void TriggerScalingModeUpdate(bool invalidate) =>
        ImageControlHelper.TriggerScalingModeUpdate(MainImage, invalidate);

    private void TouchMagnifyEvent(object? sender, PointerDeltaEventArgs e) =>
        ZoomPanControl.ZoomWithPointerWheel(e);

    public static async Task PreviewOnPointerWheelChanged(object? sender, PointerWheelEventArgs e) =>
        await MouseShortcuts.HandlePointerWheelChanged(e);

    public void InitializeImageTransformer()
    {
        ZoomPanControl.Initialize();
        _imageTransformer = new RotationTransformer(
            ImageLayoutTransformControl,
            MainImage,
            () => DataContext,
            () =>
            {
                ResetZoom(false);
            });
    }

    private void InitializeMouseInputHelper() =>
        MouseShortcuts.InitializeMouseShortcuts(
            ImageScrollViewer,
            async e => { await Dispatcher.UIThread.InvokeAsync(() => { ZoomIn(e); }); },
            async e => { await Dispatcher.UIThread.InvokeAsync(() => { ZoomOut(e); }); });

    #region Zoom
    /// <inheritdoc cref="Zoom.ZoomIn(MainViewModel)"/>
    private void ZoomIn(PointerWheelEventArgs e) =>
        ZoomPanControl.ZoomWithPointerWheel(e);

    /// <inheritdoc cref="Zoom.ZoomOut(MainViewModel)"/>
    private void ZoomOut(PointerWheelEventArgs e) =>
        ZoomPanControl.ZoomWithPointerWheel(e);

    /// <inheritdoc cref="Zoom.ZoomIn(MainViewModel)"/>
    public void ZoomIn() =>
        ZoomPanControl.ZoomIn();

    /// <inheritdoc cref="Zoom.ZoomOut(MainViewModel)"/>
    public void ZoomOut() =>
        ZoomPanControl.ZoomOut();

    /// <inheritdoc cref="Zoom.ResetZoom(bool, MainViewModel)"/>
    public void ResetZoom(bool enableAnimations = true) =>
        ZoomPanControl.ResetZoom(enableAnimations, false);
    #endregion

    #region Image Transformation
    public void Rotate(bool clockWise) => _imageTransformer?.Rotate(clockWise);
    public void Rotate(double angle) => _imageTransformer?.Rotate(angle);
    public void Flip(bool animate) => _imageTransformer?.Flip(animate);
    public void SetTransform(ExifOrientation? orientation, MagickFormat? format, bool reset = true) =>
        _imageTransformer?.SetTransform(orientation, format, reset);
    #endregion
}