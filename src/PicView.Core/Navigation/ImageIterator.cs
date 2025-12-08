using PicView.Core.Navigation.Interfaces;

namespace PicView.Core.Navigation;

public class ImageIterator : IImageIterator
{
    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
    
    public IReadOnlyList<FileInfo> Files { get; }
    public int CurrentIndex { get; }
    public int GetIteration(int index, NavigateTo navigation, bool skip1 = false, bool skip10 = false, bool skip100 = false)
    {
        throw new NotImplementedException();
    }

    public ValueTask IterateToIndexAsync(int index, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

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
        throw new NotImplementedException();
    }

    public ValueTask ReloadFileListAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
