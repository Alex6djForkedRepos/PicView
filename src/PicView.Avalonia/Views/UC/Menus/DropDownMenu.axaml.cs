using Avalonia.Interactivity;
using System;
using Avalonia.LogicalTree;
using PicView.Avalonia.CustomControls;
using PicView.Avalonia.UI.FileHistory2;
using PicView.Core.Localization;
using R3;
using MainWindowViewModel = PicView.Core.ViewModels.MainWindowViewModel;

namespace PicView.Avalonia.Views.UC.Menus;

public partial class DropDownMenu : AnimatedMenu
{
    private IDisposable? _menuVisibilitySubscription;
    private FileHistoryMenuController? _fileHistoryMenuController;

    public DropDownMenu()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        SlideShow2Sec.Text = $"2 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow5Sec.Text = $"5 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow10Sec.Text = $"10 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow20Sec.Text = $"20 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow30Sec.Text = $"30 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow60Sec.Text = $"60 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow90Sec.Text = $"90 {TranslationManager.Translation.SecAbbreviation}";
        SlideShow120Sec.Text = $"120 {TranslationManager.Translation.SecAbbreviation}";

        if (DataContext is MainWindowViewModel vm)
        {
            _fileHistoryMenuController = new FileHistoryMenuController(
                FileHistoryContainer, 
                HistorySortButton, 
                HistoryClearButton, 
                HistoryFileButton, 
                vm);

            _menuVisibilitySubscription = vm.TopTitlebarViewModel.DropDownMenu.IsDropDownMenuVisible
                .Subscribe(isVisible =>
                {
                    if (isVisible)
                    {
                        _fileHistoryMenuController?.UpdateFileHistoryMenu();
                    }
                });
        }
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromLogicalTree(e);
        Loaded -= OnLoaded;
        _menuVisibilitySubscription?.Dispose();
        _fileHistoryMenuController?.Dispose();
        _fileHistoryMenuController = null;
    }

    private void Close_OnClick(object? sender, RoutedEventArgs e)
    {
        // Trigger closing animation
        IsOpen = false;
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        // Let view model know it is closed
        vm.TopTitlebarViewModel.DropDownMenu.IsDropDownMenuVisible.Value = false;
    }
}