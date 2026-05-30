using Avalonia;
using Avalonia.Threading;
using PicView.Avalonia.UI;
using PicView.Avalonia.Views.UC.PopUps;
using PicView.Core.DebugTools;
using PicView.Core.IPlatform;
using PicView.Core.Localization;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.FileSystem;

public static class FileManager
{
    /// <summary>
    ///     Deletes the current file, either permanently or by moving to recycle bin
    /// </summary>
    public static async ValueTask DeleteFileWithOptionalDialog(bool recycle, string path, IPlatformSpecificService platformService)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            if (recycle && Settings.UIProperties.ShowRecycleConfirmation ||
                !recycle && Settings.UIProperties.ShowPermanentDeletionConfirmation)
            {
               await Dispatcher.UIThread.InvokeAsync(ShowDeleteDialog);
            }
            else
            {
                var success = await platformService.DeleteFile(path, recycle);

                if (success)
                {
                    var msg = recycle
                        ? TranslationManager.Translation.SentFileToRecycleBin
                        : TranslationManager.Translation.DeletedFile;
                    TooltipHelper.ShowTooltipMessage(msg + Environment.NewLine + Path.GetFileName(path));
                }
                else if (File.Exists(path))
                {
                    TooltipHelper.ShowTooltipMessage(TranslationManager.Translation.UnexpectedError, true);
                }
            }
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(FileManager), nameof(DeleteFileWithOptionalDialog), ex);
        }

        return;

        void ShowDeleteDialog()
        {
            var prompt = recycle
                ? TranslationManager.Translation.DeleteFile
                : TranslationManager.Translation.DeleteFilePermanently;
            var deleteDialog = new DeleteDialog(prompt, path, recycle);
            UIHelper.GetMainView.MainPanel.Children.Add(deleteDialog);
            // Dialog handles the deletion
        }
    }

    /// <summary>
    ///     Shows properties dialog for the specified file
    /// </summary>
    public static void ShowFileProperties(string path)
    {
        CoreViewModel? core = Dispatcher.UIThread.Invoke(() => Application.Current.DataContext as CoreViewModel);
        if (core is null)
        {
            return;
        }
        try
        {
            core.PlatformService.ShowFileProperties(path);
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(FileManager), nameof(ShowFileProperties), ex);
        }
    }

    /// <summary>
    ///     Opens the file location in file explorer
    /// </summary>
    public static async Task LocateOnDisk(string path)
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }

        try
        {
            await Task.Run(() => core.PlatformService!.LocateOnDisk(path));
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(FileManager), nameof(LocateOnDisk), ex);
        }
    }

    /// <summary>
    ///     Shows the dialog to open the file with another application
    /// </summary>
    public static async Task OpenWith(string path)
    {
        if (Application.Current.DataContext is not CoreViewModel core)
        {
            return;
        }
        try
        {
            await Task.Run(() => core.PlatformService!.OpenWith(path));
        }
        catch (Exception ex)
        {
            DebugHelper.LogDebug(nameof(FileManager), nameof(OpenWith), ex);
        }
    }
}