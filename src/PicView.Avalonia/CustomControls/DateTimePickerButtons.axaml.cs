using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace PicView.Avalonia.CustomControls;

public partial class DateTimePickerButtons : UserControl
{
    public static readonly StyledProperty<DateTime?> DateProperty =
        AvaloniaProperty.Register<DateTimePickerButtons, DateTime?>(nameof(Date));


    private Calendar? _calendar;
    private AnalogClock? _clock;
    
    public DateTime? Date
    {
        get => GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    public DateTimePickerButtons()
    {
        InitializeComponent();
        Loaded += delegate
        {
            Date ??= DateTime.Now;
            _calendar = new Calendar
            {
                SelectionMode = CalendarSelectionMode.SingleDate,
                SelectedDate = Date.Value,
                BorderThickness = new Thickness(1,0,1,1)
            };
            
            var calendarFlyout = new Flyout
            {
                Placement = PlacementMode.Top,
                ShowMode = FlyoutShowMode.Standard,
                Content = _calendar
            };
            FlyoutBase.SetAttachedFlyout(CalendarButton, calendarFlyout);
            CalendarButton.Click += (_, _) => { ShowPopUpControl(true); };
            
            _clock = new AnalogClock
            {
                SelectedTime = Date.Value
            };
            var timePickerFlyout = new Flyout
            {
                Placement = PlacementMode.Top,
                ShowMode = FlyoutShowMode.Standard,
                Content = _clock,
                HorizontalOffset = -TimePickerButton.Width / 2 + 5
            };
            FlyoutBase.SetAttachedFlyout(TimePickerButton, timePickerFlyout);
            TimePickerButton.Click += (_, _) => { ShowPopUpControl(false); };

            _calendar.SelectedDatesChanged += CalendarOnDisplayDateChanged;
        };
    }

    private void ShowPopUpControl(bool calendar)
    {
        if (Date.HasValue)
        {
            _calendar.SelectedDate = Date.Value;
            _clock.SelectedTime = Date.Value;
        }

        if (calendar)
        {
            _calendar.IsVisible = true;
            _calendar.Opacity = 1;
        }
        else
        {
            _clock.IsVisible = true;
            _clock.Opacity = 1;
        }
        FlyoutBase.ShowAttachedFlyout(calendar ? CalendarButton : TimePickerButton);
    }

    private void TimePickerOnSelectedTimeChanged(object? sender, TimePickerSelectedValueChangedEventArgs e)
    {
        UpdateTimeAndDate();
    }

    private void CalendarOnDisplayDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateTimeAndDate();
    }

    private void UpdateTimeAndDate()
    {
        if (_calendar.SelectedDate == null)
        {
            return;
        }
        var date = _calendar.SelectedDate.Value;
        var time = DateTime.Now.TimeOfDay;
        Date = new DateTime(new DateOnly(date.Year, date.Month, date.Day), new TimeOnly(time.Hours, time.Minutes));
    }
}