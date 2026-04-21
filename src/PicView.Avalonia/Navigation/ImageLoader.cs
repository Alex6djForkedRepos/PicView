using ImageMagick;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Input;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.ArchiveHandling;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.Gallery;
using PicView.Core.Http;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;
using PicView.Core.Models;
using PicView.Core.Navigation;

namespace PicView.Avalonia.Navigation;

// TODO deprecated, delete
public static class ImageLoader
{
    #region Load Pic From String

    /// <summary>
    ///     Loads a picture from a given string source, which can be a file path, directory path, or URL.
    /// </summary>
    public static async ValueTask LoadPicFromStringAsync(string source, MainViewModel vm, ImageIterator imageIterator)
    {
        
    }

    #endregion

    #region Load Pic From File

    /// <summary>
    /// Loads an image from a specified file and manages navigation within the directory or recreates the iterator.
    /// </summary>
    /// <param name="fileName">The full path of the file to load.</param>
    /// <param name="vm">The main view model instance associated with the application context.</param>
    /// <param name="imageIterator">An iterator for navigating through images in the directory.</param>
    /// <param name="fileInfo">Optional file information, defaults to a new <c>FileInfo</c> instance for the given file name if not provided.</param>
    public static async ValueTask LoadPicFromFile(string fileName, MainViewModel vm, ImageIterator imageIterator,
        FileInfo? fileInfo = null)
    {
        fileInfo ??= new FileInfo(fileName);
        if (!fileInfo.Exists)
        {
            return;
        }

        await CancelAsync().ConfigureAwait(false);

        if (imageIterator is not null)
        {
            // If image is in same directory as is being browsed, navigate to it. Otherwise, load without iterator.
            if (fileInfo.DirectoryName == imageIterator.InitialFileInfo.DirectoryName)
            {
                var index = imageIterator.ImagePaths.FindIndex(x => x.FullName.Equals(fileName));
                if (index != -1)
                {
                    _cancellationTokenSource ??= new CancellationTokenSource();
                    await imageIterator.IterateToIndex(index, _cancellationTokenSource).ConfigureAwait(false);
                    // await NavigationManager.CheckIfTiffAndUpdate(vm, fileInfo, index);
                }
                else
                {
                    await LoadWithoutIterator();
                }
            }
            else
            {
                await LoadWithoutIterator();
            }
        }
        else
        {
            await LoadWithoutIterator();
        }

        return;

        async Task LoadWithoutIterator()
        {
            if (Settings.UIProperties.IsTaskbarProgressEnabled)
            {
                vm.PlatformService.StopTaskbarProgress();
            }

            // await NavigationManager.LoadWithoutImageIterator(fileInfo, vm).ConfigureAwait(false);
        }
    }

    #endregion

    #region Load Pic From Directory

    /// <summary>
    /// Loads a picture from a directory.
    /// </summary>
    /// <param name="file">The path to the directory containing the picture.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <param name="fileInfo">Optional: FileInfo object for the directory.</param>
    public static async ValueTask LoadPicFromDirectoryAsync(string file, MainViewModel vm, FileInfo? fileInfo = null)
    {
        
    }

    #endregion

    #region Load Pic From Archive

    /// <summary>
    ///     Asynchronously loads pictures from the specified archive file.
    /// </summary>
    /// <param name="path">The path to the archive file containing the picture(s) to load.</param>
    /// <param name="vm">The main view model instance used to manage UI state and operations.</param>
    /// <param name="imageIterator">The image iterator to use for navigation.</param>
    public static async ValueTask LoadPicFromArchiveAsync(string path, MainViewModel vm, ImageIterator imageIterator)
    {
    }

    #endregion

    #region Load Pic From URL

    /// <summary>
    ///     Loads a picture from a given URL.
    /// </summary>
    /// <param name="url">The URL of the picture to load.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <param name="imageIterator">The image iterator to use for navigation.</param>
    public static async ValueTask LoadPicFromUrlAsync(string url, MainViewModel vm, ImageIterator imageIterator)
    {
    }

    #endregion

    #region Load Pic From Base64

    /// <summary>
    ///     Loads a picture from a Base64-encoded string.
    /// </summary>
    /// <param name="base64">The Base64-encoded string representing the picture.</param>
    /// <param name="vm">The main view model instance.</param>
    /// <param name="imageIterator">The image iterator to use for navigation.</param>
    public static async ValueTask LoadPicFromBase64Async(string base64, MainViewModel vm, ImageIterator imageIterator)
    {
    }

    #endregion

    #region Cancellation

    private static CancellationTokenSource? _cancellationTokenSource;

    public static async ValueTask CancelAsync()
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }

        _cancellationTokenSource = new CancellationTokenSource();
    }

    #endregion

    #region Image Iterator Loading

    /// <inheritdoc cref="ImageIterator.NextIteration(NavigateTo, CancellationTokenSource)" />
    public static async ValueTask LastIterationAsync(ImageIterator imageIterator) =>
        await imageIterator
            .NextIteration(NavigateTo.Last, _cancellationTokenSource)
            .ConfigureAwait(false);

    /// <inheritdoc cref="ImageIterator.NextIteration(NavigateTo, CancellationTokenSource)" />
    public static async ValueTask FirstIterationAsync(ImageIterator imageIterator) =>
        await imageIterator
            .NextIteration(NavigateTo.First, _cancellationTokenSource)
            .ConfigureAwait(false);

    public static async ValueTask CheckCancellationAndStartIterateToIndex(int index, ImageIterator imageIterator,
        CancellationToken? cancellationToken)
    {
        if (_cancellationTokenSource is not null)
        {
            await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        }

        _cancellationTokenSource = new CancellationTokenSource();
        if (cancellationToken is not null)
        {
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken.Value, _cancellationTokenSource.Token);
        }
        await imageIterator.NextIteration(index, _cancellationTokenSource).ConfigureAwait(false);
    }

    public static async ValueTask IterateToIndexAsync(int index, ImageIterator imageIterator) =>
        await imageIterator.NextIteration(index, _cancellationTokenSource).ConfigureAwait(false);

    #endregion
}