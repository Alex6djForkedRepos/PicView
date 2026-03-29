using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PicView.Core.FileHistory;
using PicView.Core.Localization;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.UI.FileHistory2;

/// <summary>
///     Responsible for building the file history menu UI elements
/// </summary>
public class FileHistoryMenuBuilder(Panel menuContainer, MainWindowViewModel viewModel, bool isDescendingSorting)
{
    private readonly string? _currentFilePath = viewModel.WindowTabs.ActiveTab.Value?.Model.FileInfo?.FullName;

    /// <summary>
    ///     Builds the entire history menu with pinned and unpinned sections
    /// </summary>
    public void BuildMenu()
    {
        menuContainer.Children.Clear();

        var entries = FileHistoryManager.AllEntries;

        var pinnedEntries = entries.Where(e => e.IsPinned).ToList();
        var unpinnedEntries = entries.Where(e => !e.IsPinned).ToList();

        // Add pinned entries section
        BuildPinnedSection(pinnedEntries);

        // Add unpinned entries section
        BuildUnpinnedSection(unpinnedEntries);
    }

    private void BuildPinnedSection(List<Entry> pinnedEntries)
    {
        if (pinnedEntries.Count == 0)
        {
            return;
        }

        // Create pinned section header
        var pinnedHeader = new TextBlock
        {
            Text = TranslationManager.Translation.Pinned,
            Margin = new Thickness(20, 5, 0, 5),
            FontFamily = new FontFamily("avares://PicView.Avalonia/Assets/Fonts/Roboto-Bold.ttf#Roboto"),
            Classes = { "txt" }
        };
        if (!Settings.Theme.Dark)
        {
            if (Application.Current?.TryGetResource("MainTextColor",
                    Application.Current.RequestedThemeVariant, out var mainTextColor) != true)
            {
                throw new InvalidOperationException();
            }

            if (mainTextColor is not Color color)
            {
                throw new InvalidOperationException();
            }

            var brush = new SolidColorBrush(color);
            pinnedHeader.Foreground = brush;
        }

        menuContainer.Children.Add(pinnedHeader);

        // Add pinned entries
        // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var entry in pinnedEntries)
        {
            var menuItem = new FileHistoryMenuItem(entry, _currentFilePath, viewModel, -1, this);
            menuContainer.Children.Add(menuItem);
        }

        // Add separator between pinned and unpinned sections
        var separator = new Separator
        {
            Margin = new Thickness(10, 7, 20, 7)
        };
        menuContainer.Children.Add(separator);
    }

    private void BuildUnpinnedSection(List<Entry> unpinnedEntries)
    {
        if (unpinnedEntries.Count == 0)
        {
            return;
        }

        var max = unpinnedEntries.Count;

        if (isDescendingSorting)
        {
            for (var i = 0; i < max; i++)
            {
                var menuItem = new FileHistoryMenuItem(unpinnedEntries[i], _currentFilePath, viewModel, i, this);
                menuContainer.Children.Add(menuItem);
            }
        }
        else
        {
            for (var i = max - 1; i >= 0; i--)
            {
                var menuItem = new FileHistoryMenuItem(unpinnedEntries[i], _currentFilePath, viewModel, i, this);
                menuContainer.Children.Add(menuItem);
            }
        }
    }
}