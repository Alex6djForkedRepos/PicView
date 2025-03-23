using System.Diagnostics;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;

namespace PicView.Avalonia.Preloading;

/// <summary>
///     Handles progressive loading of images at multiple resolution stages.
/// </summary>
public static class ProgressiveImageLoader
{
    // Resolutions to generate, from lowest to highest (percentages of original)
    private static readonly int[] ResolutionStages = [10, 25, 50, 100];

    /// <summary>
    ///     Loads an image progressively, updating the UI at each resolution stage
    /// </summary>
    public static async Task<ImageModel?> LoadProgressivelyAsync(FileInfo fileInfo, MainViewModel viewModel,
        CancellationToken cancellationToken)
    {
        // Check for early cancellation
        if (cancellationToken.IsCancellationRequested)
        {
            return null;
        }

        // Get image dimensions first without loading the entire image
        using var magickImage = new MagickImage();
        try
        {
            magickImage.Ping(fileInfo);
        }
        catch (Exception e)
        {
#if DEBUG
            Trace.WriteLine($"\n{nameof(LoadProgressivelyAsync)} ping exception: \n{e.Message}\n{e.StackTrace}");
#endif
            // Handle broken pics
            return new ImageModel
            {
                EXIFOrientation = EXIFHelper.EXIFOrientation.None,
                PixelWidth = 0,
                PixelHeight = 0,
                FileInfo = fileInfo,
                Image = null,
                ImageType = ImageType.Invalid
            };
        }

        var isLargeImage = magickImage.Width * magickImage.Height > 3500000; // ~3.5 megapixels threshold

        // If it's a small image, just load it directly
        if (!isLargeImage)
        {
            return await GetImageModel.GetImageModelAsync(fileInfo);
        }

        // Final model to return
        var finalModel = new ImageModel
        {
            FileInfo = fileInfo,
            PixelWidth = (int)magickImage.Width,
            PixelHeight = (int)magickImage.Height
        };

        await UpdatePreview(magickImage.Width, magickImage.Height, finalModel, fileInfo, viewModel, cancellationToken)
            .ConfigureAwait(false);

        if (cancellationToken.IsCancellationRequested || finalModel.Image == null)
        {
            return finalModel;
        }

        // Load full metadata if we've succeeded
        finalModel.EXIFOrientation = EXIFHelper.GetImageOrientation(fileInfo);
        if (fileInfo.Extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
        {
            finalModel.ImageType = ImageAnalyzer.IsAnimated(fileInfo) ? ImageType.AnimatedGif : ImageType.Bitmap;
        }
        else if (fileInfo.Extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
        {
            finalModel.ImageType = ImageAnalyzer.IsAnimated(fileInfo) ? ImageType.AnimatedWebp : ImageType.Bitmap;
        }
        else
        {
            finalModel.ImageType = ImageType.Bitmap;
        }

        return finalModel;
    }

    public static async Task UpdatePreview(uint width, uint height, ImageModel finalModel, FileInfo fileInfo,
        MainViewModel viewModel, CancellationToken cancellationToken)
    {
        var isSizeSet = false;

        // For large files, start with EXIF thumbnail if available
        var exifThumb = GetThumbnails.GetExifThumb(fileInfo.FullName);
        if (exifThumb != null)
        {
            await UpdateUiWithImage(exifThumb, viewModel, finalModel, 0, isSizeSet);
            isSizeSet = true;
        }

        // Load progressively increasing resolutions
        for (var i = 0; i < ResolutionStages.Length && !cancellationToken.IsCancellationRequested; i++)
        {
            var percentage = ResolutionStages[i];

            // Skip unnecessary resolution stages for performance
            if (i > 0 && percentage < 100)
            {
                continue;
            }

            try
            {
                using var magick = await ImageDecoder.LoadImageAtResolutionAsync(
                    fileInfo,
                    percentage,
                    width,
                    height,
                    cancellationToken).ConfigureAwait(false);
                var bitmap = magick.ToWriteableBitmap();

                if (bitmap != null && !cancellationToken.IsCancellationRequested)
                {
                    await UpdateUiWithImage(bitmap, viewModel, finalModel, percentage, isSizeSet);
                }

                isSizeSet = true;

                // For the final resolution, update the model's image
                if (percentage == 100 && bitmap != null)
                {
                    finalModel.Image = bitmap;
                }
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation silently
                break;
            }
            catch (Exception ex)
            {
                // If we fail at a resolution stage but have a previous one, continue
                if (i > 0)
                {
                    continue;
                }

                // If we fail at the initial stage, propagate the error
                throw new Exception($"Failed to load image at {percentage}% resolution", ex);
            }
        }
    }

    private static async Task UpdateUiWithImage(
        Bitmap image,
        MainViewModel viewModel,
        ImageModel model,
        int percentage,
        bool isSizeSet)
    {
        viewModel.PicViewer.ImageSource = image;

        if (percentage >= 100)
        {
            return;
        }

        if (NavigationManager.CanNavigate(viewModel))
        {
            // Update title, pretend we're not loading to make it feel faster
            TitleManager.SetTitle(viewModel, model);
        }
        else
        {
            // Update loading status message to show progress
            viewModel.PicViewer.Title =
                viewModel.PicViewer.WindowTitle =
                    viewModel.PicViewer.TitleTooltip =
                        $"{TranslationManager.Translation.Loading} {Path.GetFileName(model.FileInfo.Name)} ({percentage}%)";
        }

        // Don't show loading indicator, since we're already showing the image
        viewModel.IsLoading = false;

        if (!isSizeSet)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                WindowResizing.SetSize(model.PixelWidth, model.PixelHeight, 0, 0, model.Rotation, viewModel);
                if (Settings.WindowProperties.AutoFit)
                {
                    WindowFunctions.CenterWindowOnScreen();
                }
            }, DispatcherPriority.Send);
        }
    }
}