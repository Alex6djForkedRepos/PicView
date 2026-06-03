using System.Collections.ObjectModel;
using PicView.Core.ArchiveHandling;
using PicView.Core.DebugTools;
using PicView.Core.Extensions;
using PicView.Core.FileHandling;
using PicView.Core.FileHistory;
using PicView.Core.FileSearch;
using PicView.Core.FileSorting;
using PicView.Core.Gallery;
using PicView.Core.Http;
using PicView.Core.IPlatform;
using PicView.Core.Localization;
using PicView.Core.Models;
using PicView.Core.Navigation.Interfaces;
using PicView.Core.Preloading;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Core.Navigation;

public class NavigationService(
    IImageLoader imageLoader,
    IImageCache cache,
    IFileWatcherService fileWatcherService,
    IPlatformSpecificService platformService,
    IThumbnailLoader thumbnailLoader,
    Func<string, string, int> stringComparer)
    : INavigationService
{
    
    public BindableReactiveProperty<ObservableCollection<FileSearchResult>?>? FilteredFileInfos { get; set; }
    public ReactiveCommand<string>? LoadFromStringCommand { get; set; }

    public async ValueTask RepopulateIterator(FileInfo fileInfo, TabViewModel tab, CancellationTokenSource ct, List<FileInfo>? files = null)
    {
        try
        {
            fileWatcherService.Unwatch(tab);
            fileWatcherService.Watch(tab, fileInfo.DirectoryName);

            // Show image quickly to make it feel fast
            var model = await imageLoader.GetImageModelAsync(fileInfo, ct.Token).ConfigureAwait(false);
            tab.Model = model; // Image updated via reactive subscription
            
            tab.ImageIterator.Files = files ?? FileListRetriever.RetrieveFiles(fileInfo, stringComparer);
            var index = FindIndex(fileInfo, tab);
            tab.ImageIterator.SetCurrentIndex(index);
            
            tab.UpdateTabTitle();
            cache.Clear(tab.Id);
            cache.Add(tab.Id, index, new PreLoadValue(model), tab.ImageIterator.Files.Count, false);
            cache.Preload(tab.Id, index, false, tab.ImageIterator.Files, tab.GetTabCancellation().Token);

            if ((tab.Gallery.IsDockedGalleryVisible.CurrentValue || tab.Gallery.IsGalleryExpanded.CurrentValue) && tab.ThumbnailCache != null)
            {
                tab.Gallery.LoadingState = GalleryLoadingState.NotLoaded;
                await GalleryLoader.LoadGalleryAsync(tab, tab.ImageIterator.Files, thumbnailLoader, tab.ThumbnailCache, ct.Token).ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(RepopulateIterator), e);
        }
    }

    public async ValueTask LoadFromFileAsync(string source, TabViewModel tab, CancellationTokenSource ct)
    {
        ArgumentNullException.ThrowIfNull(source);
        await LoadFromFileAsync(new FileInfo(source), tab, ct).ConfigureAwait(false);
    }

    public async ValueTask LoadFromFileAsync(FileInfo fileInfo, TabViewModel tab, CancellationTokenSource ct)
    {
        if (!fileInfo.Exists)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(LoadFromFileAsync), $"Attempted to load a file that does not exist: {fileInfo}");
            return;
        }
        var iterator = tab.ImageIterator;

        if (iterator.Files is null || iterator.Files.Count == 0)
        {
            await Repopulate();
            return;
        }

        if (iterator.Files.Contains(fileInfo))
        {
            var index = FindIndex(fileInfo, tab);
            await tab.ImageIterator.IterateToIndexAsync(index, ct).ConfigureAwait(false);
        }
        else
        {
            await Repopulate();
        }
        
        ArchiveExtraction.Cleanup();

        return;

        async ValueTask Repopulate()
        {
            await RepopulateIterator(fileInfo, tab, ct).ConfigureAwait(false);
        }
    }

    public async ValueTask LoadFromDirectoryAsync(FileInfo source, TabViewModel tab, CancellationTokenSource ct)
    {
        var files = await Task.Run(() => FileListRetriever.RetrieveFiles(source, stringComparer), ct.Token).ConfigureAwait(false);
        if (files.Count == 0)
        {
            return;
        }

        var first = files[0];
        await RepopulateIterator(first, tab, ct, files).ConfigureAwait(false);
        ArchiveExtraction.Cleanup();
    }

    public async ValueTask<bool> LoadFromStringAsync(string source, TabViewModel tab, CancellationTokenSource ct)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        var check = FileTypeResolver.CheckIfLoadableString(source);
        if (check == null)
        {
            return false;
        }

        switch (check.Value.Type)
        {
            case FileTypeResolver.LoadAbleFileType.File:
                await LoadFromFileAsync(check.Value.Data, tab, ct).ConfigureAwait(false);
                return true;
            case FileTypeResolver.LoadAbleFileType.Directory:
            {
                await LoadFromDirectoryAsync(new FileInfo(check.Value.Data), tab, ct).ConfigureAwait(false);
                return true;
            }
            case FileTypeResolver.LoadAbleFileType.Web:
                await LoadFromUrlAsync(check.Value.Data, tab, ct).ConfigureAwait(false);
                return true;
            case FileTypeResolver.LoadAbleFileType.Zip:
                return await LoadFromArchiveAsync(check.Value.Data, tab, ct).ConfigureAwait(false);
            case FileTypeResolver.LoadAbleFileType.Base64:
                throw new NotImplementedException();
            default:
                return false;
        }
    }

    public async ValueTask<bool> LoadFromArchiveAsync(string archivePath, TabViewModel tab, CancellationTokenSource ct)
    {
        if (string.IsNullOrEmpty(archivePath) || !File.Exists(archivePath))
        {
            return false;
        }

        // Clean up any previous archive extraction before opening a new one
        ArchiveExtraction.Cleanup();

        var preparation = await ArchiveExtraction.PrepareArchiveAsync(
            archivePath,
            platformService.ExtractWithLocalSoftwareAsync,
            stringComparer).ConfigureAwait(false);

        if (preparation is null || string.IsNullOrEmpty(ArchiveExtraction.TempZipDirectory))
        {
            return false;
        }

        if (ct.IsCancellationRequested)
        {
            return false;
        }

        var prep = preparation.Value;

        if (prep.IsFullyExtracted)
        {
            // Local-software extractor already wrote every file to disk; build the file list
            // from the already-extracted paths so we don't depend on FileListRetriever's
            // recursion settings.
            var allFiles = prep.EntryKeys.Select(p => new FileInfo(p)).ToList();
            if (allFiles.Count == 0)
            {
                return false;
            }

            await RepopulateIterator(allFiles[0], tab, ct, allFiles).ConfigureAwait(false);

            FileHistoryManager.Add(archivePath);
            return true;
        }

        // Staged extraction: extract the first entry, navigate to it, then extract the rest in
        // the background while FileWatcherService inserts each new file into the iterator.
        var firstKey = prep.EntryKeys[0];
        var firstPath = await ArchiveExtraction.ExtractEntryAsync(archivePath, firstKey, ct.Token).ConfigureAwait(false);

        if (string.IsNullOrEmpty(firstPath) || ct.IsCancellationRequested)
        {
            return false;
        }

        // Seed the iterator with just the first extracted file. Watching the temp directory
        // first ensures every subsequent file creation event is captured by FileWatcherService
        // and inserted in sorted order into the iterator/gallery.
        var seedFiles = new List<FileInfo> { new(firstPath) };
        await RepopulateIterator(seedFiles[0], tab, ct, seedFiles).ConfigureAwait(false);

        // Kick off background extraction of remaining entries. FileWatcherService picks them up.
        if (prep.EntryKeys.Count > 1)
        {
            var remainingKeys = prep.EntryKeys.Skip(1).ToList();
            var backgroundToken = tab.GetTabCancellation().Token;
            _ = Task.Run(() => ArchiveExtraction.ExtractRemainingAsync(archivePath, remainingKeys, backgroundToken), backgroundToken);
        }

        FileHistoryManager.Add(archivePath);
        return true;
    }

    public async ValueTask LoadFromUrlAsync(string url, TabViewModel tab, CancellationTokenSource ct)
    {
        tab.ImageIterator?.Dispose();

        platformService.StopTaskbarProgress();
        var safeFileName = HttpManager.GetSafeFileName(url);
        var destPath = TempFileManager.GetNewTempFilePath(safeFileName);
        
        using var client = new HttpClientDownloadWithProgress(url, destPath);
        client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
        {
            var displayProgress = HttpManager.GetProgressDisplay(totalFileSize, totalBytesDownloaded, progressPercentage);
            var title = $"{safeFileName} {TranslationManager.Translation?.Downloading} {displayProgress}";

            // Update UI properties
            if (tab.TabTitle.Value != title) tab.TabTitle.Value = title;
            if (tab.Title.Value != title) tab.Title.Value = title;
            if (tab.WindowTitle.Value != title) tab.WindowTitle.Value = title;
            if (tab.TitleTooltip.Value != title) tab.TitleTooltip.Value = title;

            if (totalBytesDownloaded.HasValue && totalFileSize.HasValue)
            {
                platformService.SetTaskbarProgress((ulong)totalBytesDownloaded.Value, (ulong)totalFileSize.Value);
            }
        };

        try
        {
            await client.StartDownloadAsync(ct.Token).ConfigureAwait(false);
            
            platformService.StopTaskbarProgress();

            if (ct.IsCancellationRequested)
            {
                return;
            }
            
            var model = await imageLoader.GetImageModelAsync(new FileInfo(destPath), ct.Token).ConfigureAwait(false);
            tab.Model = model;
            tab.SecondaryModel = null;
            
            // Set titles to filename after successful load
            tab.SourceURL = url;
            tab.SingleImageType = SingleImageType.Url;
            tab.UpdateTabTitle();
            
            tab.CanNavigateBackwards.Value = false;
            tab.CanNavigateForwards.Value = false;

            FileHistoryManager.Add(url);
            ArchiveExtraction.Cleanup();
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(LoadFromUrlAsync), e);
            platformService.StopTaskbarProgress();
            // Revert or show error state if needed
            tab.TabTitle.Value = TranslationManager.Translation?.ErrorLoadingImage ?? "Error";
        }
    }

    public async ValueTask NavigateAsync(TabViewModel tab, NavigateTo to, CancellationTokenSource ct)
    {
        if (!CanNavigate(tab))
        {
            return;
        }

        if (tab.ImageIterator is null)
        {
            return;
        }
        
        await tab.ImageIterator.NavigateAsync(to, SkipAmount.One, ct).ConfigureAwait(false);
    }

    public async ValueTask NavigateByIncrementsAsync(TabViewModel tab, SkipAmount skipAmount, bool forwards, CancellationTokenSource ct)
    {
        var iterator = tab.ImageIterator;
        if (iterator is null)
        {
            return;
        }
        await iterator.NavigateByIncrementsAsync(skipAmount,forwards, ct).ConfigureAwait(false);
    }
    
    public async ValueTask<bool> LoadLastFileAsync(TabViewModel tab, CancellationTokenSource ct)
    {
        var lastFile = Settings.StartUp.LastFile;
        var lastEntry = FileHistoryManager.GetLastEntry();

        // determine which file source to use (prioritize LastFile, fallback to History)
        var fileToLoad = !string.IsNullOrEmpty(lastFile) ? lastFile : lastEntry;
        if (string.IsNullOrEmpty(lastEntry))
        {
            return false;
        }

        await LoadFromStringAsync(fileToLoad, tab, ct).ConfigureAwait(false);
        return true;
    }

    public bool CanNavigate(TabViewModel tab) => tab?.ImageIterator?.Files?.Count > 0;

    public async ValueTask NavigateToNextFolderAsync(TabViewModel tab, CancellationTokenSource ct)
    {
        var currentDir = tab.Model?.FileInfo?.DirectoryName;
        if (currentDir == null)
        {
            return;
        }

        var nextDir = await Task.Run(() => FindNextValidDirectory(currentDir), ct.Token).ConfigureAwait(false);
        if (nextDir != null)
        {
            await LoadFromDirectoryAsync(new FileInfo(nextDir), tab, ct).ConfigureAwait(false);
        }
    }

    public async ValueTask NavigateToPreviousFolderAsync(TabViewModel tab, CancellationTokenSource ct)
    {
        var currentDir = tab.Model?.FileInfo?.DirectoryName;
        if (currentDir == null)
        {
            return;
        }

        var prevDir = await Task.Run(() => FindPreviousValidDirectory(currentDir), ct.Token).ConfigureAwait(false);
        if (prevDir != null)
        {
            await LoadFromDirectoryAsync(new FileInfo(prevDir), tab, ct).ConfigureAwait(false);
        }
    }

    public async ValueTask NavigateToNextArchiveAsync(TabViewModel tab, CancellationTokenSource ct) 
        => await NavigateArchiveCoreAsync(tab, true, ct).ConfigureAwait(false);

    public async ValueTask NavigateToPreviousArchiveAsync(TabViewModel tab, CancellationTokenSource ct)
        => await NavigateArchiveCoreAsync(tab, false, ct).ConfigureAwait(false);

    private async ValueTask NavigateArchiveCoreAsync(TabViewModel tab, bool next, CancellationTokenSource ct)
    {
        var currentFile = ArchiveExtraction.IsArchived ?
            new FileInfo(ArchiveExtraction.LastOpenedArchive) : tab.Model.FileInfo;
           
        var currentDir = currentFile?.DirectoryName;
        if (currentDir == null)
        {
            return;
        }

        var nextArchive = 
            await Task.Run(() => FindNextArchive(currentDir, next, currentFile!.FullName), ct.Token).ConfigureAwait(false);
        if (nextArchive != null)
        {
            await LoadFromArchiveAsync(nextArchive, tab, ct).ConfigureAwait(false);
        }
    }

    private List<FileInfo> GetSortedArchivesInDirectory(string path)
    {
        try
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                return [];
            }

            var archives = dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
                .Where(f => f.FullName.IsArchive())
                .ToList();

            if (!Settings.Sorting.Ascending)
            {
                archives.Sort((x, y) => stringComparer(y.Name, x.Name));
            }
            else
            {
                archives.Sort((x, y) => stringComparer(x.Name, y.Name));
            }

            return archives;
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(GetSortedArchivesInDirectory), ex);
            return [];
        }
    }

    private string? FindNextArchive(string currentDir, bool next, string currentFilePath)
    {
        // Look for next archive in the current directory after the current file
        var archives = GetSortedArchivesInDirectory(currentDir);
        var idx = archives.FindIndex(a => a.FullName.Equals(currentFilePath, StringComparison.OrdinalIgnoreCase));
        if (!next)
        {
            return idx switch
            {
                > 0 => archives[idx - 1].FullName,
                < 0 when archives.Count > 0 => archives[^1].FullName,
                _ => GetLastArchiveInPreviousSiblingOrAncestor(currentDir)
            };
        }
        switch (idx)
        {
            case >= 0 when idx + 1 < archives.Count:
                return archives[idx + 1].FullName;
            case < 0 when archives.Count > 0:
                return archives[0].FullName;
        }
    
        if (!Settings.Sorting.IncludeSubDirectories)
        {
            return GetFirstArchiveInNextSiblingOrAncestor(currentDir);
        }
    
        var firstInChild = GetFirstArchiveInDescendants(currentDir);
        return firstInChild ?? GetFirstArchiveInNextSiblingOrAncestor(currentDir);
    }

    private string? GetFirstArchiveInDescendants(string path)
    {
        try
        {
            var dir = new DirectoryInfo(path);
            var subDirs = dir.GetDirectories().ToList();
            SortDirectories(subDirs);

            foreach (var sub in subDirs)
            {
                var archives = GetSortedArchivesInDirectory(sub.FullName);
                if (archives.Count > 0)
                {
                    return archives[0].FullName;
                }

                if (!Settings.Sorting.IncludeSubDirectories)
                {
                    continue;
                }

                var child = GetFirstArchiveInDescendants(sub.FullName);
                if (child != null)
                {
                    return child;
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(GetFirstArchiveInDescendants), ex);
        }
        return null;
    }

    private string? GetFirstArchiveInNextSiblingOrAncestor(string path)
    {
        var dir = new DirectoryInfo(path);
        var parent = dir.Parent;
        if (parent == null)
        {
            return null;
        }

        try
        {
            var siblings = parent.GetDirectories().ToList();
            SortDirectories(siblings);
            var index = siblings.FindIndex(d => d.FullName.Equals(path, StringComparison.OrdinalIgnoreCase));

            for (var i = index + 1; i < siblings.Count; i++)
            {
                var sibling = siblings[i];
                var archives = GetSortedArchivesInDirectory(sibling.FullName);
                if (archives.Count > 0)
                {
                    return archives[0].FullName;
                }

                if (!Settings.Sorting.IncludeSubDirectories)
                {
                    continue;
                }

                var child = GetFirstArchiveInDescendants(sibling.FullName);
                if (child != null)
                {
                    return child;
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(GetFirstArchiveInNextSiblingOrAncestor), ex);
        }

        return GetFirstArchiveInNextSiblingOrAncestor(parent.FullName);
    }

    private string? GetLastArchiveInPreviousSiblingOrAncestor(string currentPath)
    {
        var dir = new DirectoryInfo(currentPath);
        var parent = dir.Parent;
        if (parent is null)
        {
            return null;
        }

        try
        {
            var siblings = parent.GetDirectories().ToList();
            SortDirectories(siblings);
            var index = siblings.FindIndex(d => d.FullName.Equals(currentPath, StringComparison.OrdinalIgnoreCase));

            if (index <= 0)
            {
                var parentArchives = GetSortedArchivesInDirectory(parent.FullName);
                if (parentArchives.Count > 0)
                {
                    return parentArchives[^1].FullName;
                }
                return GetLastArchiveInPreviousSiblingOrAncestor(parent.FullName);
            }

            for (var i = index - 1; i >= 0; i--)
            {
                var sibling = siblings[i];
                var lastChild = GetLastArchiveInDescendantOrSelf(sibling.FullName);
                if (lastChild != null)
                {
                    return lastChild;
                }
            }

            var parentArchivesFallback = GetSortedArchivesInDirectory(parent.FullName);
            if (parentArchivesFallback.Count > 0)
            {
                return parentArchivesFallback[^1].FullName;
            }
            return GetLastArchiveInPreviousSiblingOrAncestor(parent.FullName);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(GetLastArchiveInPreviousSiblingOrAncestor), ex);
            return null;
        }
    }

    private string? GetLastArchiveInDescendantOrSelf(string path)
    {
        if (!Settings.Sorting.IncludeSubDirectories)
        {
            var archives = GetSortedArchivesInDirectory(path);
            return archives.Count > 0 ? archives[^1].FullName : null;
        }

        try
        {
            var dir = new DirectoryInfo(path);
            var subDirs = dir.GetDirectories().ToList();
            SortDirectories(subDirs);

            for (var i = subDirs.Count - 1; i >= 0; i--)
            {
                var lastChild = GetLastArchiveInDescendantOrSelf(subDirs[i].FullName);
                if (lastChild != null) return lastChild;
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(GetLastArchiveInDescendantOrSelf), ex);
        }

        var archivesHere = GetSortedArchivesInDirectory(path);
        return archivesHere.Count > 0 ? archivesHere[^1].FullName : null;
    }

    private string? FindNextValidDirectory(string currentPath)
    {
        if (!Settings.Sorting.IncludeSubDirectories)
        {
            return GetNextSiblingOrAncestorSibling(currentPath);
        }

        var firstChild = GetFirstValidChild(currentPath);
        return firstChild ?? GetNextSiblingOrAncestorSibling(currentPath);
    }

    private string? GetFirstValidChild(string path)
    {
        try
        {
            var dir = new DirectoryInfo(path);
            var subDirs = dir.GetDirectories().ToList();
            SortDirectories(subDirs);

            foreach (var sub in subDirs)
            {
                if (IsDirectoryValid(sub.FullName)) return sub.FullName;
                if (!Settings.Sorting.IncludeSubDirectories)
                {
                    continue;
                }

                var child = GetFirstValidChild(sub.FullName);
                if (child != null)
                {
                    return child;
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(GetFirstValidChild), ex);
        }
        return null;
    }

    private string? GetNextSiblingOrAncestorSibling(string path)
    {
        var dir = new DirectoryInfo(path);
        var parent = dir.Parent;
        if (parent == null)
        {
            return null;
        }

        try
        {
            var siblings = parent.GetDirectories().ToList();
            SortDirectories(siblings);
            var index = siblings.FindIndex(d => d.FullName.Equals(path, StringComparison.OrdinalIgnoreCase));

            for (var i = index + 1; i < siblings.Count; i++)
            {
                var sibling = siblings[i];
                if (IsDirectoryValid(sibling.FullName)) return sibling.FullName;
                if (!Settings.Sorting.IncludeSubDirectories)
                {
                    continue;
                }

                var child = GetFirstValidChild(sibling.FullName);
                if (child != null) return child;
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(GetNextSiblingOrAncestorSibling), ex);
        }

        return GetNextSiblingOrAncestorSibling(parent.FullName);
    }

    private string? FindPreviousValidDirectory(string currentPath)
    {
        var dir = new DirectoryInfo(currentPath);
        var parent = dir.Parent;
        if (parent is null)
        {
            return null;
        }

        try
        {
            var siblings = parent.GetDirectories().ToList();
            SortDirectories(siblings);
            var index = siblings.FindIndex(d => d.FullName.Equals(currentPath, StringComparison.OrdinalIgnoreCase));

            if (index <= 0)
            {
                return IsDirectoryValid(parent.FullName)
                    ? parent.FullName
                    : FindPreviousValidDirectory(parent.FullName);
            }

            for (var i = index - 1; i >= 0; i--)
            {
                var sibling = siblings[i];
                var lastChild = GetLastValidDescendantOrSelf(sibling.FullName);
                if (lastChild != null)
                {
                    return lastChild;
                }
            }

            return IsDirectoryValid(parent.FullName) ? parent.FullName : FindPreviousValidDirectory(parent.FullName);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(FindPreviousValidDirectory), ex);
            return null;
        }
    }

    private string? GetLastValidDescendantOrSelf(string path)
    {
        if (!Settings.Sorting.IncludeSubDirectories)
        {
            return IsDirectoryValid(path) ? path : null;
        }

        try
        {   
            var dir = new DirectoryInfo(path);
            var subDirs = dir.GetDirectories().ToList();
            SortDirectories(subDirs);

            for (var i = subDirs.Count - 1; i >= 0; i--)
            {
                var lastChild = GetLastValidDescendantOrSelf(subDirs[i].FullName);
                if (lastChild != null) return lastChild;
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(GetLastValidDescendantOrSelf), ex);
        }

        return IsDirectoryValid(path) ? path : null;
    }

    private static bool IsDirectoryValid(string path)
    {
        try
        {
            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                return false;
            }

            return Settings.Sorting.IncludeSubDirectories ?
                dir.EnumerateFiles("*", SearchOption.AllDirectories).Any(f => f.FullName.IsSupported()) :
                dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Any(f => f.FullName.IsSupported());
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(IsDirectoryValid), ex);
            return false;
        }
    }

    private void SortDirectories(List<DirectoryInfo> dirs)
    {
        if (!Settings.Sorting.Ascending)
        {
            dirs.Sort((x, y) => stringComparer(y.Name, x.Name));
        }
        else
        {
            dirs.Sort((x, y) => stringComparer(x.Name, y.Name));
        }
    }

    public async ValueTask SortAsync(TabViewModel tab, SortFilesBy sortOrder, CancellationTokenSource ct)
    {
        Settings.Sorting.SortPreference = (int)sortOrder;
        await ApplySortAsync(tab, ct).ConfigureAwait(false);
    }

    public async ValueTask SortAsync(TabViewModel tab, bool ascending, CancellationTokenSource ct)
    {
        Settings.Sorting.Ascending = ascending;
        await ApplySortAsync(tab, ct).ConfigureAwait(false);
    }

    private async ValueTask ApplySortAsync(TabViewModel tab, CancellationTokenSource ct)
    {
        if (!CanNavigate(tab))
        {
            return;
        }

        try
        {
            // Get current file to maintain position
            var currentFile = tab.Model?.FileInfo;
            if (currentFile is null)
            {
                return;
            }

            // Retrieve and sort files based on new settings
            var newFiles = await Task.Run(() => FileListRetriever.RetrieveFiles(currentFile, stringComparer), ct.Token).ConfigureAwait(false);

            if (newFiles.Count == 0)
            {
                return;
            }

            // Update files in iterator
            tab.ImageIterator.Files = newFiles;

            // Find new index of current file
            var newIndex = FindIndex(currentFile, tab);
            tab.ImageIterator.SetCurrentIndex(newIndex);
            
            // Update cache mapping
            cache.Resynchronize(tab.Id, newFiles);
            
            // Update title
            tab.UpdateTabTitle();
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(NavigationService), nameof(ApplySortAsync), e);
        }
    }

    private static int FindIndex(FileInfo fileInfo, TabViewModel tab) =>
        tab.ImageIterator.Files.FindIndex(x =>
            x.FullName.AsSpan().Equals(fileInfo.FullName.AsSpan(), StringComparison.OrdinalIgnoreCase));
}