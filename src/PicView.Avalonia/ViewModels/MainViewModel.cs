using PicView.Avalonia.Functions;
using PicView.Avalonia.Interfaces;
using PicView.Core.ViewModels;
using ImageViewer = PicView.Avalonia.Views.UC.ImageViewer;

namespace PicView.Avalonia.ViewModels;

// TODO deprecated, delete
public class MainViewModel
{
    public readonly IPlatformSpecificService? PlatformService;
    public readonly IPlatformWindowService? PlatformWindowService;
    
    public TranslationViewModel Translation { get; } = new();
    public ToolTipViewModel? ToolTip { get; set; }
    public GlobalSettingsViewModel GlobalSettings { get; } = new();
    public SettingsViewModel? SettingsViewModel { get; set; }
    public PicViewerModel PicViewer { get; } = new();
    public HoverbarViewModel HoverbarViewModel { get; } = new();
    public GalleryViewModel Gallery { get; } = new();
    public ExifViewModel? Exif { get; set; }
    public ImageInfoWindowViewModel? InfoWindow { get; set; }
    public FileAssociationsViewModel? AssociationsViewModel { get; set; }
    public AboutViewModel? AboutView { get; set; }
    public PrintPreviewViewModel? PrintPreview { get; set; }
    public BatchResizeViewModel? BatchResizeViewModel { get; set; }
    public KeybindingsViewModel? Keybindings { get; set; }

    public MainViewModel(IPlatformSpecificService? platformSpecificService, IPlatformWindowService? platformWindowService)
    {
        FunctionsMapper.Vm = this;
        PlatformService = platformSpecificService;
        PlatformWindowService = platformWindowService;
    }

    public MainViewModel()
    {
        // Only use for unit test
    }
    
    // TODO should remove this and work towards moving MainViewModel to Core project
    public ImageViewer? ImageViewer;
    
}