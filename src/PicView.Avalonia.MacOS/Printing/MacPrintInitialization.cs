using PicView.Avalonia.MacOS.Views;
using PicView.Core.Config;
using PicView.Core.MacOS.Printing;
using PicView.Core.Printing;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.MacOS.Printing;

public static class MacPrintInitialization
{
    public static async Task Initialize(MainWindowViewModel vm, string path, PrintPreviewWindow printPreviewWindow)
    {
        if (vm.PrintPreview.PrintWindowConfig is null)
        {
            vm.PrintPreview.PrintWindowConfig = new PrintWindowConfig();
            await vm.PrintPreview.PrintWindowConfig.LoadAsync();
        }

        var configProps = vm.PrintPreview.PrintWindowConfig.PrintProperties;
        
        // 1. Printers via CUPS
        var printers = MacOSPrint.GetAvailablePrinters().ToList(); // includes "Save as PDF" first
        vm.PrintPreview.Printers.Value = printers;

        var defaultPrinter = printers.FirstOrDefault() ?? string.Empty;
        
        var configPrinter = configProps?.PrinterName;
        if (!string.IsNullOrWhiteSpace(configPrinter) && printers.Contains(configPrinter))
        {
            defaultPrinter = configPrinter;
        }

        // 2. Paper sizes - from printer or fallback
        var paperSizes = CupsPaperQuery.GetPaperSizes(defaultPrinter).ToList();
        vm.PrintPreview.PaperSizes.Value = paperSizes;

        var defaultPaperSize = "A4";
        var configPaperSize = configProps?.PaperSize;
        if (!string.IsNullOrWhiteSpace(configPaperSize) && paperSizes.Contains(configPaperSize))
        {
            defaultPaperSize = configPaperSize;
        }
        else if (paperSizes.Any())
        {
            defaultPaperSize = paperSizes.First();
        }
        
        // Allow every format that is viewable to also be printed, or just make sure the image effect stays applied on print
        // var commonSupportedFormat = await ImageFormatConverter.ConvertToCommonSupportedFormatAsync(path, vm)
        //     .ConfigureAwait(false);

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

        printPreviewWindow.Initialize();
    }
}