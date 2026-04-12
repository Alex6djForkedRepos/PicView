using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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
                
            DropMenuButton.Background = Brushes.Transparent;
            DropMenuButton.BorderThickness = new Thickness(0);;
                
            var brush = UIHelper.GetBrush("SecondaryTextColor");
            EditableTitlebar.Foreground = brush;
            SearchButton.Foreground = brush;
            CreateTabButton.Foreground = brush;
            DropMenuButton.Foreground = brush;
        };
    }
}