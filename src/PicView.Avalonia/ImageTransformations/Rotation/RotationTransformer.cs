using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.Exif;
using PicView.Core.ImageTransformations;

namespace PicView.Avalonia.ImageTransformations.Rotation;

public class RotationTransformer(
    LayoutTransformControl imageLayoutTransformControl,
    PicBox mainImage,
    Func<object?> getDataContext,
    Action resetZoom)
{
    public void Rotate(bool clockWise)
    {
        if (getDataContext() is not MainViewModel vm || mainImage.Source is null)
        {
            return;
        }

        if (RotationHelper.IsValidRotation(vm.PicViewer.RotationAngle.CurrentValue))
        {
            var nextAngle = RotationHelper.Rotate(vm.PicViewer.RotationAngle.CurrentValue, clockWise);
            vm.PicViewer.RotationAngle.Value = nextAngle switch
            {
                360 => 0,
                -90 => 270,
                _ => nextAngle
            };
        }
        else
        {
            vm.PicViewer.RotationAngle.Value =
                RotationHelper.NextRotationAngle(vm.PicViewer.RotationAngle.CurrentValue, true);
        }

        SetImageLayoutTransform(new RotateTransform(vm.PicViewer.RotationAngle.CurrentValue));
        WindowResizing.SetSize(vm);
        mainImage.InvalidateVisual();
    }

    public void Rotate(double angle)
    {
        SetImageLayoutTransform(new RotateTransform(angle));
        WindowResizing.SetSize(getDataContext() as MainViewModel);
        mainImage.InvalidateVisual();
    }

    private void SetImageLayoutTransform(RotateTransform rotateTransform)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            imageLayoutTransformControl.LayoutTransform = rotateTransform;
        }
        else
        {
            Dispatcher.UIThread.Invoke(() =>
                imageLayoutTransformControl.LayoutTransform = rotateTransform);
        }
    }

    private ScaleTransform? _scaleTransform;
    public void Flip(bool animate)
    {
        if (getDataContext() is not MainViewModel vm || mainImage.Source is null)
        {
            return;
        }
        
        _scaleTransform ??= new ScaleTransform();

        var prevScaleX = vm.PicViewer.ScaleX.CurrentValue;
        var newScaleX = prevScaleX == -1 ? 1 : -1;

        if (animate)
        {
            _scaleTransform.Transitions ??=
            [
                new DoubleTransition
                {
                    Property = ScaleTransform.ScaleXProperty,
                    Duration = TimeSpan.FromSeconds(.2)
                }
            ];
        }
        else
        {
            _scaleTransform.Transitions = null;
        }
        imageLayoutTransformControl.RenderTransform = _scaleTransform;
        _scaleTransform.ScaleX = newScaleX;
    }

}