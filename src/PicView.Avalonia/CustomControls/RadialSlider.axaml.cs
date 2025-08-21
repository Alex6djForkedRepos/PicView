using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace PicView.Avalonia.CustomControls;

public class RadialSlider : TemplatedControl
{
    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<RadialSlider, double>(nameof(Minimum), 0);

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<RadialSlider, double>(nameof(Maximum), 100);

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<RadialSlider, double>(nameof(Value), 0, defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<double> RadiusProperty =
        AvaloniaProperty.Register<RadialSlider, double>(nameof(Radius), 100);

    public static readonly StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<RadialSlider, IBrush>(nameof(Stroke), Brushes.Gray);

    public static readonly StyledProperty<IBrush> HighlightBrushProperty =
        AvaloniaProperty.Register<RadialSlider, IBrush>(nameof(HighlightBrush), Brushes.LightBlue);

    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<RadialSlider, double>(nameof(Thickness), 10);

    public static readonly StyledProperty<bool> ShowHighlightOnSecondCycleProperty =
        AvaloniaProperty.Register<RadialSlider, bool>(nameof(ShowHighlightOnSecondCycle), false);

    public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Radius { get => GetValue(RadiusProperty); set => SetValue(RadiusProperty, value); }
    public IBrush Stroke { get => GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
    public IBrush HighlightBrush { get => GetValue(HighlightBrushProperty); set => SetValue(HighlightBrushProperty, value); }
    public double Thickness { get => GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }
    public bool ShowHighlightOnSecondCycle { get => GetValue(ShowHighlightOnSecondCycleProperty); set => SetValue(ShowHighlightOnSecondCycleProperty, value); }

    private Ellipse? _thumb;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _thumb = e.NameScope.Find<Ellipse>("PART_Thumb");

        if (_thumb != null)
        {
            _thumb.PointerPressed += ThumbPressed;
        }
    }

    private void ThumbPressed(object? sender, PointerPressedEventArgs e)
    {
        var canvas = this.GetVisualParent() as Canvas;
        if (canvas == null) return;

        e.Pointer.Capture(_thumb);

        _thumb!.PointerMoved += (s, args) =>
        {
            var pos = args.GetPosition(this);
            var center = new Point(Bounds.Width / 2, Bounds.Height / 2);

            var dx = pos.X - center.X;
            var dy = pos.Y - center.Y;
            var angle = Math.Atan2(dy, dx) * (180 / Math.PI);

            if (angle < 0) angle += 360;

            var newValue = Minimum + (angle / 360.0) * (Maximum - Minimum);
            Value = Math.Clamp(newValue, Minimum, Maximum);
        };
    }

    // Geometry for binding (clockwise circle path)
    public Geometry TrackGeometry =>
        new EllipseGeometry(new Rect(Bounds.Width/2 - Radius, Bounds.Height/2 - Radius, Radius*2, Radius*2));

    public Geometry HighlightGeometry =>
        new EllipseGeometry(new Rect(Bounds.Width/2 - Radius, Bounds.Height/2 - Radius, Radius*2, Radius*2));

    // Thumb position based on Value
    public double ThumbX
    {
        get
        {
            var angle = (Value - Minimum) / (Maximum - Minimum) * 360 * Math.PI / 180;
            return Bounds.Width / 2 + Radius * Math.Cos(angle) - 10;
        }
    }

    public double ThumbY
    {
        get
        {
            var angle = (Value - Minimum) / (Maximum - Minimum) * 360 * Math.PI / 180;
            return Bounds.Height / 2 + Radius * Math.Sin(angle) - 10;
        }
    }
}