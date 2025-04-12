using Avalonia.Controls;
using Avalonia.Media;

namespace PicView.Avalonia.Views.UC.Buttons;
public partial class PinButton : UserControl
{
    public PinButton()
    {
        InitializeComponent();
        PointerEntered += (_, e) =>
        {
            Background = new SolidColorBrush { Color = Color.FromArgb(45, 0, 0, 0) };
        };
        PointerExited += (_, e) =>
        {
            Background = Brushes.Transparent;
        };
    }
}
