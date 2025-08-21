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
            _calendar = new Calendar
            {
                SelectionMode = CalendarSelectionMode.SingleDate,
                SelectedDate = Date,
                BorderThickness = new Thickness(1,0,1,1)
            };
            
            var calendarFlyout = new Flyout
            {
                Placement = PlacementMode.Top,
                ShowMode = FlyoutShowMode.Standard,
                Content = _calendar
            };
            FlyoutBase.SetAttachedFlyout(CalendarButton, calendarFlyout);
            CalendarButton.Click += (_, _) => { FlyoutBase.ShowAttachedFlyout(CalendarButton); };
            
            _clock = new AnalogClock
            {
            };
            var timePickerFlyout = new Flyout
            {
                Placement = PlacementMode.Top,
                ShowMode = FlyoutShowMode.Standard,
                Content = _clock
            };
            FlyoutBase.SetAttachedFlyout(TimePickerButton, timePickerFlyout);
            TimePickerButton.Click += (_, _) => { FlyoutBase.ShowAttachedFlyout(TimePickerButton); };

            _calendar.SelectedDatesChanged += CalendarOnDisplayDateChanged;
        };
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