using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.CustomControls;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Views.Gallery;

public partial class GalleryItem : NavigateAbleItem
{
    public GalleryItem()
    {
        InitializeComponent();
        GalleryContextMenu.Opened += GalleryContextMenuOnOpened;
        GalleryContextMenu.Closed += GalleryContextMenuOnClosed;
    }

    private void GalleryContextMenuOnClosed(object? sender, RoutedEventArgs e)
    {
        SetContextMenuOpen(false);
    }

    private void GalleryContextMenuOnOpened(object? sender, RoutedEventArgs e)
    {
        SetContextMenuOpen(true);
    }

    private void Flyout_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control ctl)
        {
            return;
        }

        FlyoutBase.ShowAttachedFlyout(ctl);
    }
    
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var viewer = this.FindLogicalAncestorOfType<NavigateAbleItemsViewer>();
        if (viewer is null)
        {
            return;
        }

        var container = this.FindLogicalAncestorOfType<ContentPresenter>();
        if (container is null)
        {
            return;
        }

        var index = viewer.IndexFromContainer(container);
        if (index == -1)
        {
            return;
        }

        viewer.SelectedItemIndex = index;

        if (viewer.DataContext is TabViewModel tab)
        {
            tab.Gallery.OpenSelectedItemCommand.Execute(index);
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        GalleryContextMenu.Opened -= GalleryContextMenuOnOpened;
        GalleryContextMenu.Closed -= GalleryContextMenuOnClosed;
    }
}