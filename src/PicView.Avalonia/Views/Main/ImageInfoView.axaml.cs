using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.Resizing;
using PicView.Avalonia.ViewModels;
using PicView.Core.Conversion;
using PicView.Core.Exif;
using PicView.Core.Extensions;
using PicView.Core.FileHandling;
using PicView.Core.Sizing;
using PicView.Core.Titles;
using R3;

namespace PicView.Avalonia.Views.Main;

public partial class ImageInfoView : UserControl
{
    private readonly CompositeDisposable _disposables = new();

    public ImageInfoView()
    {
        InitializeComponent();
    }
}