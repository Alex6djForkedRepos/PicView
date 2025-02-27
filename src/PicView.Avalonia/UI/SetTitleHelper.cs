using PicView.Avalonia.ImageHandling;
using PicView.Avalonia.Navigation;
using PicView.Avalonia.ViewModels;
using PicView.Core.FileHandling;
using PicView.Core.ImageDecoding;
using PicView.Core.Localization;
using PicView.Core.Navigation;

namespace PicView.Avalonia.UI;

public static class SetTitleHelper
{
    public static void SetTitle(MainViewModel vm)
    {
        if (!NavigationManager.CanNavigate(vm))
        {
            string title;
            var s = vm.Title;
            if (!string.IsNullOrWhiteSpace(s.GetURL()))
            {
                title = vm.Title.GetURL();
            }
            else if (s.Contains(TranslationHelper.Translation.Base64Image))
            {
                title = TranslationHelper.Translation.Base64Image ?? "Base64 Image";
            }
            else
            {
                title = TranslationHelper.Translation.ClipboardImage ?? "Clipboard Image";
            }
            
            var singeImageWindowTitles = ImageTitleFormatter.GenerateTitleForSingleImage(vm.PixelWidth, vm.PixelWidth, title, vm.ZoomValue);
            vm.WindowTitle = singeImageWindowTitles.BaseTitle;
            vm.Title = singeImageWindowTitles.TitleWithAppName;
            vm.TitleTooltip = singeImageWindowTitles.TitleWithAppName;
            return;
        }

        var windowTitles = ImageTitleFormatter.GenerateTitleStrings(vm.PixelWidth, vm.PixelHeight, NavigationManager.GetCurrentIndex,
            vm.FileInfo, vm.ZoomValue, NavigationManager.GetCollection);
        vm.WindowTitle = windowTitles.TitleWithAppName;
        vm.Title = windowTitles.BaseTitle;
        vm.TitleTooltip = windowTitles.FilePathTitle;
    }

    public static void RefreshTitle(MainViewModel vm)
    {
        if (vm.FileInfo == null)
        {
            return;
        }
        var path = vm.FileInfo.FullName;
        vm.FileInfo = new FileInfo(path);
        SetTitle(vm);
    }
    
    public static void SetLoadingTitle(MainViewModel vm)
    {
        vm.WindowTitle = $"{TranslationHelper.Translation.Loading} - PicView";
        vm.Title = TranslationHelper.Translation.Loading;
        vm.TitleTooltip = vm.Title;
    }
    
    public static void SetTitle(MainViewModel vm, ImageModel? imageModel)
    {
        if (imageModel is null)
        {
            ReturnError();
            return;
        }

        if (imageModel.FileInfo is null)
        {
            ReturnError();
            return;
        }

        var windowTitles = ImageTitleFormatter.GenerateTitleStrings(imageModel.PixelWidth, imageModel.PixelHeight,  NavigationManager.GetCurrentIndex,
            imageModel.FileInfo,  vm.ZoomValue,  NavigationManager.GetCollection);
        vm.WindowTitle = windowTitles.TitleWithAppName;
        vm.Title = windowTitles.BaseTitle;
        vm.TitleTooltip = windowTitles.FilePathTitle;

        return;

        void ReturnError()
        {
            vm.WindowTitle =
                vm.Title =
                    vm.TitleTooltip = TranslationHelper.GetTranslation("UnableToRender");
        }
    }
    
    public static void SetTiffTitle(TiffManager.TiffNavigationInfo tiffNavigationInfo, int width, int height, int index, FileInfo fileInfo, MainViewModel vm)
    {
        var name = tiffNavigationInfo.Pages[tiffNavigationInfo.CurrentPage].FileName + $" [{tiffNavigationInfo.CurrentPage + 1}/{tiffNavigationInfo.PageCount}]";
        var singeImageWindowTitles = ImageTitleFormatter.GenerateTiffTitleStrings(width, height, index, fileInfo, tiffNavigationInfo, 1, NavigationManager.GetCollection);
        vm.WindowTitle = singeImageWindowTitles.TitleWithAppName;
        vm.Title = singeImageWindowTitles.BaseTitle; 
        vm.TitleTooltip = singeImageWindowTitles.BaseTitle;
    }
    
    public static void TrySetTiffTitle(int width, int height, int index, FileInfo fileInfo, MainViewModel vm)
    {
        if (TiffManager.GetTiffPageCount(fileInfo.FullName) is { } pageCount and > 1)
        {
            var tiffNavigationInfo = new TiffManager.TiffNavigationInfo
            {
                CurrentPage = 0,
                PageCount = pageCount,
                Pages = TiffManager.LoadTiffPages(fileInfo.FullName)
            };
            SetTiffTitle(tiffNavigationInfo, width, height,
                NavigationManager.GetCurrentIndex, fileInfo, vm);
        }
        else
        {
            SetTitle(vm);
        }
    }
    
    public static void SetSideBySideTitle(MainViewModel vm, ImageModel? imageModel1, ImageModel? imageModel2)
    {
        if (imageModel1 is null || imageModel2 is null)
        {
            ReturnError();
            return;
        }

        if (imageModel1.FileInfo is null || imageModel2.FileInfo is null)
        {
            ReturnError();
            return;
        }

        var firstWindowTitles = ImageTitleFormatter.GenerateTitleStrings(imageModel1.PixelWidth, imageModel1.PixelHeight,  NavigationManager.GetCurrentIndex,
            imageModel1.FileInfo,  vm.ZoomValue,  NavigationManager.GetCollection);
        var secondWindowTitles = ImageTitleFormatter.GenerateTitleStrings(imageModel2.PixelWidth, imageModel2.PixelHeight,  NavigationManager.GetNextIndex,
            imageModel2.FileInfo,  vm.ZoomValue,  NavigationManager.GetCollection);
        var windowTitle = $"{firstWindowTitles.BaseTitle} \u21dc || \u21dd {secondWindowTitles.BaseTitle} - PicView";
        var title = $"{firstWindowTitles.BaseTitle} \u21dc || \u21dd  {secondWindowTitles.BaseTitle}";
        var titleTooltip = $"{firstWindowTitles.FilePathTitle} \u21dc || \u21dd  {secondWindowTitles.FilePathTitle}";
        vm.WindowTitle = windowTitle;
        vm.Title = title;
        vm.TitleTooltip = titleTooltip;

        return;

        void ReturnError()
        {
            vm.WindowTitle =
                vm.Title =
                    vm.TitleTooltip = TranslationHelper.GetTranslation("UnableToRender");
        }
    }

    public static void ResetTitle(MainViewModel vm)
    {
        vm.Title = TranslationHelper.Translation.NoImage;
        vm.WindowTitle = TranslationHelper.Translation.NoImage + " - PicView";
        vm.TitleTooltip = TranslationHelper.Translation.NoImage;
    }
}
