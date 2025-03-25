using ReactiveUI;

namespace PicView.Core.FileAssociations;

public class FileTypeItem : ReactiveObject
{
    public string Description { get; }
    public string[] Extensions { get; }
    
    public string Extension => string.Join(", ", Extensions);

    public bool? IsSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsVisible
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public FileTypeItem(string description, string[] extensions, bool? isSelected = true)
    {
        Description = description;
        Extensions = extensions;
        IsSelected = isSelected;
    }
}