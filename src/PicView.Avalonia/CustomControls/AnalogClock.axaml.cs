using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using PicView.Avalonia.UI;

namespace PicView.Avalonia.CustomControls;

public partial class AnalogClock : UserControl
{
    private const double ClockMargin = 25;
    private Point _centerPoint;
    private Arc? _elapsedHoursArc;
    private Arc? _elapsedMinutesArc;
    private Rectangle? _hourHand;
    private bool _isDraggingHours;
    private bool _isDraggingMinutes;
    private Rectangle? _minuteHand;
    private Arc? _remainingHoursArc;
    private Arc? _remainingMinutesArc;

    public AnalogClock()
    {
        InitializeComponent();
        GenerateClockFace();
        UpdateHands(DateTime.Now);
    }

    private double ClockRadius => Width / 2;

    private void GenerateClockFace()
    {
        DigitalTime.Margin = new Thickness(0, 0, 0, ClockMargin * 3.2);

        const double arcHalfThickness = 4;
        const double textFontSize = 14;
        var numberRadius = ClockRadius - arcHalfThickness - ClockMargin;
        var diameter = ClockRadius * 2;
        var panel = new Panel();

        _centerPoint = new Point(ClockRadius, ClockRadius);

        // --- Draw remaining time arcs (background) ---
        _remainingHoursArc = GetArc(0, 360, diameter - ClockMargin * 4, false, 0.3, "MainBorderColor");
        _remainingHoursArc.Name = "remainingHoursArc";

        _remainingMinutesArc = GetArc(0, 360, diameter, false, 0.2, "MainBorderColor");
        _remainingMinutesArc.Name = "remainingMinutesArc";

        // --- Draw elapsed time arcs (foreground) ---
        _elapsedHoursArc = GetArc(-90, 0, diameter - ClockMargin * 4, true, 1, "AccentColor");
        _elapsedHoursArc.Name = "elapsedHoursArc";
        _elapsedHoursArc.Cursor = new Cursor(StandardCursorType.Hand);

        _elapsedMinutesArc = GetArc(-90, 0, diameter, true, 0.7, "AccentColor");
        _elapsedMinutesArc.Name = "elapsedMinutesArc";
        _elapsedMinutesArc.Cursor = new Cursor(StandardCursorType.Hand);

        // Add pointer events
        _elapsedHoursArc.PointerPressed += ElapsedHoursArc_PointerPressed;
        _elapsedHoursArc.PointerReleased += ElapsedArc_PointerReleased;
        _elapsedHoursArc.PointerMoved += ElapsedHoursArc_PointerMoved;

        _elapsedMinutesArc.PointerPressed += ElapsedMinutesArc_PointerPressed;
        _elapsedMinutesArc.PointerReleased += ElapsedArc_PointerReleased;
        _elapsedMinutesArc.PointerMoved += ElapsedMinutesArc_PointerMoved;

        MainPanel.Children.Add(_remainingHoursArc);
        MainPanel.Children.Add(_remainingMinutesArc);
        MainPanel.Children.Add(_elapsedHoursArc);
        MainPanel.Children.Add(_elapsedMinutesArc);

        // --- Numbers ---
        var canvas = new Canvas();
        for (var h = 1; h <= 12; h++)
        {
            double angleDeg = h * 30 - 90;
            var angleRad = angleDeg * Math.PI / 180;
            var x = ClockRadius + numberRadius * Math.Cos(angleRad);
            var y = ClockRadius + numberRadius * Math.Sin(angleRad);
            var text = new TextBlock { Text = h.ToString(), FontSize = textFontSize, Classes = { "txt" } };
            text.Measure(Size.Infinity);
            var size = text.DesiredSize;
            Canvas.SetLeft(text, x - size.Width / 2);
            Canvas.SetTop(text, y - size.Height / 2);
            canvas.Children.Add(text);
        }

        panel.Children.Add(canvas);
        MainPanel.Children.Add(panel);

        CreateClockHands();
    }

    private void ElapsedHoursArc_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDraggingHours = true;
        e.Pointer.Capture(sender as IInputElement);
    }

    private void ElapsedMinutesArc_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _isDraggingMinutes = true;
        e.Pointer.Capture(sender as IInputElement);
    }

    private void ElapsedArc_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDraggingHours = false;
        _isDraggingMinutes = false;
        e.Pointer.Capture(null);
    }

    private void ElapsedHoursArc_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingHours)
        {
            return;
        }

        var point = e.GetPosition(MainPanel);
        UpdateArcFromPoint(point, true);
    }

    private void ElapsedMinutesArc_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDraggingMinutes)
        {
            return;
        }

        var point = e.GetPosition(MainPanel);
        UpdateArcFromPoint(point, false);
    }

    private void UpdateArcFromPoint(Point point, bool isHours)
    {
        var angle = Math.Atan2(point.Y - _centerPoint.Y, point.X - _centerPoint.X);
        // Convert to degrees and adjust to start from top (90 degrees)
        var degrees = (angle * 180 / Math.PI + 90) % 360;
        if (degrees < 0)
        {
            degrees += 360;
        }

        if (isHours)
        {
            if (_elapsedHoursArc != null && _remainingHoursArc != null)
            {
                _elapsedHoursArc.SweepAngle = degrees;
                _remainingHoursArc.StartAngle = -90 + degrees;
                _remainingHoursArc.SweepAngle = 360 - degrees;

                // Update hour hand
                if (_hourHand != null)
                {
                    ((RotateTransform)_hourHand.RenderTransform!).Angle = degrees;
                }
            }
        }
        else
        {
            if (_elapsedMinutesArc != null && _remainingMinutesArc != null)
            {
                _elapsedMinutesArc.SweepAngle = degrees;
                _remainingMinutesArc.StartAngle = -90 + degrees;
                _remainingMinutesArc.SweepAngle = 360 - degrees;

                // Update minute hand
                if (_minuteHand != null)
                {
                    ((RotateTransform)_minuteHand.RenderTransform!).Angle = degrees;
                }
            }
        }

        // Update digital time display
        UpdateDigitalTimeFromAngles();
    }

    private void UpdateDigitalTimeFromAngles()
    {
        if (_elapsedHoursArc == null || _elapsedMinutesArc == null)
        {
            return;
        }

        var hours = (int)(_elapsedHoursArc.SweepAngle / 30) % 12;
        if (hours == 0)
        {
            hours = 12;
        }

        var minutes = (int)(_elapsedMinutesArc.SweepAngle / 6) % 60;

        DigitalTime.Text = $"{hours:D2}:{minutes:D2}";
    }

    private static Arc GetArc(double startAngle, double sweepAngle, double diameter, bool fill, double opacity,
        string colorResource)
    {
        var stroke = fill ? UIHelper.GetSolidColorBrush(colorResource) : UIHelper.GetBrush(colorResource);
        return new Arc
        {
            Stroke = stroke,
            StrokeThickness = 8,
            StrokeJoin = PenLineJoin.Round,
            StrokeLineCap = PenLineCap.Round,
            StartAngle = startAngle,
            SweepAngle = sweepAngle,
            Width = diameter,
            Height = diameter,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Opacity = opacity
        };
    }

private bool _isDraggingHourHand;
private bool _isDraggingMinuteHand;

// Modify the CreateClockHands method to add event handlers to the hands
private void CreateClockHands()
{
    // Hour hand
    _hourHand = new Rectangle
    {
        Width = 6,
        Height = 40,
        Fill = UIHelper.GetBrush("MainTextColor"),
        RenderTransformOrigin = new RelativePoint(0.5, 1, RelativeUnit.Relative),
        RenderTransform = new RotateTransform(),
        Cursor = new Cursor(StandardCursorType.Hand) // Add cursor indicator
    };

    // Minute hand
    _minuteHand = new Rectangle
    {
        Width = 4,
        Height = 60,
        Fill = UIHelper.GetBrush("MainTextColor"),
        RenderTransformOrigin = new RelativePoint(0.5, 1, RelativeUnit.Relative),
        RenderTransform = new RotateTransform(),
        Cursor = new Cursor(StandardCursorType.Hand) // Add cursor indicator
    };

    // Add pointer events to hands
    _hourHand.PointerPressed += HourHand_PointerPressed;
    _hourHand.PointerReleased += Hand_PointerReleased;
    _hourHand.PointerMoved += HourHand_PointerMoved;

    _minuteHand.PointerPressed += MinuteHand_PointerPressed;
    _minuteHand.PointerReleased += Hand_PointerReleased;
    _minuteHand.PointerMoved += MinuteHand_PointerMoved;

    // Position both hands at center bottom
    Canvas.SetLeft(_hourHand, ClockRadius - _hourHand.Width / 2);
    Canvas.SetTop(_hourHand, ClockRadius - _hourHand.Height);

    Canvas.SetLeft(_minuteHand, ClockRadius - _minuteHand.Width / 2);
    Canvas.SetTop(_minuteHand, ClockRadius - _minuteHand.Height);

    // Add to existing canvas
    var canvas = MainPanel.Children.OfType<Panel>().First().Children.OfType<Canvas>().First();
    canvas.Children.Add(_hourHand);
    canvas.Children.Add(_minuteHand);
}

// Add these event handlers for the hands
private void HourHand_PointerPressed(object sender, PointerPressedEventArgs e)
{
    _isDraggingHourHand = true;
    e.Pointer.Capture(_hourHand);
    e.Handled = true;
}

private void MinuteHand_PointerPressed(object sender, PointerPressedEventArgs e)
{
    _isDraggingMinuteHand = true;
    e.Pointer.Capture(_minuteHand);
    e.Handled = true;
}

private void Hand_PointerReleased(object sender, PointerReleasedEventArgs e)
{
    _isDraggingHourHand = false;
    _isDraggingMinuteHand = false;
    e.Pointer.Capture(null);
}

private void HourHand_PointerMoved(object sender, PointerEventArgs e)
{
    if (!_isDraggingHourHand) return;
    
    var point = e.GetPosition(MainPanel);
    UpdateArcFromPoint(point, true);
    e.Handled = true;
}

private void MinuteHand_PointerMoved(object sender, PointerEventArgs e)
{
    if (!_isDraggingMinuteHand) return;
    
    var point = e.GetPosition(MainPanel);
    UpdateArcFromPoint(point, false);
    e.Handled = true;
}

    private void UpdateHands(DateTime time)
    {
        if (_hourHand == null || _minuteHand == null ||
            _elapsedHoursArc == null || _elapsedMinutesArc == null ||
            _remainingHoursArc == null || _remainingMinutesArc == null)
        {
            return;
        }

        DigitalTime.Text = time.ToShortTimeString();

        // Update hour arcs
        var elapsedHoursAngle = time.Hour % 12 * 30 + time.Minute;
        _elapsedHoursArc.StartAngle = -90;
        _elapsedHoursArc.SweepAngle = elapsedHoursAngle;

        // Update remaining hour arc to show time until next 12-hour cycle
        _remainingHoursArc.StartAngle = -90 + elapsedHoursAngle;
        _remainingHoursArc.SweepAngle = 360 - elapsedHoursAngle;

        // Update minute arcs
        double elapsedMinutesAngle = time.Minute * 6;
        _elapsedMinutesArc.StartAngle = -90;
        _elapsedMinutesArc.SweepAngle = elapsedMinutesAngle;

        // Update remaining minute arc to show time until next hour
        _remainingMinutesArc.StartAngle = -90 + elapsedMinutesAngle;
        _remainingMinutesArc.SweepAngle = 360 - elapsedMinutesAngle;

        // Apply rotations to hands
        ((RotateTransform)_hourHand.RenderTransform!).Angle = elapsedHoursAngle;
        ((RotateTransform)_minuteHand.RenderTransform!).Angle = elapsedMinutesAngle;
    }
}