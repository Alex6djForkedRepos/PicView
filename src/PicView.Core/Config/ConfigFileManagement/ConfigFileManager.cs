using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.Keybindings;

namespace PicView.Core.Config.ConfigFileManagement;

/// <summary>
/// Provides functionality to manage configuration files, including saving and retrieving paths for various config file types.
/// </summary>
public static class ConfigFileManager
{
    /// <summary>
    /// Saves a configuration file and returns the file path where it was saved.
    /// </summary>
    /// <param name="type">The type of configuration file to save (e.g., user settings, file history, key bindings).</param>
    /// <param name="path">The target file path to save the configuration. If null, a default path will be used.</param>
    /// <param name="value">The object containing the configuration data to save.</param>
    /// <param name="inputType">The type of the object being serialized.</param>
    /// <param name="context">The serialization context used for JSON operations.</param>
    /// <returns>The path where the configuration file was saved, or null if the operation failed.</returns>
    public static async Task<string?> SaveConfigFileAndReturnPathAsync(ConfigFileType type, string? path, object? value,
        Type inputType, JsonSerializerContext context)
    {
        // If null, try to get the current user file, if exist
        path ??= GetConfigPath(type, ConfigPathKind.CurrentUser);

        try
        {
            CleanupOldConfigPath();
            
            if (!FileHelper.IsPathWritable(path))
            {
                return await TrySaveRoaming();
            }

            await JsonFileHelper.WriteJsonAsync(path, value, inputType, context).ConfigureAwait(false);
            return path;
        }
        catch (UnauthorizedAccessException)
        {
            // If unauthorized, try saving to roaming app data
            try
            {
                return await TrySaveRoaming();
            }
            catch (Exception ex)
            {
                DebugHelper.LogDebug(nameof(ConfigFileManager), nameof(SaveConfigFileAndReturnPathAsync), ex);
                return null;
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(ConfigFileManager), nameof(SaveConfigFileAndReturnPathAsync), ex);
            return null;
        }

        async Task<string> TrySaveRoaming()
        {
            var roamingPath = path ??= GetConfigPath(type, ConfigPathKind.Roaming);

            FileHelper.EnsureDirectoryExists(roamingPath);
            await JsonFileHelper.WriteJsonAsync(roamingPath, value, inputType, context).ConfigureAwait(false);

            return roamingPath;
        }

        // TODO delete this after next release
        void CleanupOldConfigPath()
        {
            if (!File.Exists(SettingsConfiguration.OldLocalSettingsPath))
            {
                return;
            }

            path = SettingsConfiguration.LocalSettingsPath;

            File.Delete(SettingsConfiguration.OldLocalSettingsPath);

            DeleteDirectoryIfExists(Path.GetDirectoryName(SettingsConfiguration.OldLocalSettingsPath));
            DeleteDirectoryIfExists(
                Path.GetDirectoryName(Path.GetDirectoryName(SettingsConfiguration.OldLocalSettingsPath)));
            DeleteDirectoryIfExists(Path.GetDirectoryName(
                Path.GetDirectoryName(Path.GetDirectoryName(SettingsConfiguration.OldLocalSettingsPath))));
        }

        void DeleteDirectoryIfExists(string? directory)
        {
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                Directory.Delete(directory);
            }
        }
    }

    public static string ResolveDefaultConfigPath(ConfigFileType type)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // On macOS, always use the roaming path. We can't save inside an app bundle.
            return GetConfigPath(type, ConfigPathKind.Roaming);
        }

        var path = GetConfigPath(type, ConfigPathKind.CurrentUser);
        return path.Replace("/", "\\");
    }

    private static string GetConfigPath(ConfigFileType type, ConfigPathKind kind)
    {
        switch (kind)
        {
            case ConfigPathKind.CurrentUser:
                switch (type)
                {
                    case ConfigFileType.FileHistory:
                        var currentFileHistoryPath = FileHistoryConfiguration.CurrentUserFileHistoryPath;
                        if (!string.IsNullOrEmpty(currentFileHistoryPath))
                        {
                            return currentFileHistoryPath;
                        }

                        return FileHelper.IsPathWritable(currentFileHistoryPath) ?
                            FileHistoryConfiguration.LocalFileHistoryPath : FileHistoryConfiguration.RoamingFileHistoryPath;

                    case ConfigFileType.KeyBindings:
                        var currentKeybindingsPath = KeyBindingsConfiguration.CurrentUserKeybindingsPath;
                        if (!string.IsNullOrEmpty(currentKeybindingsPath))
                        {
                            return currentKeybindingsPath;
                        }
                        
                        return FileHelper.IsPathWritable(currentKeybindingsPath) ?
                                KeyBindingsConfiguration.LocalKeybindingsPath : KeyBindingsConfiguration.RoamingKeybindingsPath;
                    case ConfigFileType.UserSettings:
                    default:
                        if (File.Exists(SettingsConfiguration.OldLocalSettingsPath))
                        {
                            return SettingsConfiguration.OldLocalSettingsPath;
                        }

                        var currentSettingsPath = SettingsConfiguration.CurrentUserSettingsPath;
                        if (!string.IsNullOrEmpty(currentSettingsPath))
                        {
                            return currentSettingsPath;
                        }
                        
                        return FileHelper.IsPathWritable(currentSettingsPath) ?
                            SettingsConfiguration.LocalSettingsPath : SettingsConfiguration.RoamingSettingsPath;
                }
            case ConfigPathKind.Roaming:
                return type switch
                {
                    ConfigFileType.FileHistory => FileHistoryConfiguration.RoamingFileHistoryPath,
                    ConfigFileType.KeyBindings => KeyBindingsConfiguration.RoamingKeybindingsPath,
                    _ => SettingsConfiguration.RoamingSettingsPath
                };
            case ConfigPathKind.Local:
                return type switch
                {
                    ConfigFileType.FileHistory => FileHistoryConfiguration.LocalFileHistoryPath,
                    ConfigFileType.KeyBindings => KeyBindingsConfiguration.LocalKeybindingsPath,
                    _ => SettingsConfiguration.LocalSettingsPath
                };
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, "Invalid configuration path kind.");
        }
    }
}