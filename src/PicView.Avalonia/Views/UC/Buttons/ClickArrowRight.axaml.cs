using Avalonia.Controls;
using PicView.Avalonia.ViewModels;

namespace PicView.Avalonia.Views.UC.Buttons;
public partial class ClickArrowRight : UserControl
{
    public ClickArrowRight()
    {
        InitializeComponent();
        Loaded += delegate
        {
            if (DataContext is not MainViewModel vm)
            {
                return;
            }
            PointerWheelChanged += async (_, e) => await vm.ImageViewer.PreviewOnPointerWheelChanged(this, e);
        };
    }
}
