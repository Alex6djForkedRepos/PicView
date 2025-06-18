using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PicView.Core.Keybindings;

namespace PicView.Avalonia.Views.UC.Buttons;

public partial class SettingsMenuButton : UserControl
{
    public SettingsMenuButton()
    {
        InitializeComponent();
        ToolTip.SetTip(UserSettingsItem, CurrentSettingsPath);
        ToolTip.SetTip(KeybindingsItem, KeybindingFunctions.CurrentKeybindingsPath);
        Loaded += (_, _) =>
        {
            if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
            {
                SettingsButton.Background = Brushes.Transparent;
                SettingsButton.BorderThickness = new Thickness(0);
            }
        };
    }
}