using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using PicView.Core.ViewModels;
using R3;
using System.Runtime.InteropServices;
using Avalonia.Input;

namespace PicView.Avalonia.Views.Main;

public partial class SettingsView2 : UserControl
{
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
        if (_isScrollingProgrammatically) return;

        if (_sections == null) return;

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