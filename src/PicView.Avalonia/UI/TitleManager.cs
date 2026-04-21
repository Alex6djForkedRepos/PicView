using PicView.Avalonia.ViewModels;
using PicView.Core.Titles;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.UI;
public static class TitleManager
{
    /// <summary>
    ///     Sets the title of the window and the title displayed in the UI to the appropriate
    ///     value based on the current state of the application.
    /// </summary>
    /// <param name="vm">The main view model instance.</param>
    /// <remarks>Can be used to refresh the title when files are added or removed.</remarks>
    public static void SetTitle(MainViewModel vm)
    {
    }

    public static void SetTabTitle(TabViewModel tab, double zoomValue)
    {
        var titles = ImageTitleFormatter.GenerateTitleStrings(tab.Model.PixelWidth, tab.Model.PixelHeight,
            tab.ImageIterator.CurrentIndex,
            tab.Model.FileInfo, zoomValue, tab.ImageIterator.Files);
        tab.WindowTitle.Value = titles.TitleWithAppName;
        tab.Title.Value = titles.BaseTitle;
        tab.TitleTooltip.Value = titles.FilePathTitle;
    }
}