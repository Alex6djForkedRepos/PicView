using System.Diagnostics;
using System.IO;

namespace PicView.Core.MacOS.Wallpaper
{
    public static class WallpaperHelper
    {
        public enum WallpaperStyle
        {
            FillScreen,
            FitToScreen,
            StretchToFillScreen,
            Center
        }

        public static void SetWallpaper(string imagePath, int style)
        {
            // Convert backslashes to forward slashes for macOS paths
            imagePath = imagePath.Replace("\\", "/");
            
            var script = (WallpaperStyle)style switch
            {
                WallpaperStyle.FillScreen =>
                    $"tell application \"System Events\"\n" +
                    $"    tell every desktop\n" +
                    $"        set picture to \"{imagePath}\"\n" +
                    $"        set picture placement to 1\n" + // Fill
                    $"    end tell\n" +
                    $"end tell",
                    
                WallpaperStyle.FitToScreen =>
                    $"tell application \"System Events\"\n" +
                    $"    tell every desktop\n" +
                    $"        set picture to \"{imagePath}\"\n" +
                    $"        set picture placement to 2\n" + // Fit
                    $"    end tell\n" +
                    $"end tell",
                    
                WallpaperStyle.StretchToFillScreen =>
                    $"tell application \"System Events\"\n" +
                    $"    tell every desktop\n" +
                    $"        set picture to \"{imagePath}\"\n" +
                    $"        set picture placement to 3\n" + // Stretch
                    $"    end tell\n" +
                    $"end tell",
                    
                WallpaperStyle.Center =>
                    $"tell application \"System Events\"\n" +
                    $"    tell every desktop\n" +
                    $"        set picture to \"{imagePath}\"\n" +
                    $"        set picture placement to 4\n" + // Center
                    $"    end tell\n" +
                    $"end tell",
                    
                _ => $"tell application \"System Events\" to tell every desktop to set picture to \"{imagePath}\""
            };

            var scriptPath = Path.Combine(Path.GetTempPath(), "SetWallpaper.scpt");
            File.WriteAllText(scriptPath, script);
            
            // Execute the script
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = scriptPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            
            process.Start();
            process.WaitForExit();
            
            // Clean up the script file
            try
            {
                File.Delete(scriptPath);
            }
            catch
            {
                // Ignore deletion errors
            }
        }
    }
}