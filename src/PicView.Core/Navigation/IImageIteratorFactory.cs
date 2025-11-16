namespace PicView.Core.Navigation;

public interface IImageIteratorFactory
{
    IImageIterator Create(FileInfo initialFile, IReadOnlyList<FileInfo>? files = null, bool setInitial = true);
}