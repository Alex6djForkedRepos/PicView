using PicView.Avalonia.Functions;
using PicView.Core.FileSorting;
using R3;

namespace PicView.Avalonia.ViewModels;

public class FileSortingViewModel : IDisposable
{
    public ReactiveCommand SortFilesByNameCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesByName();
    });
    
    public ReactiveCommand SortFilesByCreationTimeCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesByCreationTime();
    });
    
    public ReactiveCommand SortFilesByLastAccessTimeCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesByLastAccessTime();
    });
    
    public ReactiveCommand SortFilesBySizeCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesBySize();
    });
    
    public ReactiveCommand SortFilesByExtensionCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesByExtension();
    });
    
    public ReactiveCommand SortFilesRandomlyCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesRandomly();
    });
    
    public ReactiveCommand SortFilesAscendingCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesAscending();
    });
    
    public ReactiveCommand SortFilesDescendingCommand { get; } = new(async (_, _) =>
    {
        await FunctionsMapper.SortFilesDescending();
    });

    public BindableReactiveProperty<SortFilesBy> SortOrder { get; } = new();
    
    public BindableReactiveProperty<bool> IsAscending { get; } = new(Settings.Sorting.Ascending);

    public void Dispose()
    {
        Disposable.Dispose();
    }
}