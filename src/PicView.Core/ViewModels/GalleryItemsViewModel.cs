using R3;

namespace PicView.Core.ViewModels;

public class GalleryItemsViewModel : IDisposable
{
    public void Dispose()
    {
        Disposable.Dispose(Image,
            FileName,
            FileLocation,
            FileSize,
            FileDate);
    }

    public BindableReactiveProperty<object> Image { get; } = new();
    
    public BindableReactiveProperty<string> FileName { get; } = new();
    public BindableReactiveProperty<string> FileLocation { get; } = new();
    public BindableReactiveProperty<string> FileSize { get; } = new();
    public BindableReactiveProperty<string> FileDate { get; } = new();
    
}