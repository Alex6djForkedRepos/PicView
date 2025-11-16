using PicView.Core.Models;

namespace PicView.Core.Navigation;

public interface IImageLoader
{
    ValueTask<ImageModel> GetImageModelAsync(FileInfo file, CancellationToken ct);
    ValueTask<ImageModel> GetImageModelFromStreamAsync(Stream s, CancellationToken ct);
}