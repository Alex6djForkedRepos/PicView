using System.Collections.ObjectModel;
using PicView.Core.Config;
using PicView.Core.Gallery;
using R3;

namespace PicView.Core.ViewModels;

public class GalleryViewModel : IDisposable
{
    private readonly CompositeDisposable _disposables = new();

    public GalleryViewModel()
    {
        SetStretchModeCommand = new ReactiveCommand<string>();
        SetStretchModeCommand.Subscribe(mode => GalleryStretchService.SetStretch(this, mode)).AddTo(_disposables);

        GalleryItems.Subscribe(_ =>
        {
            GalleryStretchService.SetStretch(this, Settings.Gallery.BottomGalleryStretchMode);
        }).AddTo(_disposables);
        
        // Initial set
        GalleryStretchService.SetStretch(this, Settings.Gallery.BottomGalleryStretchMode);
    }

    public ReactiveCommand<string> SetStretchModeCommand { get; }

    public BindableReactiveProperty <ObservableCollection<GalleryItemViewModel>> GalleryItems { get; } = new([]);

    public BindableReactiveProperty<GalleryDockPosition> GalleryDockPosition { get; } = new(Settings.Gallery.DockPosition);

    public BindableReactiveProperty<GalleryMode> GalleryMode { get; } = new(Core.Gallery.GalleryMode.Closed);

    public BindableReactiveProperty<object> GalleryStretch { get; } = new();
    public BindableReactiveProperty<object> GalleryVerticalAlignment { get; } = new();
    public BindableReactiveProperty<object> GalleryOrientation { get; } = new();

    public BindableReactiveProperty<bool> IsGalleryExpanded { get; } = new();
    
    public BindableReactiveProperty<bool> IsDockedGalleryVisible { get; } = new(Settings.Gallery.IsGalleryDocked);
    public BindableReactiveProperty<bool> IsDockedGalleryShownInHiddenUI { get; } =
        new(Settings.Gallery.ShowBottomGalleryInHiddenUI);

    public void Dispose()
    {
        _disposables.Dispose();
        Disposable.Dispose(GalleryItems,
            GalleryDockPosition,
            IsDockedGalleryVisible,
            IsDockedGalleryShownInHiddenUI,
            GalleryMode,
            GalleryStretch,
            GalleryVerticalAlignment,
            GalleryOrientation,
            IsGalleryExpanded
            );
    }
}
