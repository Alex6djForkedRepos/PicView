namespace PicView.Core.Navigation.Interfaces;

public interface IArchiveService
{
    Task<DirectoryInfo> ExtractToTempAsync(FileInfo archive, CancellationToken ct);
}