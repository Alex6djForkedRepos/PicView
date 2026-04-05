using Avalonia;
using Avalonia.Media;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI;

namespace PicView.Avalonia.MacOS.Views;

public partial class MacOSTitlebar : MainTitleBar
{
    public MacOSTitlebar()
    {
        InitializeComponent();
        
        Loaded += (_, _) =>
        {
            if (!Settings.Theme.GlassTheme)
            {
                return;
            }

            TopWindowBorder.Background = Brushes.Transparent;

            EditableTitlebar.Background = Brushes.Transparent;
            EditableTitlebar.BorderThickness = new Thickness(0);

            CreateTabButton.Background = Brushes.Transparent;
            CreateTabButton.BorderThickness = new Thickness(0);;
                
            MenuButton.Background = Brushes.Transparent;
            MenuButton.BorderThickness = new Thickness(0);;
                
            var brush = UIHelper.GetBrush("SecondaryTextColor");
            EditableTitlebar.Foreground = brush;
            SearchButton.Foreground = brush;
            CreateTabButton.Foreground = brush;
            MenuButton.Foreground = brush;
        };
    }
}