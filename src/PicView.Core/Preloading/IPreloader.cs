using PicView.Core.Models;

namespace PicView.Core.Preloading;

public interface IPreloader
{
    void Add(int index, FileInfo file, ImageModel model);
    ValueTask<bool> AddAsync(int index, IReadOnlyList<FileInfo> list, bool isReverse = false, CancellationToken ct = default);
    
    PreLoadValue? Get(FileInfo file, List<FileInfo> list);
    ValueTask<PreLoadValue?> GetOrLoadAsync(int index, IReadOnlyList<FileInfo> files, CancellationToken ct);
    ValueTask<PreLoadValue?> GetOrLoadAsync(int key, IReadOnlyList<FileInfo> list);

    bool Contains(int key, List<FileInfo> list);
    
    void Resynchronize(IReadOnlyList<FileInfo> files);
    
    ValueTask PreloadAsync(int currentIndex, bool reversed, IReadOnlyList<FileInfo> files, CancellationToken ct);
}