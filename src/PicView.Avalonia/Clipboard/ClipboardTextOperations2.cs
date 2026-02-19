using Avalonia.Input.Platform;

namespace PicView.Avalonia.Clipboard;

/// <summary>
/// Handles clipboard operations related to text
/// </summary>
public static class ClipboardTextOperations2
{
    /// <summary>
    /// Copies text to the clipboard
    /// </summary>
    /// <param name="text">The text to copy</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public static async Task<bool> CopyTextToClipboard(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }
        
        var clipboard = ClipboardService2.GetClipboard();
        if (clipboard == null)
        {
            return false;
        }

        return await ClipboardService2.ExecuteClipboardOperation(async () =>
        {
            await clipboard.SetTextAsync(text);
            return true;
        }, showAnimation: true);
    }
}