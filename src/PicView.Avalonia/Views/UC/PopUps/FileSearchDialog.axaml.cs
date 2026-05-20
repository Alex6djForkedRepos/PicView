using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.UI;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileSearch;
using R3;

namespace PicView.Avalonia.Views.UC.PopUps;

public partial class FileSearchDialog : AnimatedPopUp
{
    private readonly CompositeDisposable _disposables = new();

    public FileSearchDialog()
    {
        // DataContext = UIHelper.GetMainView.DataContext as MainViewModel;
        // if (DataContext is MainViewModel { PicViewer.FilteredFileInfos.CurrentValue: null } vm)
        // {
        //     vm.PicViewer.FilteredFileInfos.Value = [];
        // }
        //
        // InitializeComponent();
        // Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        // // Ensure we don't double-subscribe if Loaded fires multiple times
        // _disposables.Clear();
        // SetupSearchSubscription();
        //
        // // SearchBox.Focus();
        //
        // AddHandler(KeyDownEvent, KeysDownAsync, RoutingStrategies.Tunnel);
    }

    private async ValueTask KeysDownAsync(object? sender, KeyEventArgs e)
    {
        // switch (e.Key)
        // {
        //     case Key.Down:
        //         MoveFocus(NavigationDirection.Next);
        //         e.Handled = true;
        //         break;
        //     case Key.Up:
        //         MoveFocus(NavigationDirection.Previous);
        //         e.Handled = true;
        //         break;
        //     case Key.Enter:
        //         // if (string.IsNullOrWhiteSpace(SearchBox.Text))
        //         // {
        //         //     return;
        //         // }
        //         //
        //         // if (uint.TryParse(SearchBox.Text, out var result))
        //         // {
        //         //     e.Handled = true;
        //         //     var desiredIndex = result <= 0 ? 0 : Math.Min(NavigationManager.GetCount - 1, result - 1);
        //         //     await ImageLoader.CheckCancellationAndStartIterateToIndex((int)desiredIndex,
        //         //             NavigationManager.ImageIterator, CancellationToken.None)
        //         //         .ConfigureAwait(false);
        //         // }
        //
        //         break;
        // }
    }

    private void MoveFocus(NavigationDirection direction)
    {
        // if (TopLevel.GetTopLevel(this) is not { FocusManager: { } focusManager })
        // {
        //     return;
        // }
        //
        // var focused = focusManager.GetFocusedElement();
        // if (focused is null)
        // {
        //     return;
        // }
        //
        // var next = focusManager.FindNextElement(direction);
        // next?.Focus();
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Loaded -= OnLoaded;
        Dispose();
    }

    private void SetupSearchSubscription()
    {
       
    }

    public void Dispose()
    {
        // _disposables.Dispose();
        // RemoveHandler(KeyDownEvent, KeysDownAsync);
        //
        // if (DataContext is MainViewModel vm)
        // {
        //     vm.PicViewer.FilteredFileInfos.Value.Clear();
        // }
    }
}