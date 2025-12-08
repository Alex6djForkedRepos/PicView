using PicView.Core.Models;
using PicView.Core.Preloading;

namespace PicView.Core.Navigation.Interfaces;

public interface IImageCache : IAsyncDisposable
{
    /// <summary>
    /// Gets or loads the image model for the specified file.
    /// This should also implicitly register 'high priority' for this file for the given owner, if possible,
    /// or the caller should call UpdatePriorities.
    /// </summary>
    ValueTask<ImageModel> GetOrLoadAsync(FileInfo file, CancellationToken ct = default);
    
    /// <summary>
    /// Gets the existing preload value or schedules a background load if not present.
    /// Returns immediately.
    /// </summary>
    PreLoadValue GetOrScheduleLoad(FileInfo file, CancellationToken ct = default);

    /// <summary>
    /// Updates the priority list for a specific owner (e.g., a Tab).
    /// The cache uses this to determine which images to keep.
    /// Files at the beginning of the list are highest priority (distance 0).
    /// Files not in the list for an owner are considered lowest priority for that owner.
    /// </summary>
    /// <param name="owner">The object requesting the images (e.g. TabViewModel).</param>
    /// <param name="prioritizedFiles">List of file paths ordered by importance.</param>
    void UpdatePriorities(object owner, IEnumerable<string> prioritizedFiles);
    
    /// <summary>
    /// Removes an owner from the cache tracking. 
    /// Should be called when a Tab is closed.
    /// </summary>
    void RemoveOwner(object owner);
    
    bool TryGet(FileInfo f, out PreLoadValue? value);
    void Remove(FileInfo f);
    void Clear();
}