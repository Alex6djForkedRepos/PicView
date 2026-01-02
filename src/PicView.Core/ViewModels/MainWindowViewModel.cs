using PicView.Core.IPlatform;
using PicView.Core.Sizing;
using R3;

namespace PicView.Core.ViewModels;

public class MainWindowViewModel : IDisposable
{
    public IFunctionsMapper? Mapper { get; set; }
    public IPlatformWindowService? PlatformWindowService { get; set; }
    
    public TranslationViewModel Translation { get;  } 
    public TopTitlebarViewModel TopTitlebarViewModel { get; }  = new();
    public TabOverviewViewModel WindowTabs { get; } = new();
    public GalleryViewModel Gallery  { get; } = new();
    
    public bool IsNavigationButtonLeftClicked { get; set; }
    public bool IsNavigationButtonRightClicked { get; set; }
    
    public bool IsClickArrowLeftClicked { get; set; }
    public bool IsClickArrowRightClicked { get; set; }

    public bool IsBottomToolbarRotationClicked { get; set; }
    
    public bool IsTitlebarRotationClicked { get; set; }
    
    public Subject<Unit> RequestActive { get; } = new();

    public BindableReactiveProperty<int> BackgroundChoice { get; } = new();

    public BindableReactiveProperty<double> WindowMinWidth { get; } = new(SizeDefaults.WindowMinSize);

    public BindableReactiveProperty<double> SecondaryWindowMinWidth { get; } =
        new(SizeDefaults.SecondaryWindowMinWidth);
    public BindableReactiveProperty<double> WindowMinHeight { get; } = new(SizeDefaults.WindowMinSize);

    public BindableReactiveProperty<double> TitlebarHeight { get; } = new();

    public BindableReactiveProperty<double> BottombarHeight { get; } = new();
    public BindableReactiveProperty<object?> ImageBackground { get; } = new();

    public BindableReactiveProperty<object?> ConstrainedImageBackground { get; } = new();
    public BindableReactiveProperty<object> RightControlOffSetMargin { get; } = new(0);
    public BindableReactiveProperty<object> TopScreenMargin { get; } = new(0);


    // public BindableReactiveProperty<SizeToContent> SizeToContent { get; } = new();
    //
    // public BindableReactiveProperty<ScrollBarVisibility> ToggleScrollBarVisibility { get; } = new();

    public BindableReactiveProperty<bool> CanResize { get; } = new();

    public BindableReactiveProperty<object?> CurrentView { get; } = new();

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
    public BindableReactiveProperty<object> BottomScreenMargin { get; } = new(0);
    public BindableReactiveProperty<object> BottomCornerRadius { get; } = new();

    public BindableReactiveProperty<bool> IsEditableTitlebarOpen { get; } = new();

    public ReactiveCommand ExitCommand { get; }
    
    private async ValueTask Exit(Unit unit, CancellationToken cancellationToken)
    {
        await Mapper.Exit();
    }

    public ReactiveCommand MaximizeCommand { get; }
    private async ValueTask Maximize(Unit unit, CancellationToken cancellationToken)
    {
        await Mapper.Maximize();
    }

    public ReactiveCommand MinimizeCommand { get; }
    private async ValueTask Minimize(Unit unit, CancellationToken cancellationToken)
    {
        await Mapper.Minimize();
    }

    public ReactiveCommand RestoreCommand { get; }
    private async ValueTask Restore(Unit unit, CancellationToken cancellationToken)
    {
        await Mapper.Restore();
    }

    public ReactiveCommand ToggleFullscreenCommand { get; }
    private async ValueTask ToggleFullscreen(Unit unit, CancellationToken cancellationToken)
    {
        await Mapper.ToggleFullscreen();
    }
    
    public ReactiveCommand OpenCommand { get; }
    private async ValueTask Open(Unit unit, CancellationToken cancellationToken)
    {
        await Mapper.Open();
    }    
    public MainWindowViewModel(TranslationViewModel translations, IPlatformWindowService windowService)
    {
        Translation = translations;
        PlatformWindowService = windowService;
        
        ExitCommand = new ReactiveCommand(Exit);
        MaximizeCommand = new ReactiveCommand(Maximize);
        MinimizeCommand = new ReactiveCommand(Minimize);
        RestoreCommand = new ReactiveCommand(Restore);
        ToggleFullscreenCommand = new ReactiveCommand(ToggleFullscreen);
        OpenCommand = new ReactiveCommand(Open);
    }

    public void Dispose()
    {
        Disposable.Dispose(
            BackgroundChoice,
            WindowMinWidth,
            WindowMinHeight,
            TitlebarHeight,
            BottombarHeight,
            CanResize,
            CurrentView,
            TitleMaxWidth,
            IsLoadingIndicatorShown,
            IsUIShown,
            IsTopToolbarShown,
            IsBottomToolbarShown,
            IsEditableTitlebarOpen);
    }
    
    // Call this method from the View's "Activated" or "GotFocus" event
    public void SetAsActive()
    {
        RequestActive.OnNext(Unit.Default);
    }

    public void LayoutButtonSubscription()
    {
    }

    public static void HoverBarSubscription()
    {
    }


    private void SetButtonValues()
    {
        ShouldRestore.Value = IsFullscreen.CurrentValue || IsMaximized.CurrentValue;
        ShouldMaximizeBeShown.Value = !IsFullscreen.CurrentValue && !IsMaximized.CurrentValue;
    }
}