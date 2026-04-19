using System.Diagnostics;
using Avalonia.Threading;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.ArchiveHandling;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.Gallery;
using PicView.Core.Models;
using PicView.Core.Navigation;
using PicView.Core.Preloading;
using Timer = System.Timers.Timer;

namespace PicView.Avalonia.Navigation;

// TODO deprecated, delete
public class ImageIterator : IAsyncDisposable
{
    #region Properties

    private bool _disposed;

    public List<FileInfo> ImagePaths { get; private set; }
    public int CurrentIndex { get; private set; }
    
    public bool IsWatcherEnabled { get; set; } = Settings.Navigation.IsFileWatcherEnabled;

    public int GetNonZeroIndex => CurrentIndex + 1 > GetCount ? 1 : CurrentIndex + 1;

    public int NextIndex => GetIteration(CurrentIndex, NavigateTo.Next);

    public int GetCount => ImagePaths.Count;

    public FileInfo InitialFileInfo { get; private set; } = null!;
    public bool IsReversed { get; private set; }
    private PreLoader PreLoader { get; } = new(GetImageModel.GetImageModelAsync);

    private static FileSystemWatcher? _watcher;

    private bool _isRunning;

    private readonly MainViewModel? _vm;

    #endregion

    #region Constructors

    public ImageIterator(FileInfo fileInfo, MainViewModel vm, bool setInitial = true)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(fileInfo);
#endif
        _vm = vm;
        FileInfo initialDirectory;
        
        // If setInitial is true, we want to continue from where we left off
        if (Settings.Sorting.IncludeSubDirectories && setInitial)
        {
            if (!string.IsNullOrWhiteSpace(Settings.StartUp.StartUpDirectory) && !ArchiveExtraction.IsArchived)
            {
                if (fileInfo.FullName.Contains(Settings.StartUp.StartUpDirectory))
                {
                    initialDirectory = new FileInfo(Settings.StartUp.StartUpDirectory);
                }
                else
                {
                    initialDirectory = new FileInfo(fileInfo.DirectoryName);
                }
            }
            else
            {
                initialDirectory = new FileInfo(fileInfo.DirectoryName);
            }
        }
        else
        {
            initialDirectory = new FileInfo(fileInfo.DirectoryName);
        }
        ImagePaths = vm.PlatformService.GetFiles(initialDirectory);
        CurrentIndex = ImagePaths.FindIndex(x => x.FullName.Equals(fileInfo.FullName));
        InitiateFileSystemWatcher(fileInfo);
        if (setInitial)
        {
            Settings.StartUp.StartUpDirectory = initialDirectory.FullName;
        }
        
        vm.PicViewer.Maximum.Value = ImagePaths.Count;
    }

    public ImageIterator(FileInfo fileInfo, List<FileInfo> imagePaths, int currentIndex, MainViewModel vm)
    {
#if DEBUG
        ArgumentNullException.ThrowIfNull(fileInfo);
#endif
        _vm = vm;
        vm.PicViewer.Maximum.Value = imagePaths.Count;
        ImagePaths = imagePaths;
        CurrentIndex = currentIndex;
        InitiateFileSystemWatcher(fileInfo);
    }

    #endregion

    #region File Watcher

    private void InitiateFileSystemWatcher(FileInfo fileInfo)
    {
       
    }

    public async ValueTask AddFile(string fileName)
    {
        var fileInfo = new FileInfo(fileName);
        if (!fileInfo.Exists)
        {
            return;
        }
        var sourceFileInfo = Settings.Sorting.IncludeSubDirectories
            ? new FileInfo(_watcher.Path)
            : fileInfo;

        var newList = await Task.FromResult(_vm.PlatformService.GetFiles(sourceFileInfo));
        if (newList.Count == 0)
        {
            return;
        }

        ImagePaths = newList;
        
        TitleManager.SetTitle(_vm);

        var index = ImagePaths.FindIndex(x => x.FullName.Equals(fileName));
        if (index < 0)
        {
            PreLoader.Resynchronize(ImagePaths);
            _isRunning = false;
            return;
        }

        PreLoader.Resynchronize(ImagePaths);
    }

    private async ValueTask OnFileDeleted(FileSystemEventArgs e)
    {
        try
        {
            _isRunning = true;

            var index = ImagePaths.FindIndex(x => x.FullName.Equals(e.FullPath));
            if (index < 0)
            {
                return;
            }

            ImagePaths.RemoveAt(index);
            if (ImagePaths.Count <= 0)
            {
                ErrorHandling.ShowStartUpMenu(_vm);
                return;
            }

            var currentIndex = CurrentIndex;
            var isSameFile = currentIndex == index;

            RemoveItemFromPreLoader(index);
            PreLoader.Resynchronize(ImagePaths);
            
            if (isSameFile)
            {
                if (Settings.Navigation.IsNavigatingBackwardsWhenDeleting)
                {
                    await IterateToIndex(GetIteration(index, NavigateTo.Previous), new CancellationTokenSource());
                }
                else
                {
                    await IterateToIndex(index, new CancellationTokenSource());
                }
            }
            else
            {
                TitleManager.SetTitle(_vm);
            }


            FileHistoryManager.Remove(e.FullPath);
        }
        catch (Exception exception)
        {
            DebugHelper.LogDebug(nameof(ImageIterator), nameof(OnFileDeleted), exception);
        }
        finally
        {
            _isRunning = false;
        }
    }

    private async ValueTask OnFileRenamed(RenamedEventArgs e)
    {
    }

    #endregion

    #region Preloader

    public async ValueTask ClearAsync() =>
        await PreLoader.ClearAsync().ConfigureAwait(false);

    public async ValueTask PreloadAsync() =>
        await PreLoader.PreLoadAsync(CurrentIndex, IsReversed, ImagePaths).ConfigureAwait(false);

    public void Add(int index, ImageModel imageModel) =>
        PreLoader.Add(index, ImagePaths, imageModel, IsReversed);

    public bool Add(FileInfo file, ImageModel imageModel) =>
        PreLoader.Add(ImagePaths.FindIndex(x => x.FullName.Equals(file.FullName)), ImagePaths, imageModel, IsReversed);

    public PreLoadValue? GetPreLoadValue(int index)
    {
        if (index < 0 || index >= ImagePaths.Count)
        {
            return null;
        }

        return _isRunning
            ? PreLoader.Get(ImagePaths[index], ImagePaths)
            : PreLoader.Get(index, ImagePaths);
    }

    public PreLoadValue? GetPreLoadValue(FileInfo file) =>
        PreLoader.Get(file, ImagePaths);


    public async ValueTask<PreLoadValue?> GetOrLoadPreLoadValueAsync(int index) =>
        await PreLoader.GetOrLoadAsync(index, ImagePaths);
    
    public async ValueTask<PreLoadValue?> GetOrLoadPreLoadValueAsync(FileInfo file) =>
        await PreLoader.GetOrLoadAsync(file, ImagePaths);

    public PreLoadValue? GetCurrentPreLoadValue() =>
        _isRunning
            ? PreLoader.Get(_vm.PicViewer.FileInfo.CurrentValue, ImagePaths)
            : PreLoader.Get(CurrentIndex, ImagePaths);

    public async Task<PreLoadValue?> GetCurrentPreLoadValueAsync() =>
        _isRunning
            ? await PreLoader.GetOrLoadAsync(_vm.PicViewer.FileInfo.CurrentValue, ImagePaths)
            : await PreLoader.GetOrLoadAsync(CurrentIndex, ImagePaths);

    public PreLoadValue? GetNextPreLoadValue()
    {
        var nextIndex = GetIteration(CurrentIndex, IsReversed ? NavigateTo.Previous : NavigateTo.Next);
        return _isRunning ? PreLoader.Get(ImagePaths[nextIndex], ImagePaths) : PreLoader.Get(nextIndex, ImagePaths);
    }

    public async Task<PreLoadValue?>? GetNextPreLoadValueAsync()
    {
        var nextIndex = GetIteration(CurrentIndex, NavigateTo.Next);
        return _isRunning
            ? await PreLoader.GetOrLoadAsync(ImagePaths[nextIndex], ImagePaths)
            : await PreLoader.GetOrLoadAsync(nextIndex, ImagePaths);
    }

    public void RemoveItemFromPreLoader(int index) => PreLoader.Remove(index, ImagePaths);
    public void RemoveItemFromPreLoader(string fileName) => PreLoader.Remove(fileName, ImagePaths);

    public void RemoveCurrentItemFromPreLoader() => PreLoader.Remove(CurrentIndex, ImagePaths);

    public void Resynchronize() => PreLoader.Resynchronize(ImagePaths);

    #endregion

    #region Navigation

    public async ValueTask ReloadFileListAsync()
    {
        
    }

    public async ValueTask QuickReload()
    {
        
    }

    public int GetIteration(int index, NavigateTo navigateTo, bool skip1 = false, bool skip10 = false,
        bool skip100 = false)
    {
       
        return 0;
    }

    public async ValueTask NextIteration(NavigateTo navigateTo, CancellationTokenSource? cts)
    {
    }

    public async ValueTask NextIteration(int iteration, CancellationTokenSource? cts)
    {
        
    }

    public async ValueTask IterateToIndex(int index, CancellationToken token)
    {
        var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        await IterateToIndex(index, cts);
    }

    /// <summary>
    ///     Iterates to the given index in the image list, shows the corresponding image and preloads the next/previous images.
    /// </summary>
    /// <param name="index">The index to iterate to.</param>
    /// <param name="cts">The cancellation token source.</param>
    public async ValueTask IterateToIndex(int index, CancellationTokenSource? cts)
    {
        
    }

    private static Timer? _timer;


    private async ValueTask TimerIteration(int index, CancellationTokenSource? cts)
    {
        
    }

    public void UpdateFileListAndIndex(List<FileInfo> fileList, int index)
    {
    }

    #endregion

    #region IDisposable

    public async ValueTask DisposeAsync()
    {
        await ClearAsync().ConfigureAwait(false);
        Dispose(true, true);
    }

    private void Dispose(bool disposing, bool cleared = false)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _watcher?.Dispose();
            if (!cleared)
            {
                PreLoader.Clear();
            }

            _timer?.Dispose();
            _timer = null;
            PreLoader.Dispose();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion
}