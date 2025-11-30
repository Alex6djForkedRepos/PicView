using PicView.Core.Navigation;
using R3;

namespace PicView.Core.ViewModels;

public class TabViewModel : IAsyncDisposable
{
    public string Id { get; init; }
    public PicViewerModel PicViewer { get; }
    public IImageIterator? ImageIterator { get; }
    public CancellationTokenSource NavigationCts { get; private set; } = new();
    
    public string? TabTitle { get; set; } = string.Empty;
    public string? TabTooltip { get; set; } = string.Empty;
    

    public TabViewModel(PicViewerModel model, IImageIterator iterator, string id, Func<string, ValueTask> closeTab)
    {
        PicViewer = model;
        ImageIterator = iterator;
        Id = id;
        CloseTabCommand = new ReactiveCommand(async _ =>
        {
            TabTitle = null; // Signal it to be removed from the UI
            await closeTab.Invoke(id);
        });
    }

    public ReactiveCommand CloseTabCommand { get; }

    public void CancelNavigation()
    {
        NavigationCts.Cancel();
        NavigationCts.Dispose();
        NavigationCts = new CancellationTokenSource();
    }

    public void Dispose() => ImageIterator.DisposeAsync().AsTask().Wait();
    public async ValueTask DisposeAsync()
    {
        if (ImageIterator is not null)
        {
            await ImageIterator.DisposeAsync();
        }
    }
}