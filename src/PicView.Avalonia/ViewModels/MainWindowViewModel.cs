using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PicView.Avalonia.UI;
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

    public BindableReactiveProperty<bool> IsFileMenuVisible { get; } = new();

    public BindableReactiveProperty<bool> IsImageMenuVisible { get; } = new();

    public BindableReactiveProperty<bool> IsSettingsMenuVisible { get; } = new();

    public BindableReactiveProperty<bool> IsToolsMenuVisible { get; } = new();
    
    public BindableReactiveProperty<double> TitleMaxWidth { get; } = new();
    
    public BindableReactiveProperty<bool> IsFullscreen { get; } = new();

    public BindableReactiveProperty<bool> IsMaximized { get; } = new();

    public BindableReactiveProperty<bool> ShouldRestore { get; } = new();

    public BindableReactiveProperty<bool> ShouldMaximizeBeShown { get; } = new(true);

    public BindableReactiveProperty<bool> IsLoadingIndicatorShown { get; } = new();

    public BindableReactiveProperty<bool> IsUIShown { get; } = new();
    public BindableReactiveProperty<bool> IsTopToolbarShown { get; } = new();

    public BindableReactiveProperty<bool> IsBottomToolbarShown { get; } = new();

    public BindableReactiveProperty<bool> IsEditableTitlebarOpen { get; } = new();

    public void LayoutButtonSubscription()
    {
        Observable.EveryValueChanged(this, x => x.IsMaximized.CurrentValue, UIHelper.GetFrameProvider)
            .Subscribe(_ => SetButtonValues());
        Observable.EveryValueChanged(this, x => x.IsFullscreen.CurrentValue, UIHelper.GetFrameProvider)
            .Subscribe(_ => SetButtonValues());
    }

    private void SetButtonValues()
    {
        ShouldRestore.Value = IsFullscreen.CurrentValue || IsMaximized.CurrentValue;
        ShouldMaximizeBeShown.Value = !IsFullscreen.CurrentValue && !IsMaximized.CurrentValue;
    }

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
            CurrentView,
            TitleMaxWidth,
            IsLoadingIndicatorShown,
            IsUIShown,
            IsTopToolbarShown,
            IsBottomToolbarShown,
            IsEditableTitlebarOpen);
    }
}