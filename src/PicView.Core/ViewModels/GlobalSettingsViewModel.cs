using R3;

namespace PicView.Core.ViewModels;

public class GlobalSettingsViewModel : IDisposable
{

    public BindableReactiveProperty<double> RotationAngle { get; } = new();

    public BindableReactiveProperty<double> ZoomValue { get; } = new();
    public void Dispose()
    {
        Disposable.Dispose();
    }
}