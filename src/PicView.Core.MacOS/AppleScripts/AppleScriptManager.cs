using System.Diagnostics;

namespace PicView.Core.MacOS.AppleScripts;

public static class AppleScriptManager
{
    public static async Task<bool> ExecuteAppleScriptAsync(string appleScript)
    {
        var scriptPath = Path.Combine(Path.GetTempPath(), "appleScript.scpt");
        await File.WriteAllTextAsync(scriptPath, appleScript);
        
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
        await process.WaitForExitAsync();
        
        // Clean up the script file
        try
        {
            File.Delete(scriptPath);
        }
        catch
        {
            // Ignore deletion errors
        }
        return process.ExitCode == 0;
    }
}
