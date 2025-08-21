using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using PicView.Avalonia.UI;

namespace PicView.Avalonia.CustomControls;

public partial class AnalogClock : UserControl
{
    private Arc? _elapsedHoursArc;
    private Arc? _elapsedMinutesArc;
    private Rectangle? _hourHand;
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
    private const double ClockMargin = 25; // Desired margin

    private void GenerateClockFace()
    {
        const double arcHalfThickness = 4;
        
        const double textFontSize = 14;
        var numberRadius = ClockRadius - arcHalfThickness - ClockMargin;
        var diameter = ClockRadius * 2;
        var panel = new Panel();

        // --- Draw remaining time arcs (background) ---
        _remainingHoursArc = GetArc(0, 360, diameter - ClockMargin * 4, false, 0.3, "MainBorderColor");
        _remainingHoursArc.Name = "remainingHoursArc";

        _remainingMinutesArc = GetArc(0, 360, diameter, false, 0.2, "MainBorderColor");
        _remainingMinutesArc.Name = "remainingMinutesArc";

        // --- Draw elapsed time arcs (foreground) ---
        _elapsedHoursArc = GetArc(-90, 0, diameter - ClockMargin * 4, true, 1, "AccentColor");
        _elapsedHoursArc.Name = "elapsedHoursArc";

        _elapsedMinutesArc = GetArc(-90, 0, diameter, true, 0.7, "AccentColor");
        _elapsedMinutesArc.Name = "elapsedMinutesArc";

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

    private void CreateClockHands()
    {
        // Hour hand
        _hourHand = new Rectangle
        {
            Width = 6,
            Height = 40,
            Fill = UIHelper.GetBrush("MainTextColor"),
            RenderTransformOrigin = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            RenderTransform = new RotateTransform()
        };

        // Minute hand
        _minuteHand = new Rectangle
        {
            Width = 4,
            Height = 60,
            Fill = UIHelper.GetBrush("MainTextColor"),
            RenderTransformOrigin = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            RenderTransform = new RotateTransform()
        };

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

    private void UpdateHands(DateTime time)
    {
        if (_hourHand == null || _minuteHand == null ||
            _elapsedHoursArc == null || _elapsedMinutesArc == null ||
            _remainingHoursArc == null || _remainingMinutesArc == null)
        {
            return;
        }

        DigitalTime.Text = time.ToShortTimeString();
        DigitalTime.Margin = new Thickness(0, 0, 0, ClockMargin * 3.2);

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