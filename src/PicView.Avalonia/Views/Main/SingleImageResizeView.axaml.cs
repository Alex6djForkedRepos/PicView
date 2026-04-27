using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ImageMagick;
using PicView.Avalonia.FileSystem;
using PicView.Avalonia.Resizing;
using PicView.Avalonia.UI;
using PicView.Core.DebugTools;
using PicView.Core.ImageDecoding;
using PicView.Core.ViewModels;
using R3;

namespace PicView.Avalonia.Views.Main;

public partial class SingleImageResizeView : UserControl
{
    private double _aspectRatio;
    private readonly CompositeDisposable _imageUpdateSubscription = new();
    private bool _isKeepingAspectRatio = true;

    public SingleImageResizeView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        
        if (!Settings.Theme.Dark || Settings.Theme.GlassTheme)
        {
            BgPanel.Background = Brushes.Transparent;
        }
        if (!Settings.Theme.Dark)
        {
            var topBg = new SolidColorBrush(Color.FromArgb(a: 65, r: 162, g: 162, b: 162));
            var bottomBg = new SolidColorBrush(Color.FromArgb(a: 93, r: 162, g: 162, b: 162));
            MainBorder.Background = topBg;
            BottomBorder.Background = bottomBg;

            var noThickness = new Thickness(0);
            PixelWidthTextBox.BorderThickness = noThickness;
            PixelHeightTextBox.BorderThickness = noThickness;
            
            if (TryGetResource("CancelBrush",
                    Application.Current.RequestedThemeVariant, out var cBrush))
            {
                if (cBrush is SolidColorBrush brush)
                {
                    UIHelper.SetButtonHover(CancelButton, brush);
                }
            }
            UIHelper.SwitchAccentHoverClass(CancelButton);
        }

        var tab = vm.WindowTabs.ActiveTab.CurrentValue;
        var pixelWidth = tab.Model.PixelWidth;
        var pixelHeight = tab.Model.PixelHeight;

        _aspectRatio = (double)pixelWidth / pixelHeight;

        RegisterEventHandlers(vm);

        Observable.EveryValueChanged(tab.FileInfo, x => x.CurrentValue, UIHelper.GetFrameProvider)
            .Subscribe(UpdateState, DebugHelper.LogError(nameof(SingleImageResizeView), nameof(UpdateState)))
            .AddTo(_imageUpdateSubscription);
    }

    private void UpdateState(FileInfo? fileInfo)
    {
        if (fileInfo is null)
        {
            return;
        }
        UpdateQualitySliderState(fileInfo);
        ShowCancelButton();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        _imageUpdateSubscription?.Dispose();
    }

    private void RegisterEventHandlers(MainWindowViewModel vm)
    {
        var fileInfo = vm.WindowTabs.ActiveTab.CurrentValue.FileInfo.CurrentValue;
        UpdateQualitySliderState(fileInfo);
        QualitySlider.ValueChanged += (_, _) => ShowResetButton();

        SaveButton.Click += async (_, _) => await SaveImage(vm).ConfigureAwait(false);
        SaveAsButton.Click += async (_, _) => await SaveImageAs(fileInfo).ConfigureAwait(false);

        PixelWidthTextBox.KeyDown += async (_, e) => await SaveImageOnEnter(e);
        PixelHeightTextBox.KeyDown += async (_, e) => await SaveImageOnEnter(e);

        PixelWidthTextBox.KeyUp += (_, _) => AdjustAspectRatio(PixelWidthTextBox);
        PixelHeightTextBox.KeyUp += (_, _) => AdjustAspectRatio(PixelHeightTextBox);

        ConversionComboBox.SelectionChanged += (_, _) =>
        {
            UpdateQualitySliderState(fileInfo);
            ShowResetButton();
        };

        ResetButton.Click += (_, _) => ResetSettings();
        CancelButton.Click += (_, _) => (TopLevel.GetTopLevel(this) as Window)?.Close();

        LinkChainButton.Click += (_, _) => ToggleAspectRatio();
    }

    private void ShowResetButton()
    {
        CancelButton.IsVisible = false;
        ResetButton.IsVisible = true;
    }

    private void ShowCancelButton()
    {
        CancelButton.IsVisible = true;
        ResetButton.IsVisible = false;
    }

    private void AdjustAspectRatio(TextBox sender)
    {
        if (!_isKeepingAspectRatio || DataContext is not MainWindowViewModel vm)
        {
            return;
        }
        
        var tab = vm.WindowTabs.ActiveTab.CurrentValue;
        var pixelWidth = tab.Model.PixelWidth;
        var pixelHeight = tab.Model.PixelHeight;

        AspectRatioHelper.SetAspectRatioForTextBox(
            PixelWidthTextBox, PixelHeightTextBox, sender == PixelWidthTextBox,
            _aspectRatio, pixelWidth, pixelHeight);

        ShowResetButton();
    }

    private void UpdateQualitySliderState(FileInfo fileInfo)
    { try
        {
            if (IsConversionToQualityFormat())
            {
                QualitySlider.IsEnabled = true;
                QualitySlider.Value = 75;
            }
            else if (IsOriginalFileQualityFormat(fileInfo.Extension))
            {
                QualitySlider.IsEnabled = true;
                QualitySlider.Value = ImageAnalyzer.GetCompressionQuality(fileInfo.FullName);
            }
            else
            {
                QualitySlider.IsEnabled = false;
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(SingleImageResizeView), nameof(UpdateQualitySliderState), e);
        }
    }

    private bool IsConversionToQualityFormat()
        => JpgItem.IsSelected || PngItem.IsSelected;

    private static bool IsOriginalFileQualityFormat(string ext)
        => ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
           || ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
           || ext.Equals(".png", StringComparison.OrdinalIgnoreCase);

    private async Task SaveImageOnEnter(KeyEventArgs e)
    {
        if (e.Key != Key.Enter || DataContext is not MainWindowViewModel vm)
        {
            return;
        }

        var tab = vm.WindowTabs.ActiveTab.CurrentValue;
        var file = tab.FileInfo.CurrentValue;
        var destination = file.FullName;
        var isFlipped = tab.ScaleX.CurrentValue < 0;
        var rotationAngle = tab.RotationAngle.CurrentValue;
        await SaveImage(file, destination, isFlipped, rotationAngle).ConfigureAwait(false);
    }

    private async ValueTask SaveImageAs(FileInfo fileInfo)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop
            || desktop.MainWindow?.StorageProvider is null || DataContext  is not MainWindowViewModel vm)
        {
            return;
        }

        var fileInfoFullName = fileInfo.FullName;
        var ext = GetSelectedFileExtension(fileInfo, ref fileInfoFullName);

        var destination = await FilePicker.PickFileForSavingAsync(fileInfo.FullName, ext);
        if (destination is null)
        {
            return;
        }
        var tab = vm.WindowTabs.ActiveTab.CurrentValue;
        var isFlipped = tab.ScaleX.CurrentValue < 0;
        var rotationAngle = tab.RotationAngle.CurrentValue;
        await SaveImage(fileInfo, destination, isFlipped, rotationAngle).ConfigureAwait(false);
    }

    private async ValueTask SaveImage(MainWindowViewModel vm)
    {
        var tab = vm.WindowTabs.ActiveTab.CurrentValue;
        var fileInfo = tab.FileInfo.CurrentValue;
        var destination = fileInfo.FullName;
        var isFlipped = tab.ScaleX.CurrentValue < 0;
        var rotationAngle = tab.RotationAngle.CurrentValue;
        await SaveImage(fileInfo, destination, isFlipped, rotationAngle).ConfigureAwait(false);
    }

    private async ValueTask SaveImage(FileInfo fileInfo, string destination, bool isFLipped, int rotationAngle)
    {
        if (!uint.TryParse(PixelWidthTextBox.Text, out var width) ||
            !uint.TryParse(PixelHeightTextBox.Text, out var height))
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() => SetLoadingState(true));
        
        var ext = GetSelectedFileExtension(fileInfo, ref destination);
        destination = Path.ChangeExtension(destination, ext);
        var sameFile = destination.Equals(fileInfo.FullName,
            StringComparison.OrdinalIgnoreCase);
        var quality = GetQualityValue(ext, destination);

        using var magickImage = new MagickImage(fileInfo);
        if (quality is not null)
        {
            magickImage.Quality = quality.Value;
        }
        
        if (isFLipped)
        {
            magickImage.Flop();
        }
        
        if (rotationAngle != 0)
        {
            magickImage.Rotate(rotationAngle);
        }
        
        magickImage.Resize(width, height);
        await magickImage.WriteAsync(destination).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() => SetLoadingState(false));
        
    }

    private void SetLoadingState(bool isLoading)
    {
        ParentContainer.Opacity = isLoading ? 0.1 : 1;
        ParentContainer.IsHitTestVisible = !isLoading;
        SpinWaiter.IsVisible = isLoading;
    }

    private string GetSelectedFileExtension(FileInfo fileInfo, ref string destination)
    {
        var ext = fileInfo.Extension;
        if (NoConversion.IsSelected)
        {
            return ext;
        }

        ext = GetExtensionFromSelectedItem() ?? ext;
        destination = Path.ChangeExtension(destination, ext);
        return ext;
    }

    private string? GetExtensionFromSelectedItem()
    {
        if (PngItem.IsSelected)
        {
            return ".png";
        }

        if (JpgItem.IsSelected)
        {
            return ".jpg";
        }

        if (WebpItem.IsSelected)
        {
            return ".webp";
        }

        if (AvifItem.IsSelected)
        {
            return ".avif";
        }

        if (HeicItem.IsSelected)
        {
            return ".heic";
        }

        if (JxlItem.IsSelected)
        {
            return ".jxl";
        }

        return null;
    }

    private uint? GetQualityValue(string ext, string destination)
    {
        if (QualitySlider.IsEnabled && (
                ext.Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                Path.GetExtension(destination).Equals(".jpg", StringComparison.OrdinalIgnoreCase) ||
                Path.GetExtension(destination).Equals(".jpeg", StringComparison.OrdinalIgnoreCase)))
        {
            return (uint)QualitySlider.Value;
        }

        return null;
    }

    private void ResetSettings()
    {
        if (DataContext is not MainWindowViewModel vm)
        {
            return;
        }   
        var tab = vm.WindowTabs.ActiveTab.CurrentValue;
        var fileInfo = tab.FileInfo.CurrentValue;
        var pixelWidth = tab.Model.PixelWidth;
        var pixelHeight = tab.Model.PixelHeight;
        
        PixelWidthTextBox.Text = pixelWidth.ToString();
        PixelHeightTextBox.Text = pixelHeight.ToString();

        if (IsOriginalFileQualityFormat(fileInfo.Extension))
        {
            QualitySlider.IsEnabled = true;
            QualitySlider.Value = ImageAnalyzer.GetCompressionQuality(fileInfo.FullName);
        }
        else
        {
            QualitySlider.IsEnabled = false;
        }

        ConversionComboBox.SelectedItem = NoConversion;

        _isKeepingAspectRatio = true;
        ToggleLinkChain();

        ShowCancelButton();
    }

    private void ToggleAspectRatio()
    {
        _isKeepingAspectRatio = !_isKeepingAspectRatio;
        ToggleLinkChain();

        if (_isKeepingAspectRatio)
        {
            AdjustAspectRatio(PixelWidthTextBox);
        }

        if (!_isKeepingAspectRatio)
        {
            ShowResetButton();
        }
    }

    private void ToggleLinkChain()
    {
        if (!_isKeepingAspectRatio)
        {
            if (!Application.Current.TryGetResource("UnlinkChainImage",
                    Application.Current.RequestedThemeVariant, out var link))
            {
                return;
            }
            
            if (link is not DrawingImage linkImage)
            {
                return;
            }
            LinkChainButton.Icon  = linkImage;
        }
        else
        {
            if (!Application.Current.TryGetResource("LinkChainImage",
                    Application.Current.RequestedThemeVariant, out var link))
            {
                return;
            }
            
            if (link is not DrawingImage linkImage)
            {
                return;
            }
            LinkChainButton.Icon  = linkImage;
        }

    }

    ~SingleImageResizeView()
    {
        _imageUpdateSubscription?.Dispose();
    }
}