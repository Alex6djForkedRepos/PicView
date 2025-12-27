using System.Collections.ObjectModel;
using R3;

namespace PicView.Core.ViewModels;

public class MainWindowOverviewViewModel
{
    public BindableReactiveProperty<ObservableCollection<MainWindowViewModel>> MainWindows { get; } = new([]);
    /// Tracks the correct position of the active window
    public BindableReactiveProperty<int> ActiveWindowIndex { get; } = new(0);
    public BindableReactiveProperty<MainWindowViewModel> ActiveWindow { get; }

    public MainWindowOverviewViewModel()
    {
    }
    
}
