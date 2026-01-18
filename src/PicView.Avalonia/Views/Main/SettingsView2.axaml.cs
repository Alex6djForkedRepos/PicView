using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PicView.Avalonia.UI;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Views.Main;

public partial class SettingsView2 : UserControl
{
    private const string SearchDim = "searchDim";
    private const string SearchMatch = "searchMatch";

    private bool _isScrollingProgrammatically;
    private bool _isUpdatingFromSpy;
    private Dictionary<SettingsCategory, Control>? _sections;
    private IDisposable? _subscription;

    public SettingsView2()
    {
        InitializeComponent();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            //FileAssociationsTabItem.IsEnabled = false;
        }

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        FilterBox.TextChanged += OnSearchTextChanged;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        InitializeSectionsMap();
        CategoriesListBox.SelectionChanged += OnListBoxSelectionChanged;
        ContentScrollViewer.ScrollChanged += OnScrollChanged;

        KeyDown += OnKeyDown;

        if (DataContext is not CoreViewModel core)
        {
            return;
        }

        _subscription = core.SettingsViewModel.SelectedCategory.Subscribe(OnViewModelCategoryChanged);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not CoreViewModel core)
        {
            return;
        }

        var isSearchFocused = FilterBox.IsFocused || FilterBox.IsKeyboardFocusWithin;

        if (e.Key is Key.Escape)
        {
            if (isSearchFocused)
            {
                if (FilterBox.Text.Length <= 0)
                {
                    // Switch away focus
                    ContentScrollViewer.Focus();
                }
                else
                {
                    FilterBox.Clear();
                }

                e.Handled = true;
                return;
            }
        }

        var isCtrl = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
            ? e.KeyModifiers is KeyModifiers.Meta
            : e.KeyModifiers is KeyModifiers.Control;

        if ((e.Key is Key.F && isCtrl) || (!isSearchFocused && e.Key is Key.OemQuestion))
        {
            FilterBox.Focus();
            e.Handled = true;
            return;
        }

        if (core.SettingsViewModel.IsOverviewVisible.CurrentValue)
        {
            // TODO: Use arrow keys to navigate overview categories
            return;
        }

        switch (e.Key)
        {
            case Key.Down:
            case Key.PageDown:
                ContentScrollViewer.LineDown();
                break;
            case Key.Up:
            case Key.PageUp:
                ContentScrollViewer.LineUp();
                break;
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        var searchText = FilterBox.Text;

        // Logic: 
        // Always keep the main view visible. 
        // Just run the styling pass.

        // Ensure we are in the "Split View" mode (where settings are visible), 
        // because searching in the "Overview" (grid of big buttons) might be confusing
        // if we are highlighting individual controls inside the categories.
        if (DataContext is CoreViewModel core && !string.IsNullOrWhiteSpace(searchText))
        {
            // Force switch to Split View so we can see the controls we are searching
            if (core.SettingsViewModel.IsOverviewVisible.Value)
            {
                core.SettingsViewModel.IsOverviewVisible.Value = false;
            }
        }

        PerformSearch(searchText);

        // 2. Scroll to the best match
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // We invoke this on the next UI tick to ensure Layout/Bounds are updated 
            // if the view just switched from Overview to SplitView
            Dispatcher.UIThread.Post(ScrollToNearestMatch, DispatcherPriority.Input);
        }
    }

    private void PerformSearch(string query)
    {
        if (_sections == null)
        {
            return;
        }

        // 1. CLEAR: If query is empty, remove all search classes
        if (string.IsNullOrWhiteSpace(query))
        {
            foreach (var child in _sections.Values
                         .Select(sectionControl => sectionControl.GetVisualDescendants().OfType<Border>())
                         .SelectMany(allControls => allControls))
            {
                child.Classes.Remove(SearchMatch);
                child.Classes.Remove(SearchDim);
            }

            return;
        }

        // 2. SEARCH: Loop through all controls
        foreach (var sectionControl in _sections.Values)
        {
            var children = sectionControl.GetVisualDescendants().OfType<Control>();

            foreach (var child in children)
            {
                // Skip controls that aren't marked for search
                if (child.Tag == null && SearchProperties.GetKeywords(child) == null)
                {
                    continue;
                }

                // Determine if it matches
                var isMatch = false;

                // Get keywords (Attached Property)
                var keywordsString = SearchProperties.GetKeywords(child);

                // Fallback to Tag if Keywords are missing
                if (string.IsNullOrEmpty(keywordsString) && child.Tag is string tag)
                {
                    keywordsString = tag;
                }

                if (!string.IsNullOrEmpty(keywordsString))
                {
                    var keywords = keywordsString.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
                    isMatch = keywords.Any(k => k.Contains(query, StringComparison.OrdinalIgnoreCase));
                }

                // 3. APPLY CLASSES
                if (isMatch)
                {
                    child.Classes.Remove(SearchDim);
                    child.Classes.Add(SearchMatch);
                }
                else
                {
                    child.Classes.Remove(SearchMatch);
                    child.Classes.Add(SearchDim);
                }
            }
        }
    }

    private void ScrollToNearestMatch()
    {
        if (_sections == null)
        {
            return;
        }

        Control? bestMatch = null;
        var bestDistance = double.MaxValue;

        // 1. Iterate through sections in visual order (Top to Bottom)
        var sortedSections = _sections.OrderBy(x => x.Value.Bounds.Y);

        foreach (var sectionKvp in sortedSections)
        {
            var section = sectionKvp.Value;

            // Find all controls in this section that are highlighted matches
            var matches = section.GetVisualDescendants()
                .OfType<Control>()
                .Where(c => c.Classes.Contains("searchMatch"));

            foreach (var match in matches)
            {
                // 2. Calculate position relative to the ScrollViewer's visible viewport
                // A negative Y means it's above the viewport (scrolled past).
                // A positive Y means it's inside or below the viewport.
                var relativePoint = match.TranslatePoint(new Point(0, 0), ContentScrollViewer);

                if (relativePoint.HasValue)
                {
                    var y = relativePoint.Value.Y;

                    // 3. Check if this is the "closest downwards" match
                    // We use -10 as a small tolerance so we catch items partially cut off at the top
                    if (y >= -10 && y < bestDistance)
                    {
                        bestDistance = y;
                        bestMatch = match;
                    }
                }
            }
        }

        // 4. Scroll to the match if found
        if (bestMatch != null)
        {
            // Add current offset to the relative distance to get absolute position
            // Subtract 50 for a nice visual padding (margin) at the top
            var targetOffset = ContentScrollViewer.Offset.Y + bestDistance - 50;

            // Ensure we don't scroll past the limits
            targetOffset = Math.Max(0, targetOffset);

            // Programmatic scroll
            _isScrollingProgrammatically = true;
            try
            {
                ContentScrollViewer.Offset = new Vector(0, targetOffset);
            }
            finally
            {
                // Reset flag after a short delay or immediately depending on preference
                // (Using the existing pattern from your file)
                Dispatcher.UIThread.Post(() => _isScrollingProgrammatically = false, DispatcherPriority.Input);
            }
        }
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        CategoriesListBox.SelectionChanged -= OnListBoxSelectionChanged;
        ContentScrollViewer.ScrollChanged -= OnScrollChanged;
        _subscription?.Dispose();
    }

    private void InitializeSectionsMap()
    {
        _sections = new Dictionary<SettingsCategory, Control>();
        foreach (var category in Enum.GetValues<SettingsCategory>())
        {
            var sectionName = category + "Section";
            var control = this.FindControl<Control>(sectionName);
            if (control != null)
            {
                _sections[category] = control;
            }
        }
    }

    private void OnViewModelCategoryChanged(SettingsCategory category)
    {
        // Update ListBox selection
        var item = CategoriesListBox.Items.OfType<ListBoxItem>()
            .FirstOrDefault(x => x.Tag is SettingsCategory cat && cat == category);

        if (!ReferenceEquals(CategoriesListBox.SelectedItem, item))
        {
            CategoriesListBox.SelectedItem = item;
        }

        if (!_isUpdatingFromSpy)
        {
            // Scroll to section
            ScrollToCategory(category);
        }
    }

    private void OnListBoxSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (CategoriesListBox.SelectedItem is not ListBoxItem { Tag: SettingsCategory category })
        {
            return;
        }

        if (DataContext is not CoreViewModel core)
        {
            return;
        }

        if (core.SettingsViewModel != null && core.SettingsViewModel.SelectedCategory.Value != category)
        {
            core.SettingsViewModel.SelectedCategory.Value = category;
        }
    }

    private void ScrollToCategory(SettingsCategory category)
    {
        if (_sections == null || !_sections.TryGetValue(category, out var section))
        {
            return;
        }

        _isScrollingProgrammatically = true;
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                if (section.Bounds.Y != 0 || section == _sections[SettingsCategory.General])
                {
                    ContentScrollViewer.Offset = new Vector(0, section.Bounds.Y);
                }
            }
            finally
            {
                // Delay flag reset slightly to ensure scroll events from this action are ignored
                Dispatcher.UIThread.Post(() => _isScrollingProgrammatically = false, DispatcherPriority.Input);
            }
        }, DispatcherPriority.Loaded);
    }

    private void OnScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_isScrollingProgrammatically)
        {
            return;
        }

        if (_sections == null)
        {
            return;
        }

        var offset = ContentScrollViewer.Offset.Y;

        SettingsCategory? bestMatch = null;

        // Sort sections by their visual Y position.
        // This ensures we iterate from top to bottom, so the "last match" 
        // is always the deepest section that satisfies the condition.
        var sortedSections = _sections.OrderBy(x => x.Value.Bounds.Y);

        foreach (var kvp in sortedSections)
        {
            if (kvp.Value.Bounds.Y <= offset)
            {
                bestMatch = kvp.Key;
            }
        }

        // If no section is above offset (e.g. at very top), default to General
        if (!bestMatch.HasValue && _sections.Count > 0)
        {
            bestMatch = SettingsCategory.General;
        }

        if (DataContext is not CoreViewModel core)
        {
            return;
        }

        if (!bestMatch.HasValue || core?.SettingsViewModel == null ||
            core.SettingsViewModel.SelectedCategory.Value == bestMatch.Value)
        {
            return;
        }

        _isUpdatingFromSpy = true;
        try
        {
            core.SettingsViewModel.SelectedCategory.Value = bestMatch.Value;
        }
        finally
        {
            _isUpdatingFromSpy = false;
        }
    }
}