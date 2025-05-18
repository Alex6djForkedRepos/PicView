using Avalonia.Controls;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using ReactiveUI;

namespace PicView.Avalonia.Views;

public partial class WindowSettingsView : UserControl
{
    public WindowSettingsView()
    {
        InitializeComponent();
        Loaded += delegate
        {
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
