using Avalonia;
using Avalonia.Media;
using PicView.Avalonia.ColorManagement;
using PicView.Core.ViewModels;
using PicView.Core.ColorHandling;
using PicView.Core.ISettings;

namespace PicView.Avalonia.Services;

public class ThemeService : IThemeService
{
    public void SetTheme(int themeIndex)
    {
        if (Enum.IsDefined(typeof(ThemeManager.Theme), themeIndex))
        {
            ThemeManager.SetTheme((ThemeManager.Theme)themeIndex);
        }
    }

    public void SetColorTheme(int colorIndex)
    {
        ColorManager.UpdateAccentColors(colorIndex);
    }

    public void SetBackground(int backgroundIndex)
    {
        var coreVm = Application.Current?.DataContext as CoreViewModel;
        if (coreVm?.MainWindows.ActiveWindow.Value is not { } activeWindow)
        {
            return;
        }

        Settings.UIProperties.BgColorChoice = backgroundIndex;
                 
        var brush = BackgroundManager.GetBackgroundBrush((BackgroundType)backgroundIndex);
                 
        if (Settings.UIProperties.IsConstrainBackgroundColorEnabled)
        {
            activeWindow.ImageBackground.Value = new SolidColorBrush(Colors.Transparent);
            activeWindow.ConstrainedImageBackground.Value = brush;
        }
        else
        {
            activeWindow.ImageBackground.Value = brush;
            activeWindow.ConstrainedImageBackground.Value = new SolidColorBrush(Colors.Transparent);
        }
                 
        activeWindow.BackgroundChoice.Value = backgroundIndex;
    }
    
    private Brush GetNoiseTextureBrush()
    {
         if (Application.Current.TryGetResource("NoisyTexture", Application.Current.RequestedThemeVariant, out var texture) && 
            texture is ImageBrush imageBrush)
        {
            return imageBrush;
        }
        return new SolidColorBrush(Colors.Transparent);
    }
}
