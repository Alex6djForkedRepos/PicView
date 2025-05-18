using Avalonia.Controls;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using ReactiveUI;

namespace PicView.Avalonia.Views;

public partial class ZoomSettingsView : UserControl
{
    public ZoomSettingsView()
    {
        InitializeComponent();
        Loaded += delegate
        {
            MouseWheelBox.SelectedIndex = Settings.Zoom.CtrlZoom ? 0 : 1;

            MouseWheelBox.SelectionChanged += async delegate
            {
                if (MouseWheelBox.SelectedIndex == -1)
                {
                    return;
                }

                Settings.Zoom.CtrlZoom = MouseWheelBox.SelectedIndex == 0;
                await SaveSettingsAsync();
            };
            MouseWheelBox.DropDownOpened += delegate
            {
                if (MouseWheelBox.SelectedIndex == -1)
                {
                    MouseWheelBox.SelectedIndex = Settings.Zoom.CtrlZoom ? 0 : 1;
                }
            };

            if (DataContext is not MainViewModel vm)
            {
                return;
            }
            vm.SettingsViewModel.WindowMargin = Settings.WindowProperties.Margin;
            vm.SettingsViewModel.ObservableForProperty(x => x.WindowMargin)
                .Subscribe(x =>
                {
                    Settings.WindowProperties.Margin = x.Value;
                    if (Settings.WindowProperties.AutoFit)
                    {
                        WindowResizing.SetSize(vm);
                        WindowFunctions.CenterWindowOnScreen();
                    }
                });
        };
    }
}
