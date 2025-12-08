using PicView.Core.Navigation.Interfaces;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class NavigationService : INavigationService
{
    private readonly IArchiveService _archive;
    private readonly IImageCache _cache;
    private readonly IImageLoader _imageLoader;

    public NavigationService(IImageLoader imageLoader, IArchiveService archive, IImageCache cache)
    {
        _imageLoader = imageLoader;
        _archive = archive;
        _cache = cache;
    }

    public async ValueTask LoadFromStringAsync(string source, TabViewModel tab, CancellationToken ct)
    {
        // TODO: Implement logic to determine if source is URL, File, etc.
        // For now, assume file.
        var fileInfo = new FileInfo(source);
        if (fileInfo.Exists)
        {
            // Logic to load file into tab
            // This might involve setting up the ImageIterator for the tab?
            // Or just loading the single image?

            // In the new architecture, the TabViewModel manages the Iterator.
            // But NavigationService orchestrates it.

            // Example:
            // tab.ImageIterator.Initialize(new List<FileInfo>{ fileInfo }, 0);
            // await tab.ImageIterator.IterateToIndexAsync(0, ct);
        }
    }

    public async ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationToken ct)
    {
        if (!CanNavigate(tab))
        {
            return;
        }

        if (tab.ImageIterator is null)
        {
            return;
        }

        var next = tab.ImageIterator.GetIteration(tab.ImageIterator.CurrentIndex, to);
        await tab.ImageIterator.IterateToIndexAsync(next, ct).ConfigureAwait(false);
    }

    public ValueTask NavigateToIndexAsync(TabViewModel tab, int index, CancellationToken ct)
    {
        if (tab.ImageIterator is null)
        {
            return ValueTask.CompletedTask;
        }

        return tab.ImageIterator.IterateToIndexAsync(index, ct);
    }

    public bool CanNavigate(TabViewModel tab)
    {
        return tab?.ImageIterator?.Files?.Count > 0;
    }

    public async ValueTask DisposeAsync()
    {
        // Dispose dependencies if needed, or leave it to DI container
    }
}