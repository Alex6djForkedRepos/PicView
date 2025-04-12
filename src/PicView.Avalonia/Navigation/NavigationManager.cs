using Avalonia.Threading;
using PicView.Avalonia.Crop;
using PicView.Avalonia.Gallery;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Preloading;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileHistory;
using PicView.Core.Gallery;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;
using PicView.Core.Navigation;

namespace PicView.Avalonia.Navigation;

/// <summary>
///     Manages image navigation within the application.
/// </summary>
public static class NavigationManager
{
    public static TiffManager.TiffNavigationInfo? TiffNavigationInfo { get; private set; }

    // Should be updated to handle multiple iterators, in the future when adding tab support
    private static ImageIterator? _imageIterator;

    #region Navigation

    /// <summary>
    ///     Determines whether navigation is possible based on the current state of the <see cref="MainViewModel" />.
    /// </summary>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>True if navigation is possible, otherwise false.</returns>
    public static bool CanNavigate(MainViewModel vm)
    {
        return _imageIterator?.ImagePaths is not null &&
               _imageIterator.ImagePaths.Count > 0 && !CropFunctions.IsCropping &&
               !DialogManager.IsDialogOpen && vm is { IsEditableTitlebarOpen: false, PicViewer.FileInfo: not null };
        // TODO: should probably turn this into CanExecute observable for ReactiveUI
    }

    /// <summary>
    ///     Navigates to the next or previous image based on the <paramref name="next" /> parameter.
    /// </summary>
    /// <param name="next">True to navigate to the next image, false for the previous image.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Navigate(bool next, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            if (vm.PicViewer.FileInfo is null && _imageIterator is not null)
            {
                // Fixes issue that shouldn't happen. Should investigate.
                vm.PicViewer.FileInfo = new FileInfo(_imageIterator.ImagePaths[0]);
            }
            else
            {
                return;
            }
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            await GalleryNavigation.ScrollGallery(next);
            return;
        }
        
        if (_imageIterator.CurrentIndex < 0 || _imageIterator.CurrentIndex >= _imageIterator.ImagePaths.Count)
        {
            ErrorHandling.ShowStartUpMenu(vm);
            return;
        }

        var navigateTo = next ? NavigateTo.Next : NavigateTo.Previous;
        var nextIteration = _imageIterator.GetIteration(_imageIterator.CurrentIndex, navigateTo);
        var currentFileName = _imageIterator.ImagePaths[_imageIterator.CurrentIndex];
        if (TiffManager.IsTiff(currentFileName))
        {
            await TiffNavigation(vm, currentFileName, nextIteration).ConfigureAwait(false);
        }
        else
        {
            await ImageLoader.CheckCancellationAndStartIterateToIndex(nextIteration, _imageIterator).ConfigureAwait(false);
        }
    }

    private static async Task TiffNavigation(MainViewModel vm, string currentFileName, int nextIteration)
    {
        if (TiffNavigationInfo is null && !_imageIterator.IsReversed)
        {
            var tiffPages = await Task.FromResult(TiffManager.LoadTiffPages(currentFileName)).ConfigureAwait(false);
            if (tiffPages.Count < 1)
            {
                await ImageLoader.CheckCancellationAndStartIterateToIndex(nextIteration, _imageIterator).ConfigureAwait(false);
                return;
            }

            TiffNavigationInfo = new TiffManager.TiffNavigationInfo
            {
                CurrentPage = 0,
                PageCount = tiffPages.Count,
                Pages = tiffPages
            };
        }

        if (TiffNavigationInfo is null)
        {
            await ImageLoader.CheckCancellationAndStartIterateToIndex(nextIteration, _imageIterator).ConfigureAwait(false);
        }
        else
        {
            if (_imageIterator.IsReversed)
            {
                if (TiffNavigationInfo.CurrentPage - 1 < 0)
                {
                    await ExitTiffNavigationAndNavigate().ConfigureAwait(false);
                    return;
                }

                TiffNavigationInfo.CurrentPage -= 1;
            }
            else
            {
                TiffNavigationInfo.CurrentPage += 1;
            }

            if (TiffNavigationInfo.CurrentPage >= TiffNavigationInfo.PageCount || TiffNavigationInfo.CurrentPage < 0)
            {
                await ExitTiffNavigationAndNavigate().ConfigureAwait(false);
            }
            else
            {
                await UpdateImage.SetTiffImageAsync(TiffNavigationInfo, _imageIterator.CurrentIndex, vm.PicViewer.FileInfo, vm);
            }
        }
        return;
        
        async Task ExitTiffNavigationAndNavigate()
        {
            await ImageLoader.CheckCancellationAndStartIterateToIndex(nextIteration, _imageIterator).ConfigureAwait(false);
            TiffNavigationInfo?.Dispose();
            TiffNavigationInfo = null;
        }
    }
    
    public static async Task<bool> CheckIfTiffAndUpdate(MainViewModel vm, FileInfo fileInfo, int index)
    {
        if (!TiffManager.IsTiff(fileInfo))
        {
            return false;
        }
        
        var tiffPages = await Task.FromResult(TiffManager.LoadTiffPages(fileInfo.FullName)).ConfigureAwait(false);
        if (tiffPages.Count < 1)
        {
            return false;
        }

        TiffNavigationInfo = new TiffManager.TiffNavigationInfo
        {
            CurrentPage = 0,
            PageCount = tiffPages.Count,
            Pages = tiffPages
        };
        await UpdateImage.SetTiffImageAsync(TiffNavigationInfo, index, fileInfo, vm);
        return true;
    }

    public static async Task Navigate(int index, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        await ImageLoader.CheckCancellationAndStartIterateToIndex(index, _imageIterator).ConfigureAwait(false);
    }
    
    public static async Task Navigate(string fileName, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            return;
        }
        
        var index = _imageIterator.ImagePaths.IndexOf(fileName);

        await ImageLoader.CheckCancellationAndStartIterateToIndex(index, _imageIterator).ConfigureAwait(false);
    }

    private static async Task NavigateIncrements(MainViewModel vm, bool next, bool is10, bool is100)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        var currentIndex = _imageIterator.CurrentIndex;
        var direction = next ? NavigateTo.Next : NavigateTo.Previous;
        var index = _imageIterator.GetIteration(currentIndex, direction, false, is10, is100);

        await ImageLoader.CheckCancellationAndStartIterateToIndex(index, _imageIterator).ConfigureAwait(false);
    }

    public static Task Next10(MainViewModel vm) => NavigateIncrements(vm, true, true, false);
    public static Task Next100(MainViewModel vm) => NavigateIncrements(vm, true, false, true);
    public static Task Prev10(MainViewModel vm) => NavigateIncrements(vm, false, true, false);
    public static Task Prev100(MainViewModel vm) => NavigateIncrements(vm, false, false, true);

    /// <summary>
    ///     Navigates to the first or last image in the collection.
    /// </summary>
    /// <param name="last">True to navigate to the last image, false to navigate to the first image.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task NavigateFirstOrLast(bool last, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            GalleryNavigation.NavigateGallery(last, vm);
        }
        else
        {
            if (last)
            {
                await ImageLoader.LastIterationAsync(_imageIterator).ConfigureAwait(false);
            }
            else
            {
                await ImageLoader.FirstIterationAsync(_imageIterator).ConfigureAwait(false);
            }
            await UIHelper.ScrollToEndIfNecessary(last);
        }
    }

    /// <summary>
    ///     Iterates to the next or previous image based on the <paramref name="next" /> parameter.
    /// </summary>
    /// <param name="next">True to iterate to the next image, false for the previous image.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Iterate(bool next, MainViewModel vm)
    {
        if (GalleryFunctions.IsFullGalleryOpen)
        {
            GalleryNavigation.NavigateGallery(next ? Direction.Right : Direction.Left, vm);
        }
        else
        {
            await Navigate(next, vm);
        }
    }

    /// <summary>
    ///     Navigates and moves the cursor to the corresponding button.
    /// </summary>
    /// <param name="next">True to navigate to the next image, false for the previous image.</param>
    /// <param name="arrow">True to move cursor to the arrow, false for the button.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task NavigateAndPositionCursor(bool next, bool arrow, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        if (GalleryFunctions.IsFullGalleryOpen)
        {
            await GalleryNavigation.ScrollGallery(next);
        }
        else
        {
            await Navigate(next, vm);
            await UIHelper.MoveCursorOnButtonClick(next, arrow, vm);
        }
    }

    /// <summary>
    ///     Navigates to the next or previous folder and loads the first image in that folder.
    /// </summary>
    /// <param name="next">True to navigate to the next folder, false for the previous folder.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task GoToNextFolder(bool next, MainViewModel vm)
    {
        if (!CanNavigate(vm))
        {
            return;
        }

        TitleManager.SetLoadingTitle(vm);
        await ImageLoader.CancelAsync().ConfigureAwait(false);

        var fileList = await GetNextFolderFileList(next, vm).ConfigureAwait(false);

        if (fileList is null)
        {
            TitleManager.SetTitle(vm);
        }
        else
        {
            vm.PlatformService.StopTaskbarProgress();
            await LoadWithoutImageIterator(new FileInfo(fileList[0]), vm, fileList);
            if (vm.PicViewer.Title == TranslationManager.Translation.Loading)
            {
                TitleManager.SetTitle(vm);
            }
        }
    }

    #endregion

    #region Load pictures from string, file or url

    /// <inheritdoc cref="ImageLoader.LoadPicFromStringAsync(string, MainViewModel, ImageIterator)" />
    public static async Task LoadPicFromStringAsync(string source, MainViewModel vm) =>
        await ImageLoader.LoadPicFromStringAsync(source, vm, _imageIterator).ConfigureAwait(false);

    /// <inheritdoc cref="ImageLoader.LoadPicFromFile(string, MainViewModel, ImageIterator, FileInfo)" />
    public static async Task LoadPicFromFile(string fileName, MainViewModel vm, FileInfo? fileInfo = null) =>
        await ImageLoader.LoadPicFromFile(fileName, vm, _imageIterator, fileInfo).ConfigureAwait(false);

    /// <inheritdoc cref="ImageLoader.LoadPicFromArchiveAsync(string, MainViewModel, ImageIterator)" />
    public static async Task LoadPicFromArchiveAsync(string path, MainViewModel vm) =>
        await ImageLoader.LoadPicFromArchiveAsync(path, vm, _imageIterator).ConfigureAwait(false);

    /// <inheritdoc cref="ImageLoader.LoadPicFromUrlAsync(string, MainViewModel, ImageIterator)" />
    public static async Task LoadPicFromUrlAsync(string url, MainViewModel vm) =>
        await ImageLoader.LoadPicFromUrlAsync(url, vm, _imageIterator).ConfigureAwait(false);

    /// <inheritdoc cref="ImageLoader.LoadPicFromBase64Async(string, MainViewModel, ImageIterator)" />
    public static async Task LoadPicFromBase64Async(string base64, MainViewModel vm) =>
        await ImageLoader.LoadPicFromBase64Async(base64, vm, _imageIterator).ConfigureAwait(false);

    /// <inheritdoc cref="ImageLoader.LoadPicFromDirectoryAsync(string, MainViewModel, FileInfo)"/>
    public static async Task LoadPicFromDirectoryAsync(string file, MainViewModel vm, FileInfo? fileInfo = null) =>
        await ImageLoader.LoadPicFromDirectoryAsync(file, vm, fileInfo).ConfigureAwait(false);

    #endregion

    #region ImageIterator

    public static void InitializeImageIterator(MainViewModel vm)
    {
        _imageIterator ??= new ImageIterator(vm.PicViewer.FileInfo, vm);
    }
    
    public static async Task DisposeImageIteratorAsync()
    {
        if (_imageIterator is null)
        {
            return;
        }
        await _imageIterator.ClearAsync();
        _imageIterator.ImagePaths.Clear();
        await _imageIterator.DisposeAsync();
    }
    
    public static bool IsCollectionEmpty => _imageIterator?.ImagePaths is null || _imageIterator?.ImagePaths?.Count < 0;
    public static List<string>? GetCollection => _imageIterator?.ImagePaths;
    
    public static void UpdateFileListAndIndex(List<string> fileList, int index) => _imageIterator?.UpdateFileListAndIndex(fileList, index);
    
    public static int? GetFileNameIndex(string fileName) =>
        IsCollectionEmpty ? null : _imageIterator.ImagePaths.IndexOf(fileName);

    /// <summary>
    ///     Returns the file name at a given index in the image collection.
    /// </summary>
    /// <param name="index">The index of the file to retrieve.</param>
    /// <returns>The file name at the given index.</returns>
    public static string? GetFileNameAt(int index)
    {
        if (IsCollectionEmpty)
        {
            return null;
        }

        if (index < 0 || index >= _imageIterator.ImagePaths.Count)
        {
            return null;
        }

        return _imageIterator.ImagePaths[index];
    }
    
    /// <summary>
    ///     Gets the current file name.
    /// </summary>
    public static string? GetCurrentFileName => GetFileNameAt(_imageIterator?.CurrentIndex ?? -1);
    
    /// <summary>
    ///     Gets the next file name.
    /// </summary>
    public static string? GetNextFileName => GetFileNameAt(_imageIterator?.NextIndex ?? -1);

    public static int GetCurrentIndex => _imageIterator?.CurrentIndex ?? -1;
    
    public static int GetNextIndex => _imageIterator?.NextIndex ?? -1;
    
    public static int GetNonZeroIndex => _imageIterator?.GetNonZeroIndex ?? -1;
    
    public static int GetCount => _imageIterator?.GetCount ?? -1;
    
    public static FileInfo? GetInitialFileInfo => _imageIterator?.InitialFileInfo;
    
    public static PreLoadValue? GetPreLoadValue(int index) => _imageIterator?.GetPreLoadValue(index) ?? null;
    public static async Task<PreLoadValue?> GetPreLoadValueAsync(int index) => await _imageIterator?.GetPreLoadValueAsync(index) ?? null;
    public static async Task<PreLoadValue?> GetPreLoadValueAsync(string fileName) => await _imageIterator?.GetPreLoadValueAsync(GetFileNameIndex(fileName) ?? GetCurrentIndex) ?? null;
    public static PreLoadValue? GetCurrentPreLoadValue() => _imageIterator?.GetCurrentPreLoadValue() ?? null;
    public static async Task<PreLoadValue?> GetCurrentPreLoadValueAsync() => await _imageIterator?.GetCurrentPreLoadValueAsync() ?? null;
    public static PreLoadValue? GetNextPreLoadValue() => _imageIterator?.GetNextPreLoadValue() ?? null;
    public static async Task<PreLoadValue?> GetNextPreLoadValueAsync() => await _imageIterator?.GetNextPreLoadValueAsync() ?? null;
    
    public static async Task ReloadFileListAsync() => await _imageIterator?.ReloadFileListAsync();
    
    public static void AddToPreloader(int index, ImageModel imageModel) => _imageIterator?.Add(index, imageModel);
    public static async Task PreloadAsync() => await _imageIterator?.PreloadAsync();
    
    public static async Task QuickReload() => await _imageIterator?.QuickReload();

    #endregion

    /// <summary>
    ///     Gets the list of files in the next or previous folder.
    /// </summary>
    /// <param name="next">True to get the next folder, false for the previous folder.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <returns>A task representing the asynchronous operation that returns a list of file paths.</returns>
    private static async Task<List<string>?> GetNextFolderFileList(bool next, MainViewModel vm)
    {
        return await Task.Run(() =>
        {
            var indexChange = next ? 1 : -1;
            var currentFolder = Path.GetDirectoryName(_imageIterator?.ImagePaths[_imageIterator.CurrentIndex]);
            var parentFolder = Path.GetDirectoryName(currentFolder);
            var directories = Directory.GetDirectories(parentFolder, "*", SearchOption.TopDirectoryOnly);
            var directoryIndex = Array.IndexOf(directories, currentFolder);
            if (Settings.UIProperties.Looping)
            {
                directoryIndex = (directoryIndex + indexChange + directories.Length) % directories.Length;
            }
            else
            {
                directoryIndex += indexChange;
                if (directoryIndex < 0 || directoryIndex >= directories.Length)
                {
                    return null;
                }
            }

            for (var i = directoryIndex; i < directories.Length; i++)
            {
                var fileInfo = new FileInfo(directories[i]);
                var fileList = vm.PlatformService.GetFiles(fileInfo);
                if (fileList is { Count: > 0 })
                {
                    return fileList;
                }
            }

            return null;
        }).ConfigureAwait(false);
    }


    /// <summary>
    ///     Loads a picture from a given file, reloads the ImageIterator and loads the corresponding gallery from the file's
    ///     directory.
    /// </summary>
    /// <param name="fileInfo">The FileInfo object representing the file to load.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <param name="files">
    ///     Optional: The list of file paths to load. If null, the list is loaded from the given file's
    ///     directory.
    /// </param>
    /// <param name="index">Optional: The index at which to start the navigation. Defaults to 0.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task LoadWithoutImageIterator(FileInfo fileInfo, MainViewModel vm, List<string>? files = null,
        int index = 0)
    {
        var imageModel = await GetImageModel.GetImageModelAsync(fileInfo).ConfigureAwait(false);
        ImageModel? nextImageModel = null;
        vm.PicViewer.ImageSource = imageModel.Image;
        vm.PicViewer.ImageType = imageModel.ImageType;
        if (!Settings.ImageScaling.ShowImageSideBySide)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                WindowResizing.SetSize(imageModel.PixelWidth, imageModel.PixelHeight, 0, 0, imageModel.Rotation,
                    vm);
            });
        }

        await DisposeImageIteratorAsync();
        
        if (files is null)
        {
            _imageIterator = new ImageIterator(fileInfo, vm);
            index = _imageIterator.CurrentIndex;
        }
        else
        {
            _imageIterator = new ImageIterator(fileInfo, files, index, vm);
        }

        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            nextImageModel = (await _imageIterator.GetNextPreLoadValueAsync()).ImageModel;
            vm.PicViewer.SecondaryImageSource = nextImageModel.Image;
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                WindowResizing.SetSize(imageModel.PixelWidth, imageModel.PixelHeight, nextImageModel.PixelWidth,
                    nextImageModel.PixelHeight, imageModel.Rotation, vm);
            });
            
            TitleManager.SetSideBySideTitle(vm, imageModel, nextImageModel);
            UpdateImage.SetStats(vm, index, imageModel);
            
            // Fixes incorrect rendering in the side by side view
            // TODO: Improve and fix side by side and remove this hack 
            Dispatcher.UIThread.Post(() => { vm.ImageViewer?.MainImage?.InvalidateVisual(); });
        }
        else
        {
            var isTiffUpdated = await CheckIfTiffAndUpdate(vm, fileInfo, index); 
            if (!isTiffUpdated)
            {
                if (Settings.ImageScaling.ShowImageSideBySide)
                {
                    TitleManager.SetSideBySideTitle(vm, imageModel, nextImageModel);
                }
                else
                {
                    TitleManager.SetTitle(vm, imageModel);
                }
        
                UpdateImage.SetStats(vm, index, imageModel);
            }
        }

        vm.IsLoading = false;
        FileHistoryManager.Add(_imageIterator.ImagePaths[index]);
        if (Settings.ImageScaling.ShowImageSideBySide)
        {
            FileHistoryManager.Add(_imageIterator.ImagePaths[_imageIterator.GetIteration(index, NavigateTo.Next)]);
        }
        await GalleryLoad.CheckAndReloadGallery(fileInfo, vm);
    }
}