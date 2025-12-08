namespace PicView.Core.Navigation.Interfaces;

public interface IFileWatcher : IDisposable
{
    void StartWatching(string directory, bool includeSubdirs);
    event Func<FileSystemEventArgs, Task>? Created;
    event Func<RenamedEventArgs, Task>? Renamed;
    event Func<FileSystemEventArgs, Task>? Deleted;
}