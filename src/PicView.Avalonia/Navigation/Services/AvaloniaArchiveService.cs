using PicView.Core.ArchiveHandling;
using PicView.Core.Navigation.Interfaces;

namespace PicView.Avalonia.Navigation.Services;

public class AvaloniaArchiveService : IArchiveService
{
    public async Task<DirectoryInfo> ExtractToTempAsync(FileInfo archive, CancellationToken ct)
    {
        // Wrapper for ArchiveExtraction static class
        // ExtractArchiveAsync requires a delegate for external tools. 
        // We provide a dummy one that returns false (fallback to internal extraction if supported, or fail).
        
        var result = await ArchiveExtraction.ExtractArchiveAsync(
            archive.FullName, 
            (path, dest) => Task.FromResult(false) 
        );

        if (result && ArchiveExtraction.TempZipDirectory != null)
        {
            return new DirectoryInfo(ArchiveExtraction.TempZipDirectory);
        }

        throw new IOException($"Failed to extract archive: {archive.FullName}");
    }
}