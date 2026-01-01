using System.Text.Json;
using Avalonia.Input;
using PicView.Core.DebugTools;
using PicView.Core.IPlatform;
using PicView.Core.Keybindings;

namespace PicView.Avalonia.Input;

public static class KeybindingManager2
{
    public static Dictionary<KeyGesture, string>? CustomShortcuts { get; private set; }

    public static async ValueTask LoadKeybindings(IPlatformSpecificService platformSpecificService)
    {
        var keybindings = await KeybindingFunctions.LoadKeyBindingsFile().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(keybindings))
        {
            SetDefaultKeybindings(platformSpecificService);
        }
        else
        {
            UpdateKeybindings(keybindings);
        }
    }

    private static void UpdateKeybindings(string json)
    {
        // Deserialize JSON into a dictionary of string keys and string values
        var keyValues = JsonSerializer.Deserialize(
                json, typeof(Dictionary<string, string>), SourceGenerationContext.Default)
            as Dictionary<string, string>;

        CustomShortcuts ??= new Dictionary<KeyGesture, string>();
        PopulateCustomShortcuts(keyValues);
    }

    public static async ValueTask UpdateKeyBindingsFile()
    {
        try
        {
            var json = JsonSerializer.Serialize(
                CustomShortcuts.ToDictionary(kvp => kvp.Key.ToString(),
                    kvp => kvp.Value), typeof(Dictionary<string, string>),
                SourceGenerationContext.Default).Replace("\\u002B", "+"); // Fix plus sign encoded to Unicode
            await KeybindingFunctions.SaveKeyBindingsFile(json).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            DebugHelper.LogDebug(nameof(KeybindingManager2), nameof(UpdateKeyBindingsFile), exception);
        }
    }

    private static void PopulateCustomShortcuts(Dictionary<string, string> keyValues)
    {
        foreach (var kvp in keyValues)
        {
            try
            {
                var gesture = KeyGesture.Parse(kvp.Key);
                if (gesture is null)
                {
                    continue;
                }

                // Add to the dictionary
                if (!string.IsNullOrWhiteSpace(kvp.Value))
                {
                    CustomShortcuts[gesture] = kvp.Value;
                }
            }
            catch (Exception exception)
            {
                DebugHelper.LogDebug(nameof(KeybindingManager2), nameof(PopulateCustomShortcuts), exception);
            }
        }
    }

    public static void SetDefaultKeybindings(IPlatformSpecificService platformSpecificService)
    {
        if (CustomShortcuts is not null)
        {
            CustomShortcuts.Clear();
        }
        else
        {
            CustomShortcuts = new Dictionary<KeyGesture, string>();
        }
        var defaultKeybindings = platformSpecificService.DefaultJsonKeyMap();
        var keyValues = JsonSerializer.Deserialize(
                defaultKeybindings, typeof(Dictionary<string, string>), SourceGenerationContext.Default)
            as Dictionary<string, string>;

        PopulateCustomShortcuts(keyValues);
    }
    
    public static string? GetFunctionName(KeyGesture gesture)
    {
        if (CustomShortcuts != null && CustomShortcuts.TryGetValue(gesture, out var functionName))
        {
            return functionName;
        }
        return null;
    }
}
