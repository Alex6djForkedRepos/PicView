using System.Runtime;
using Avalonia;
using Avalonia.Markup.Xaml;
using PicView.Avalonia.Win32.Views;

namespace PicView.Avalonia.Win32;

public class App : Application
{
    private static WinMainWindow2? _mainWindow;
    public override void Initialize()
    {
#if DEBUG
        ProfileOptimization.SetProfileRoot(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config/"));
        ProfileOptimization.StartProfile("ProfileOptimization");
#endif
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        _mainWindow = new WinMainWindow2(false);
    }

}