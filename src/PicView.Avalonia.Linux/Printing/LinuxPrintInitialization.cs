using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.Linux.Views;
using PicView.Core.Linux.Printing;
using PicView.Core.Printing;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.Linux.Printing;

public static class LinuxPrintInitialization
{
    public static async Task Initialize(MainWindowViewModel vm, string path, PrintPreviewWindow printPreviewWindow)
    {
        if (vm.WindowTabs.ActiveTab.CurrentValue.Model?.CurrentValue?.Image != null && File.Exists(path))
        {
            await using var fs = File.OpenRead(path);
            vm.PrintPreview.PreviewImage.Value = new Bitmap(fs);
            // Prefill page sizes to avoid excessive resize
            vm.PrintPreview.PageWidth.Value = 650;
            vm.PrintPreview.PageHeight.Value = 950;
        }

        // 1. Printers via CUPS
        var printers = LinuxPrint.GetAvailablePrinters().ToList(); // includes "Save as PDF" first
        vm.PrintPreview.Printers.Value = printers;

        var defaultPrinter = printers.FirstOrDefault() ?? string.Empty;

        // 2. Paper sizes - from printer or fallback
        // vm.PrintPreview.PaperSizes.Value =
        //     CupsPaperQuery.GetPaperSizes(defaultPrinter).ToList();

        // 3. Build initial PrintSettings
        var currentPrintSettings = new PrintSettings
        {
            ImagePath = { Value = path },
            PrinterName = { Value = defaultPrinter },
            PaperSize = { Value = "A4" },
            ColorMode = { Value = (int)ColorModes.Auto },
            Orientation = { Value = (int)Orientations.Portrait },
            MarginTop = { Value = 10 },     // mm
            MarginBottom = { Value = 10 },
            MarginLeft = { Value = 10 },
            MarginRight = { Value = 10 },
        };

        vm.PrintPreview.PrintSettings.Value = currentPrintSettings;

        await Dispatcher.UIThread.InvokeAsync(() => printPreviewWindow.Initialize());
    }
}