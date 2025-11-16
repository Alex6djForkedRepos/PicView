using System.Collections.ObjectModel;
using PicView.Core.FileSearch;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Core.Navigation;

// This should be pointed at from the MainViewModel
public class NavigationViewModel : IDisposable
{
    // Todo: use this for tab layout in future 
    public BindableReactiveProperty<ObservableCollection<PicViewerModel>> PicViewModels { get; } = new();
    
    // Todo: Use this for shared cached single frame images to be preloaded
    // The Idea, is to have a shared state collection, to reduce memory, and to avoid having to load the exact same file again. 
    public BindableReactiveProperty<ObservableCollection<object?>> CachedPics { get; } = new();
    
    // Todo: Use this for binding to thumbnails
    // The Idea, is to avoid having to load the same thumbnails
    public BindableReactiveProperty<ObservableCollection<object?>> Thumbnails { get; } = new();
    
    // Todo: Use this for shared list of supported files and hook it up with a FileWatcher
    // The idea is to have a shared FileWatcher, to avoid consecutive updates and reduce memory usage
    public BindableReactiveProperty<ObservableCollection<List<FileInfo>>> FileInfos { get; } = new();
    
    // TODO: Use this to replace the ones from the PicViewer model
    // The idea is that it should be able to intelligently switch to a tab, if that tab has the file search result opened
    public BindableReactiveProperty<ObservableCollection<FileSearchResult>> FilteredFileInfos { get; } = new();

    public void Dispose()
    {
        // TODO release managed resources here
    }
}