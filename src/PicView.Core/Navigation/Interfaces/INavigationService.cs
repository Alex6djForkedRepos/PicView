using PicView.Core.ViewModels;

namespace PicView.Core.Navigation.Interfaces;

public interface INavigationService
{
    ValueTask LoadFromFileAsync(string source, TabViewModel tab, CancellationTokenSource ct);
    ValueTask LoadFromFileAsync(FileInfo fileInfo, TabViewModel tab, CancellationTokenSource ct);
    ValueTask LoadFromStringAsync(string source, TabViewModel tab, CancellationTokenSource ct);
    ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationTokenSource ct);
    ValueTask NavigateToIndexAsync(TabViewModel tab, int index, CancellationTokenSource ct);
    bool CanNavigate(TabViewModel tab);
}