using System.Collections.ObjectModel;
using PicView.Core.Models;
using PicView.Core.Navigation;
using R3;

namespace PicView.Core.ViewModels;

public class NavigationViewModel : IDisposable
{
    public BindableReactiveProperty<ObservableCollection<TabModel>>? Tabs { get; } = new([]);
    public BindableReactiveProperty<int> ActiveTabIndex { get; } = new(0);

    private readonly IImageIteratorFactory? _iteratorFactory;
    private readonly INavigationService? _navService;
    private readonly IImageCache? _sharedCache;

    public NavigationViewModel(IImageIteratorFactory iteratorFactory, INavigationService navService, IImageCache cache)
    {
        _iteratorFactory = iteratorFactory;
        _navService = navService;
        _sharedCache = cache;
        
        for (var i = 0; i < 6; i++)
        {
            var randomFileName = Path.GetRandomFileName();
            Tabs.Value.Add(new TabModel
            {
                TabTitle = randomFileName,
                TabTooltip = "tooltip"
            });
        }
    }

    public TabViewModel CreateTab(FileInfo? file = null)
    {
        var picModel = new PicViewerModel();
        var iterator = _iteratorFactory.Create(file ?? new FileInfo(Environment.CurrentDirectory));

        var tab = new TabViewModel(picModel, iterator);
        Tabs.Value.Add(new TabModel
        {
            TabTitle = file?.Name ?? "New Tab"
        });
        ActiveTabIndex.Value = Tabs.Value.Count - 1;
        return tab;
    }

    public void CloseTab(TabModel tab)
    {
        Tabs.Value.Remove(tab);
        if (Tabs.Value.Count == 0)
        {
            // maybe create an empty tab
        }
    }

    public void Dispose()
    {
        Tabs.Value.Clear();
    }
}
