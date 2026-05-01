using Avalonia.Controls;
using PicView.Core.ViewModels;

namespace PicView.Avalonia.CustomControls;

public class PrintWindow: GenericWindow
{
    protected const float PreviewDpi = 96f;
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        if (vm.PrintPreview == null)
        {
            return;
        }

        var ps = vm.PrintPreview.PrintSettings.Value;
        var config = vm.PrintPreview.PrintWindowConfig;
        if (ps != null && config is { PrintProperties: not null })
        {
            var props = config.PrintProperties;
            props.PrinterName = ps.PrinterName.Value;
            props.PaperSize = ps.PaperSize.Value;
            props.Orientation = ps.Orientation.Value;
            props.ScaleMode = ps.ScaleMode.Value;
            props.ColorMode = ps.ColorMode.Value;
            props.Copies = ps.Copies.Value;
            props.MarginTop = ps.MarginTop.Value;
            props.MarginBottom = ps.MarginBottom.Value;
            props.MarginLeft = ps.MarginLeft.Value;
            props.MarginRight = ps.MarginRight.Value;

            _ = config.SaveAsync();
        }

        vm.PrintPreview.Dispose();
    }
}