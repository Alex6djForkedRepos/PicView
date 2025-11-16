using PicView.Core.Navigation;

namespace PicView.Core.ViewModels;

public class TabViewModel : IAsyncDisposable
{
    public string Id { get; } = Guid.NewGuid().ToString("N");
    public PicViewerModel PicViewer { get; }
    public IImageIterator ImageIterator { get; }
    public CancellationTokenSource NavigationCts { get; private set; } = new();

    public TabViewModel(PicViewerModel model, IImageIterator iterator)
    {
        PicViewer = model;
        ImageIterator = iterator;
    }

    public void CancelNavigation()
    {
        NavigationCts.Cancel();
        NavigationCts.Dispose();
        NavigationCts = new CancellationTokenSource();
    }

    public void Dispose() => ImageIterator.DisposeAsync().AsTask().Wait();
    public async ValueTask DisposeAsync() => await ImageIterator.DisposeAsync();
}