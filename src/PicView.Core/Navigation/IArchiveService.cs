namespace PicView.Core.Navigation;

public interface IArchiveService
{
    Task<DirectoryInfo> ExtractToTempAsync(FileInfo archive, CancellationToken ct);
}