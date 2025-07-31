using System.Globalization;
using Avalonia.Controls;
using PicView.Avalonia.ViewModels;
using PicView.Core.DebugTools;
using PicView.Core.Extensions;
using PicView.Core.Localization;

namespace PicView.Avalonia.Resizing;

public static class AspectRatioHelper
{
    /// <summary>
    /// Adjusts the dimensions of the TextBoxes while maintaining the specified aspect ratio.
    /// </summary>
    /// <param name="widthTextBox">The TextBox that contains the width value.</param>
    /// <param name="heightTextBox">The TextBox that contains the height value.</param>
    /// <param name="isWidth">Indicates whether the width is being adjusted. If false, height is adjusted.</param>
    /// <param name="aspectRatio">The aspect ratio to maintain between width and height.</param>
    /// <param name="vm">The MainViewModel instance containing relevant data.</param>
    public static void SetAspectRatioForTextBox(TextBox widthTextBox, TextBox heightTextBox, bool isWidth,
        double aspectRatio, MainViewModel vm)
    {
        try
        {
            var percentage = isWidth ? widthTextBox.Text.GetPercentage() : heightTextBox.Text.GetPercentage();
            if (percentage > 0)
            {
                // Clamp the calculated value to prevent overflow
                var newWidth = (uint)Math.Clamp(vm.PicViewer.PixelWidth.CurrentValue * (percentage / 100),
                    uint.MinValue,
                    uint.MaxValue);
                var newHeight = (uint)Math.Clamp(vm.PicViewer.PixelHeight.CurrentValue * (percentage / 100),
                    uint.MinValue,
                    uint.MaxValue);

                widthTextBox.Text = newWidth.ToString("# ", CultureInfo.CurrentCulture);
                heightTextBox.Text = newHeight.ToString("# ", CultureInfo.CurrentCulture);

                if (isWidth)
                {
                    heightTextBox.Text = newHeight.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    widthTextBox.Text = newWidth.ToString(CultureInfo.CurrentCulture);
                }
            }
            else
            {
                if (!uint.TryParse(widthTextBox.Text, out var width) ||
                    !uint.TryParse(heightTextBox.Text, out var height))
                {
                    // Invalid input, delete last character
                    // TODO: Find a more user friendly solution
                    if (isWidth && widthTextBox.Text.Length > 1)
                    {
                        widthTextBox.Text = widthTextBox.Text[..^1];
                    }
                    else if (heightTextBox.Text.Length > 1)
                    {
                        heightTextBox.Text = heightTextBox.Text[..^1];
                    }
                }
                else
                {
                    if (isWidth)
                    {
                        // Clamp the calculated value to prevent overflow
                        var newHeight = (uint)Math.Clamp(Math.Round(width / aspectRatio), uint.MinValue, uint.MaxValue);
                        heightTextBox.Text = newHeight.ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        // Clamp the calculated value to prevent overflow
                        var newWidth = (uint)Math.Clamp(Math.Round(height * aspectRatio), uint.MinValue, uint.MaxValue);
                        widthTextBox.Text = newWidth.ToString(CultureInfo.CurrentCulture);
                    }
                }
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(AspectRatioHelper), nameof(SetAspectRatioForTextBox), e);
        }
    }

    public static PrintSizes GetPrintSizes(int pixelWidth, int pixelHeight, double dpiX, double dpiY)
    {
        var cm = TranslationManager.Translation.Centimeters;
        var mp = TranslationManager.Translation.MegaPixels;
        var inches = TranslationManager.Translation.Inches;
        var inchesWidth = pixelWidth / dpiX;
        var inchesHeight = pixelHeight / dpiY;
        var printSizeInch =
            $"{inchesWidth.ToString("0.##", CultureInfo.CurrentCulture)} x {inchesHeight.ToString("0.##", CultureInfo.CurrentCulture)} {inches}";

        var cmWidth = pixelWidth / dpiX * 2.54;
        var cmHeight = pixelHeight / dpiY * 2.54;
        var printSizeCm =
            $"{cmWidth.ToString("0.##", CultureInfo.CurrentCulture)} x {cmHeight.ToString("0.##", CultureInfo.CurrentCulture)} {cm}";
        var sizeMp =
            $"{((float)pixelHeight * pixelWidth / 1000000).ToString("0.##", CultureInfo.CurrentCulture)} {mp}";

        return new PrintSizes(printSizeCm, printSizeInch, sizeMp);
    }

    public static string GetFormattedAspectRatio(int gcd, int width, int height)
    {
        var square = TranslationManager.Translation.Square;
        var landscape = TranslationManager.Translation.Landscape;
        var portrait = TranslationManager.Translation.Portrait;

        var firstRatio = width / gcd;
        var secondRatio = height / gcd;

        if (firstRatio == secondRatio)
        {
            return $"{firstRatio}:{secondRatio} ({square})";
        }

        return firstRatio > secondRatio
            ? $"{firstRatio}:{secondRatio} ({landscape})"
            : $"{firstRatio}:{secondRatio} ({portrait})";
    }

    public readonly struct PrintSizes(string printSizeCm, string printSizeInch, string sizeMp)
    {
        public string PrintSizeCm { get; } = printSizeCm;
        public string PrintSizeInch { get; } = printSizeInch;
        public string SizeMp { get; } = sizeMp;
    }
}