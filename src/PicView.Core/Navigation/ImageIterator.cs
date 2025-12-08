using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public class ImageIterator : IImageIterator
{
    private readonly IImageCache _cache;
    private readonly int _negativeIterations;
    private readonly TabViewModel _tab;

    // Configuration for preloading window
    private readonly int _positiveIterations;

    private List<FileInfo> _files = [];

    public ImageIterator(IImageCache cache, TabViewModel tab)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _tab = tab ?? throw new ArgumentNullException(nameof(tab));

        // Defaults from settings or injected config
        _positiveIterations = PreLoaderConfig.PositiveIterations;
        _negativeIterations = PreLoaderConfig.NegativeIterations;
    }

    public IReadOnlyList<FileInfo> Files => _files;
    public int CurrentIndex { get; private set; } = -1;

    public async ValueTask IterateToIndexAsync(int index, CancellationToken ct)
    {
        if (index < 0 || index >= _files.Count)
        {
            return;
        }
        
        // Get the current image to ensure it's loaded (User is waiting for this)
        var currentFile = _files[index];
        if (_cache.TryGet(currentFile, out var preLoaded))
        {
            _tab.CurrentModel.Value = preLoaded.ImageModel;
        }
        else
        {
            var loaded = await _cache.GetOrLoadAsync(currentFile, ct).ConfigureAwait(false);
            _tab.CurrentModel.Value = loaded;
        }
        
        CurrentIndex = index;

        // Update background priorities
        UpdateCachePriorities();
    }

    // UI-Agnostic "GetIteration" logic
    public int GetIteration(int index, NavigateTo navigation, bool skip1 = false, bool skip10 = false,
        bool skip100 = false)
    {
        if (_files.Count == 0)
        {
            return -1;
        }

        var skipAmount = skip100 ? 100 : skip10 ? 10 : skip1 ? 2 : 1;
        int next;

        switch (navigation)
        {
            case NavigateTo.Next:
            case NavigateTo.Previous:
                var change = navigation == NavigateTo.Next ? skipAmount : -skipAmount;
                // Assuming looping is handled by caller or config?
                // The original code checked Settings.UIProperties.Looping.
                // We should inject this or assume true/false.
                // For Core, let's implement standard looping logic or make it configurable.
                // Let's assume looping for now as it's common.
                next = (index + change) % _files.Count;
                if (next < 0)
                {
                    next += _files.Count;
                }

                break;

            case NavigateTo.First:
                next = 0;
                break;
            case NavigateTo.Last:
                next = _files.Count - 1;
                break;
            default:
                return index;
        }

        return next;
    }

    public ValueTask DisposeAsync()
    {
        _cache.RemoveOwner(_tab);
        return ValueTask.CompletedTask;
    }

    // Implementing interface stubs
    public ValueTask RepeatNavigateAsync(NavigateTo to, TimeSpan repeatInterval, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public ValueTask SlimUpdate(int index, object? imageSource)
    {
        throw new NotImplementedException();
    }

    public ValueTask IterateToIndexSlim(int index, bool isReverse, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public ValueTask PreloadAsync()
    {
        UpdateCachePriorities();
        return ValueTask.CompletedTask;
    }

    public ValueTask ReloadFileListAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public void Initialize(List<FileInfo> files, int initialIndex = 0)
    {
        _files = files ?? [];
        CurrentIndex = initialIndex;

        if (CurrentIndex < 0 && _files.Count > 0)
        {
            CurrentIndex = 0;
        }

        if (CurrentIndex >= _files.Count)
        {
            CurrentIndex = _files.Count - 1;
        }

        UpdateCachePriorities();
    }

    private void UpdateCachePriorities()
    {
        if (_files.Count == 0)
        {
            return;
        }

        // Calculate window around current index
        // Priority list: [Current, Next, Prev, Next+1, Prev+1, ...]

        var priorities = new List<string> { _files[CurrentIndex].FullName };

        for (var i = 1; i <= Math.Max(_positiveIterations, _negativeIterations); i++)
        {
            if (i <= _positiveIterations)
            {
                var nextIndex = (CurrentIndex + i) % _files.Count;
                priorities.Add(_files[nextIndex].FullName);
            }

            if (i > _negativeIterations)
            {
                continue;
            }

            var prevIndex = (CurrentIndex - i + _files.Count) % _files.Count;
            priorities.Add(_files[prevIndex].FullName);
        }

        _cache.UpdatePriorities(_tab, priorities);
    }
}