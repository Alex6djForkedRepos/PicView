using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.StartUp;
using PicView.Avalonia.WindowBehavior;
using PicView.Core.FileHistory;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.UC;

public partial class FileHistoryItem : UserControl
{
    public FileHistoryItem()
    {
        InitializeComponent();
        PointerEntered += OnPointerEntered;
        PointerExited += OnPointerExited;
        MainButton.AddHandler(PointerPressedEvent, MainButtonOnClick, RoutingStrategies.Tunnel);
    }

    private async ValueTask MainButtonOnClick(object? sender, RoutedEventArgs e)
    {
        if (Application.Current.DataContext is not CoreViewModel core || DataContext is not FileHistoryEntryViewModel entry)
        {
            return;
        }
        var window = core.MainWindows.ActiveWindow.CurrentValue;
        window.TopTitlebarViewModel.DropDownMenu.IsDropDownMenuVisible.Value = false;
        
        var tabs = window.WindowTabs;
        var tab = tabs.ActiveTab.CurrentValue;
        var path = entry.FilePath.CurrentValue;
        
        if (!tab.IsInitialized)
        {
            await QuickLoad.QuickLoadAsync(core, path, true);
            return;
        }

        var isViewStartUpMenu = false;
        if (tab.CurrentView.CurrentValue is StartUpMenu)
        {
            tab.CurrentView.Value = new ImageViewer();
            isViewStartUpMenu = true;
        }
        
        var isLoadedSuccessfully = await tabs.LoadFromStringAsync(path);
        if (!isLoadedSuccessfully && isViewStartUpMenu)
        {
            tab.CurrentView.Value = new StartUpMenu();
        }
        else
        {
            WindowResizing.SetSize(window, WindowResizeReason.Layout);
        }
    }

    private void OnPointerEntered(object? sender, PointerEventArgs e)
    {
        ButtonPanel.IsVisible = true;
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        ButtonPanel.IsVisible = false;
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        PointerEntered -= OnPointerEntered;
        PointerExited -= OnPointerExited;
    }
}