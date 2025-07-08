using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using R3;

namespace PicView.Avalonia.ViewModels;

public class MainWindowViewModel : IDisposable
{
    public BindableReactiveProperty<Brush?> ImageBackground { get; } = new();

    public BindableReactiveProperty<Brush?> ConstrainedImageBackground { get; } = new();

    public BindableReactiveProperty<Thickness> RightControlOffSetMargin { get; } = new();

    public BindableReactiveProperty<Thickness> TopScreenMargin { get; } = new();

    public BindableReactiveProperty<Thickness> BottomScreenMargin { get; } = new();

    public BindableReactiveProperty<CornerRadius> BottomCornerRadius { get; } = new();

    public BindableReactiveProperty<int> BackgroundChoice { get; } = new();

    public BindableReactiveProperty<double> WindowMinSize { get; } = new();

    public BindableReactiveProperty<double> TitlebarHeight { get; } = new();

    public BindableReactiveProperty<double> BottombarHeight { get; } = new();
    
    public BindableReactiveProperty<SizeToContent> SizeToContent { get; } = new();

    public BindableReactiveProperty<bool> CanResize { get; } = new();

    public BindableReactiveProperty<UserControl?> CurrentView { get; } = new();

    public void Dispose()
    {
        Disposable.Dispose(ImageBackground,
            ConstrainedImageBackground,
            RightControlOffSetMargin,
            TopScreenMargin,
            BottomScreenMargin,
            BottomCornerRadius,
            BackgroundChoice,
            WindowMinSize,
            TitlebarHeight,
            BottombarHeight,
            SizeToContent,
            CanResize,
            CurrentView);
    }
}