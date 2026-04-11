using Avalonia.Media.Imaging;
using Avalonia.Threading;
using PicView.Avalonia.Linux.Views;
using PicView.Core.Config;
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

        if (vm.PrintPreview.PrintWindowConfig is null)
        {
            vm.PrintPreview.PrintWindowConfig = new PrintWindowConfig();
            await vm.PrintPreview.PrintWindowConfig.LoadAsync();
        }

        var configProps = vm.PrintPreview.PrintWindowConfig.PrintProperties;

        // 1. Printers via CUPS
        var printers = LinuxPrint.GetAvailablePrinters().ToList(); // includes "Save as PDF" first
        vm.PrintPreview.Printers.Value = printers;

        var defaultPrinter = printers.FirstOrDefault() ?? string.Empty;
        var configPrinter = configProps?.PrinterName;
        if (!string.IsNullOrWhiteSpace(configPrinter) && printers.Contains(configPrinter))
        {
            defaultPrinter = configPrinter;
        }

        // 2. Paper sizes - from printer or fallback
        // vm.PrintPreview.PaperSizes.Value =
        //     CupsPaperQuery.GetPaperSizes(defaultPrinter).ToList();
        
        var defaultPaperSize = "A4";
        if (!string.IsNullOrWhiteSpace(configProps?.PaperSize))
        {
            defaultPaperSize = configProps.PaperSize;
        }

        // 3. Build initial PrintSettings
        var currentPrintSettings = new PrintSettings
        {
            ImagePath = { Value = path },
            PrinterName = { Value = defaultPrinter },
            PaperSize = { Value = defaultPaperSize },
            ColorMode = { Value = configProps?.ColorMode ?? (int)ColorModes.Auto },
            Orientation = { Value = configProps?.Orientation ?? (int)Orientations.Portrait },
            ScaleMode = { Value = configProps?.ScaleMode ?? (int)ScaleModes.Fit },
            Copies = { Value = configProps?.Copies ?? 1 },
            MarginTop = { Value = configProps?.MarginTop ?? 10 },     // mm
            MarginBottom = { Value = configProps?.MarginBottom ?? 10 },
            MarginLeft = { Value = configProps?.MarginLeft ?? 10 },
            MarginRight = { Value = configProps?.MarginRight ?? 10 },
        };

        vm.PrintPreview.PrintSettings.Value = currentPrintSettings;

        await Dispatcher.UIThread.InvokeAsync(() => printPreviewWindow.Initialize());
    }
}