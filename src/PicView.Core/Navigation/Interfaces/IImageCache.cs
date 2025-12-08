using PicView.Core.Models;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;

namespace PicView.Core.Navigation.Interfaces;

public interface IImageCache
{
    ValueTask<ImageModel> GetOrLoadAsync(FileInfo file, CancellationToken ct = default);
    bool TryGet(FileInfo f, out PreLoadValue? value);
    bool TryGet(int index, out PreLoadValue? value);
    void Add(FileInfo f, PreLoadValue v);
    void Remove(FileInfo f);
    
    void Clear(TabViewModel tab);
}