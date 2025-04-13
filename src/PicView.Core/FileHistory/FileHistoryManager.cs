using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using PicView.Core.ArchiveHandling;
using PicView.Core.Config;
using PicView.Core.FileHandling;

namespace PicView.Core.FileHistory;

[JsonSourceGenerationOptions(AllowTrailingCommas = true, WriteIndented = true)]
[JsonSerializable(typeof(FileHistoryEntries))]
[JsonSerializable(typeof(List<Entry>))]
internal partial class FileHistoryGenerationContext : JsonSerializerContext;

/// <summary>
///     Manages the history of recently accessed files.
/// </summary>
public static class FileHistoryManager
{
    private const int MaxHistoryEntries = 50;
    public const int MaxPinnedEntries = 5;
    private static readonly List<Entry> Entries = [];
    private static string? _fileLocation;

    // ReSharper disable once ReplaceWithFieldKeyword
    private static int _currentIndex = -1;

    /// <summary>
    ///     Gets the number of entries in the file history.
    /// </summary>
    public static int Count => Entries.Count;

    public static bool IsSortingDescending { get; set; }

    /// <summary>
    ///     Gets all history entries.
    /// </summary>
    public static IReadOnlyList<Entry> AllEntries => Entries.AsReadOnly();

    /// <summary>
    ///     Gets or sets the current index position in history.
    /// </summary>
    public static int CurrentIndex
    {
        get => _currentIndex;
        private set => _currentIndex = Math.Clamp(value, -1, Count - 1);
    }

    /// <summary>
    ///     Indicates whether there is a previous entry available in history (older entry).
    /// </summary>
    public static bool HasPrevious => CurrentIndex > 0;

    /// <summary>
    ///     Indicates whether there is a next entry available in history (newer entry).
    /// </summary>
    public static bool HasNext => CurrentIndex < Count - 1 && Count > 0;

    /// <summary>
    ///     Gets the current entry at the current index.
    /// </summary>
    public static string? CurrentEntry =>
        CurrentIndex >= 0 && CurrentIndex < Count ? Entries[CurrentIndex].Path : null;

    public static string CurrentFileHistoryFile => _fileLocation.Replace("/", "\\");

    /// <summary>
    ///     Initializes the file history by loading entries from the history file.
    /// </summary>
    /// <remarks>
    ///     Tries to create the file if it doesn't exist and sets the current index to the most recent entry.
    /// </remarks>
    public static void Initialize()
    {
        _fileLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SettingsConfiguration.LocalHistoryFilePath);
        try
        {
            if (!File.Exists(_fileLocation))
            {
                CreateFile();
            }
        }
        catch (Exception e)
        {
            // Fall back to the app data directory if the file cannot be created in the current directory.
            try
            {
                // TODO: test on macOS
                _fileLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    SettingsConfiguration.RoamingFileHistoryPath);
                if (!File.Exists(_fileLocation))
                {
                    CreateFile();
                }
            }
            catch (Exception exception)
            {
                // Log the exception in debug mode.
#if DEBUG
                Trace.WriteLine($"{nameof(FileHistory)} exception, \n{exception.Message}");
#endif
            }
#if DEBUG
            Trace.WriteLine($"{nameof(FileHistory)} exception, \n{e.Message}");
#endif
        }

        LoadFromFile();
        // Set the current index to the most recent entry.
        CurrentIndex = Count > 0 ? Count - 1 : -1;

        return;

        void CreateFile()
        {
            var directory = Path.GetDirectoryName(_fileLocation);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var fs = File.Create(_fileLocation);
            fs.Seek(0, SeekOrigin.Begin);
        }
    }

    public static void Pin(string path) =>
        Pin(path, true);

    public static void UnPin(string path) =>
        Pin(path, false);

    private static void Pin(string path, bool isPinned)
    {
        var entryIndex = Entries.FindIndex(x => x.Path == path);
        if (entryIndex < 0)
        {
            return;
        }

        // If already in the desired state, do nothing
        if (Entries[entryIndex].IsPinned == isPinned)
        {
            return;
        }

        // If trying to pin and we already have max pinned entries, don't allow it
        if (isPinned && Entries.Count(e => e.IsPinned) >= MaxPinnedEntries)
        {
            // Unpin the oldest pinned entry to make room
            var oldestPinned = Entries.Where(e => e.IsPinned).OrderBy(e => Entries.IndexOf(e)).FirstOrDefault();
            if (oldestPinned != null)
            {
                var oldestIndex = Entries.IndexOf(oldestPinned);
                Entries[oldestIndex].IsPinned = false;
            }
        }

        Entries[entryIndex].IsPinned = isPinned;
    }

    /// <summary>
    ///     Adds an entry to the history.
    /// </summary>
    public static void Add(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        // Don't add if browsing an archive, unless the file is an archive itself.
        if (!string.IsNullOrWhiteSpace(ArchiveExtraction.TempZipDirectory))
        {
            if (!path.IsArchive())
            {
                return;
            }
        }

        // Check if the entry already exists.
        var existingIndex = Entries.FindIndex(x => x.Path == path);

        if (existingIndex >= 0)
        {
            // If entry already exists, just update current index to point to it.
            CurrentIndex = existingIndex;
            return;
        }

        // Count unpinned entries
        var unpinnedCount = Entries.Count(e => !e.IsPinned);

        // If we'll exceed the maximum unpinned entries, remove the oldest unpinned entry
        if (unpinnedCount >= MaxHistoryEntries)
        {
            // Find the oldest unpinned entry
            for (var i = 0; i < Entries.Count; i++)
            {
                if (Entries[i].IsPinned)
                {
                    continue;
                }

                Entries.RemoveAt(i);

                // Adjust current index since we removed an item.
                if (CurrentIndex > i)
                {
                    CurrentIndex--;
                }

                break;
            }
        }

        // Add to the end of the list (newest entry).
        Entries.Add(new Entry { Path = path });

        // Set the current index to the newly added item (last position).
        CurrentIndex = Entries.Count - 1;
    }

    /// <summary>
    ///     Gets the next entry in history (newer entry).
    /// </summary>
    public static string? GetNextEntry()
    {
        if (!HasNext)
        {
            return null;
        }

        CurrentIndex++;
        return CurrentEntry;
    }

    /// <summary>
    ///     Gets the previous entry in history (older entry).
    /// </summary>
    public static string? GetPreviousEntry()
    {
        if (!HasPrevious)
        {
            return null;
        }

        CurrentIndex--;
        return CurrentEntry;
    }

    /// <summary>
    ///     Gets an entry at the specified index.
    /// </summary>
    public static Entry? GetEntry(int index)
    {
        if (index < 0 || index >= Entries.Count)
        {
            return null;
        }

        return Entries[index];
    }

    /// <summary>
    ///     Gets the first entry in history (oldest).
    /// </summary>
    public static string? GetFirstEntry() => Entries.Count > 0 ? Entries[0].Path : null;

    /// <summary>
    ///     Gets the last entry in history (newest).
    /// </summary>
    public static string? GetLastEntry() => Entries.Count > 0 ? Entries[^1].Path : null;

    /// <summary>
    ///     Tries to find an entry that matches or contains the given string.
    /// </summary>
    public static Entry? GetEntryByString(string searchString)
    {
        if (string.IsNullOrWhiteSpace(searchString))
        {
            return null;
        }

        // First try exact match.
        var exactMatch = Entries.FirstOrDefault(e =>
            string.Equals(e.Path, searchString, StringComparison.OrdinalIgnoreCase));

        if (exactMatch != null)
        {
            return exactMatch;
        }

        // Then try contains.
        return Entries.Find(e => e.Path.Contains(searchString, StringComparison.OrdinalIgnoreCase)) ??
               null;
    }

    /// <summary>
    ///     Clears all history entries.
    /// </summary>
    public static void Clear()
    {
        Entries.Clear();
        CurrentIndex = -1;
    }

    /// <summary>
    ///     Removes a specific entry from history.
    /// </summary>
    public static bool Remove(string path)
    {
        var index = Entries.FindIndex(e => e.Path == path);
        if (index < 0)
        {
            return false;
        }

        Entries.RemoveAt(index);

        // Adjust current index if necessary.
        if (index <= CurrentIndex)
        {
            CurrentIndex = Math.Max(-1, CurrentIndex - 1);
        }

        return true;
    }

    /// <summary>
    ///     Renames a file in the history, replacing the old entry with the new one.
    /// </summary>
    public static void Rename(string oldName, string newName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            var entry = GetEntryByString(oldName);
            if (string.IsNullOrWhiteSpace(entry?.Path) || Entries.All(x => x.Path != entry?.Path))
            {
                return;
            }

            var index = Entries.Where(x => x.Path == entry.Path).Select(x => Entries.IndexOf(x)).First();
            Entries[index].Path = newName;
        }
        catch (Exception e)
        {
#if DEBUG
            Trace.WriteLine($"{nameof(FileHistory)}: {nameof(Rename)} exception,\n{e.Message}");
#endif
        }
    }

    /// <summary>
    ///     Saves the history to the history file.
    /// </summary>
    public static void SaveToFile()
    {
        try
        {
            if (_fileLocation == null)
            {
                return;
            }

            // Create a new sorted list with pinned entries first (max 5), then unpinned entries (max MaxHistoryEntries)
            var sortedEntries = new List<Entry>();

            // Add all pinned entries first (preserving their original order) - should be max 5
            sortedEntries.AddRange(Entries.Where(e => e.IsPinned).Take(MaxPinnedEntries));

            // Then add all unpinned entries (preserving their original order) - limited by MaxHistoryEntries
            sortedEntries.AddRange(Entries.Where(e => !e.IsPinned).Take(MaxHistoryEntries));

            var historyEntries = new FileHistoryEntries
            {
                Entries = sortedEntries,
                IsSortingDescending = IsSortingDescending
            };
            var json = JsonSerializer.Serialize(historyEntries, typeof(FileHistoryEntries),
                FileHistoryGenerationContext.Default);
            File.WriteAllText(_fileLocation, json);
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Error saving file history: {ex.Message}");
#endif
        }
    }

    /// <summary>
    ///     Loads the history from the history file.
    /// </summary>
    private static void LoadFromFile()
    {
        try
        {
            if (_fileLocation == null || !File.Exists(_fileLocation))
            {
                return;
            }

            var jsonString = File.ReadAllText(_fileLocation);

            if (JsonSerializer.Deserialize(
                    jsonString, typeof(FileHistoryEntries),
                    FileHistoryGenerationContext.Default) is not FileHistoryEntries entries)
            {
                throw new JsonException("Failed to deserialize settings");
            }

            IsSortingDescending = entries.IsSortingDescending;

            foreach (var entry in entries.Entries)
            {
                Entries.Add(entry);
            }
        }
        catch (Exception ex)
        {
#if DEBUG
            Debug.WriteLine($"Error loading file history: {ex.Message}");
#endif
        }
    }
}