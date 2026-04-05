using Avalonia.Controls;
using PicView.Avalonia.Views.UC;
using R3;

namespace PicView.Avalonia.Interfaces;

public interface IMainWindow
{
    CompositeDisposable Disposables { get; set; }
    
    bool IsChangingWindowState { get; set; }
    
    BottomBar2? SharedBottomBar { get; set; }
    
    UserControl? SharedTitleBar { get; set; }
}
