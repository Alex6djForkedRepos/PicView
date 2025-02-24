using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;

namespace PicView.Avalonia.ColorManagement;
public static class ThemeManager
{
    public enum Theme
    {
        Dark = 0,
        Light = 1,
        Glass = 2
    }
    
    public static void SetTheme(Theme theme)
    {
        var application = Application.Current;
        if (application is null)
            return;
        
        // StyleInclude breaks trimming and AOT

        switch (theme)
        {
            default:
                Settings.Theme.Dark = true;
                Settings.Theme.GlassTheme = false;
                application.RequestedThemeVariant = ThemeVariant.Dark;
                break;
            case Theme.Light:
                Settings.Theme.Dark = false;
                Settings.Theme.GlassTheme = false;
                application.RequestedThemeVariant = ThemeVariant.Light;
                break;
            case Theme.Glass:
                Settings.Theme.GlassTheme = true;
                application.RequestedThemeVariant = ThemeVariant.Light;
                GlassThemeUpdates();
                break;
        }
        
        ColorManager.UpdateAccentColors(Settings.Theme.ColorTheme);
    }

    public static void GlassThemeUpdates()
    {
        if (!Application.Current.TryGetResource("MainTextColor",
                ThemeVariant.Dark, out var textColor))
        {
            return;
        }

        if (textColor is not Color mainColor)
        {
            return;
        }

        Application.Current.Resources["MainTextColor"] = mainColor;
        
        Application.Current.Resources["MainButtonBackgroundColor"] = Color.Parse("#4D000000");
        Application.Current.Resources["MainBackgroundColor"] = Color.Parse("#4D000000");
        
        Application.Current.Resources["SecondaryButtonBackgroundColor"] = Color.Parse("#D1464646");
        Application.Current.Resources["SecondaryBackgroundColor"] = Color.Parse("#DE5B5B5B");
        
        Application.Current.Resources["DisabledBackgroundColor"] = Color.Parse("#4D5B5B5B");

        Application.Current.Resources["MainBorderColor"] = Colors.Transparent;
        Application.Current.Resources["SecondaryBorderColor"] = Colors.Transparent;
        Application.Current.Resources["TertiaryBorderColor"] = Colors.Transparent;
        
        Application.Current.Resources["ContextMenuTextColor"] = mainColor;
        Application.Current.Resources["ContextMenuBackgroundColor"] = Color.Parse("#A1464646");
            
        Application.Current.Resources["MenuBackgroundColor"] = Color.Parse("#6D5B5B5B");
        Application.Current.Resources["MenuButtonColor"] = Color.Parse("#50797979");
        
        Application.Current.Resources["WindowBorderColor"] = Color.Parse("#15FFFFFF");
        
    }

    public static void DetermineTheme(Application? application, bool settingsExists)
    {
        if (!settingsExists)
        {
            application.RequestedThemeVariant = application.ActualThemeVariant;
            Settings.Theme.Dark = application.RequestedThemeVariant == ThemeVariant.Dark;
        }
        else
        {
            if (Settings.Theme.GlassTheme)
            {
                application.RequestedThemeVariant = ThemeVariant.Light;
            }
            else if (Settings.Theme.UseSystemTheme)
            {
                application.RequestedThemeVariant = ThemeVariant.Dark; // TODO : Figure out how to get the system theme
            }
            else
            {
                application.RequestedThemeVariant = Settings.Theme.Dark ? ThemeVariant.Dark : ThemeVariant.Light;
            }
        }
    }
}
