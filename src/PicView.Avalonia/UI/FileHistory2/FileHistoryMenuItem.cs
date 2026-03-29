using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using PicView.Avalonia.Views.UC.Buttons;
using PicView.Core.ArchiveHandling;
using PicView.Core.Extensions;
using PicView.Core.FileHistory;
using PicView.Core.ViewModels;
using System;
using System.IO;
using Avalonia.Controls.ApplicationLifetimes;

namespace PicView.Avalonia.UI.FileHistory2
{
    /// <summary>
    ///     Represents a single file history menu item with pin/unpin functionality
    /// </summary>
    public class FileHistoryMenuItem : Panel
    {
        private const int MaxFilenameLength = 42;
        private readonly FileHistoryMenuBuilder _builder;

        public FileHistoryMenuItem(Entry entry, string? currentFilePath, MainWindowViewModel viewModel, int index, FileHistoryMenuBuilder builder)
        {
            _builder = builder;
            var fileLocation = entry.Path;
            if (string.IsNullOrEmpty(fileLocation))
            {
                return;
            }

            bool isSelected;
            if (ArchiveExtraction.IsArchived)
            {
                isSelected = fileLocation == ArchiveExtraction.LastOpenedArchive;
            }
            else
            {
                isSelected = fileLocation == currentFilePath;
            }
            
            var filename = Path.GetFileName(fileLocation);
            var header = filename.Length > MaxFilenameLength ? filename.Shorten(MaxFilenameLength) : filename;

            // Create the pin button with appropriate visibility
            var pinButton = CreatePinButton(entry, fileLocation);

            // Create the menu item button with file info
            var menuItemButton = CreateMenuItemButton(header, fileLocation, isSelected, index, viewModel);

            // Add components to the panel
            Children.Add(menuItemButton);
            Children.Add(pinButton);

            // Add hover behavior
            ConfigureHoverBehavior(pinButton);

            // Set tooltip
            ToolTip.SetTip(menuItemButton, fileLocation);
        }

        private PinButton CreatePinButton(Entry entry, string fileLocation)
        {
            var pinBtn = new PinButton
            {
                Opacity = 0,
                Width = 25,
                HorizontalAlignment = HorizontalAlignment.Right,
                ZIndex = 1
            };

            // Toggle just this button's visibility instead of rebuilding the whole menu
            pinBtn.PinBtn.Click += (_, _) => 
            {
                FileHistoryManager.Pin(fileLocation);
                // Just update this button's visibility
                pinBtn.PinBtn.IsVisible = false;
                pinBtn.UnPinBtn.IsVisible = true;
        
                // Schedule a deferred menu refresh to happen after this event has completed
                Dispatcher.UIThread.Post(() =>
                {
                    _builder.BuildMenu();
                });
            };
    
            pinBtn.UnPinBtn.Click += (_, _) => 
            {
                FileHistoryManager.UnPin(fileLocation);
                // Just update this button's visibility
                pinBtn.PinBtn.IsVisible = true;
                pinBtn.UnPinBtn.IsVisible = false;
        
                // Schedule a deferred menu refresh to happen after this event has completed
                Dispatcher.UIThread.Post(() =>
                {
                    _builder.BuildMenu();
                });
            };

            if (entry.IsPinned)
            {
                pinBtn.PinBtn.IsVisible = false;
                pinBtn.UnPinBtn.IsVisible = true;
            }
            else
            {
                pinBtn.PinBtn.IsVisible = true;
                pinBtn.UnPinBtn.IsVisible = false;
            }

            return pinBtn;
        }

        private static Button CreateMenuItemButton(string header, string fileLocation, bool isSelected, int index,
            MainWindowViewModel viewModel)
        {
            var item = new Button
            {
                Background = Brushes.Transparent,
                Padding = new Thickness(5, 6),
                Width = 355
            };
            
            TextBlock? indexText = null;
            TextBlock? headerText = null;

            if (index < 0)
            {
                // Pinned item without index number
                item.Padding = new Thickness(15, 0, 0, 0);
                headerText = new TextBlock
                {
                    Classes = { "txt" },
                    Text = header,
                    Padding = new Thickness(5, 5, 0, 5)
                };
            }
            else
            {
                // Regular item with index number
                indexText = new TextBlock
                {
                    Classes = { "txt" },
                    Text = (index + 1).ToString(),
                    Padding = new Thickness(5, 0, 2, 0)
                };

                headerText = new TextBlock
                {
                    Classes = { "txt" },
                    Text = header,
                    Padding = new Thickness(5, 0, 0, 0)
                };
            }
            
            if (!Settings.Theme.Dark)
            {
                if (Application.Current?.TryGetResource("MainTextColor",
                        Application.Current.RequestedThemeVariant, out var mainTextColor) != true ||
                    Application.Current.TryGetResource("SecondaryTextColor", Application.Current.RequestedThemeVariant, out var secondaryTextColor) != true)
                {
                    throw new InvalidOperationException();
                }

                if (mainTextColor is not Color color || secondaryTextColor is not Color secondaryColor)
                {
                    throw new InvalidOperationException();
                }

                var brush = new SolidColorBrush(color);
                var secondaryBrush = new SolidColorBrush(secondaryColor);
                if (indexText is not null)
                {
                    indexText.Foreground = brush;
                }
                headerText.Foreground = brush;

                item.PointerEntered += delegate
                {
                    if (indexText is not null)
                    {
                        indexText.Foreground = secondaryBrush;
                    }
                    headerText.Foreground = secondaryBrush;
                };
                item.PointerExited += delegate
                {
                    if (indexText is not null)
                    {
                        indexText.Foreground = brush;
                    }
                    headerText.Foreground = brush;
                };
            }

            if (index < 0)
            {
                item.Content = headerText;
            }
            else
            {
                item.Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children = { indexText, headerText }
                };
            }

            if (isSelected)
            {
                item.Classes.Add("active");
            }

            item.Click += async delegate
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
                {
                    // Let the DropDownMenu close
                    viewModel.TopTitlebarViewModel.DropDownMenu.IsDropDownMenuVisible.Value = false;
                }
                
                // Using TabOverviewViewModel LoadFromStringAsync
                await viewModel.WindowTabs.LoadFromStringAsync(fileLocation).ConfigureAwait(false);
            };

            return item;
        }

        private void ConfigureHoverBehavior(PinButton pinBtn)
        {
            PointerEntered += (_, _) =>
            {
                pinBtn.Opacity = 1;
                if (Application.Current?.TryGetResource("AccentColor", Application.Current.RequestedThemeVariant,
                        out var accentColor) == true)
                {
                    Background = accentColor as SolidColorBrush;
                }
            };

            PointerExited += (_, _) =>
            {
                pinBtn.Opacity = 0;
                Background = Brushes.Transparent;
            };
        }
    }
}