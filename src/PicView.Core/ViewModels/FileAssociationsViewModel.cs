using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using DynamicData;
using PicView.Core.FileAssociations;
using ReactiveUI;

namespace PicView.Core.ViewModels;

public class FileAssociationsViewModel : ReactiveObject
{
    private readonly ReadOnlyObservableCollection<FileTypeGroup> _fileTypeGroups;
    private readonly SourceList<FileTypeGroup> _fileTypeGroupsList = new();

    public ReadOnlyObservableCollection<FileTypeGroup> FileTypeGroups => _fileTypeGroups;

    public string? FilterText
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = string.Empty;

    private bool IsProcessing
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<Unit, bool> ApplyCommand { get; }
    public ReactiveCommand<Unit, string> ClearFilterCommand { get; }
    
    public FileAssociationsViewModel()
    {
        // Create file type groups and populate with data
        InitializeFileTypes();
        
        // Setup the filtering
        var filter = this.WhenAnyValue(x => x.FilterText)
            .Throttle(TimeSpan.FromMilliseconds(200))
            .Select(BuildFilter);
            
        _fileTypeGroupsList.Connect()
            .AutoRefresh()
            .Filter(filter)
            .Bind(out _fileTypeGroups)
            .Subscribe();

        // Canexecute for ApplyCommand
        var canExecute = this.WhenAnyValue(x => x.IsProcessing)
            .Select(processing => !processing);
            
        // Initialize commands with error handling
        ApplyCommand = ReactiveCommand.CreateFromTask(
            ApplyFileAssociations, 
            canExecute);
            
        // Handle errors from the Apply command
        ApplyCommand.ThrownExceptions
            .Subscribe(ex => 
            {
                IsProcessing = false;
#if DEBUG
                Debug.WriteLine($"Error in ApplyCommand: {ex}");
#endif
            });
            
        ClearFilterCommand = ReactiveCommand.Create(() => FilterText = string.Empty);
    }
    
    private Func<FileTypeGroup, bool> BuildFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            // Reset all items to visible when filter is empty
            foreach (var group in _fileTypeGroupsList.Items)
            {
                foreach (var item in group.FileTypes)
                {
                    item.IsVisible = true;
                }
            }
            return _ => true;
        }
        
        return group => {
            // Update visibility of items based on filter
            var anyVisible = false;
            foreach (var item in group.FileTypes)
            {
                item.IsVisible = item.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) || 
                                 item.Extension.Contains(filter, StringComparison.OrdinalIgnoreCase);
                if (item.IsVisible)
                    anyVisible = true;
            }
        
            // Only show groups that have at least one visible item
            return anyVisible;
        };
    }
    
    private void SyncUIStateToViewModel()
    {
        // Force property notifications to ensure all changes are processed
        foreach (var group in FileTypeGroups)
        {
            group.IsSelected = group.IsSelected;
            foreach (var fileType in group.FileTypes)
            {
                fileType.IsSelected = fileType.IsSelected;
            }
        }
    }

    private async Task<bool> ApplyFileAssociations()
    {
        try
        {
            IsProcessing = true;
            
            // Ensure all UI changes are synced to the ViewModel
            SyncUIStateToViewModel();
            
            // Now process the associations
            return await FileTypeHelper.SetFileAssociations(FileTypeGroups);
        }
        finally
        {
            IsProcessing = false;
        }
    }
    
    public void InitializeFileTypes()
    {
        var groups = FileTypeHelper.GetFileTypes();
        
        _fileTypeGroupsList.Edit(list =>
        {
            list.Clear();
            list.AddRange(groups);
        });
    }
}