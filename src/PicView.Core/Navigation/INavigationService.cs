using PicView.Core.ViewModels;

namespace PicView.Core.Navigation;

public interface INavigationService : IAsyncDisposable
{
    Task LoadFromPathAsync(string source, TabViewModel tab, CancellationToken ct);
    Task NavigateAsync(TabViewModel tab, NavigateTo to, CancellationToken ct);
    Task NavigateToIndexAsync(TabViewModel tab, int index, CancellationToken ct);
    bool CanNavigate(TabViewModel tab);
}