namespace PicView.Core.Navigation;

public interface IImageIterator : IAsyncDisposable
{
    IReadOnlyList<FileInfo> ImagePaths { get; }
    int CurrentIndex { get; }
    int GetIteration(int index, NavigateTo navigation, bool skip1=false, bool skip10=false, bool skip100=false);
    ValueTask IterateToIndexAsync(int index, CancellationToken ct);
    ValueTask TimerIteration(int index, CancellationTokenSource? cts);
    ValueTask SlimUpdate(int index, object? imageSource);
    ValueTask IterateToIndexSlim(int index, bool isReverse, CancellationToken token);
    ValueTask PreloadAsync();
}
