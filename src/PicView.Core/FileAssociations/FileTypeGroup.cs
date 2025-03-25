using System.Collections.ObjectModel;
using ReactiveUI;

namespace PicView.Core.FileAssociations;

public class FileTypeGroup : ReactiveObject
{
    public string Name { get; set; }
    public ObservableCollection<FileTypeItem> FileTypes { get; }

    public bool? IsSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public FileTypeGroup(string name, IEnumerable<FileTypeItem> fileTypes, bool? isSelected = true)
    {
        Name = name;
        FileTypes = new ObservableCollection<FileTypeItem>(fileTypes);
        IsSelected = isSelected;
    }
}