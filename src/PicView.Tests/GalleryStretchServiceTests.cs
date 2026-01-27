using PicView.Core.Gallery;
using PicView.Core.ViewModels;
using Xunit;
using R3;

namespace PicView.Tests;

public class GalleryStretchServiceTests
{
    public GalleryStretchServiceTests()
    {
        PicView.Core.Config.SettingsManager.SetDefaults();
    }

    [Fact]
    public void SetStretch_Uniform_UpdatesPropertyAndItems()
    {
        // Arrange
        var vm = new GalleryViewModel();
        // Add some items
        var item1 = new GalleryItemViewModel();
        item1.ItemHeight.Value = 100;
        vm.GalleryItems.Value.Add(item1);

        // Act
        GalleryStretchService.SetStretch(vm, "Uniform");

        // Assert
        Assert.Equal("Uniform", vm.GalleryStretch.Value);
        Assert.Equal(double.NaN, item1.ItemWidth.Value);
    }

    [Fact]
    public void SetStretch_Square_UpdatesPropertyAndItems()
    {
        // Arrange
        var vm = new GalleryViewModel();
        var item1 = new GalleryItemViewModel();
        item1.ItemHeight.Value = 100;
        vm.GalleryItems.Value.Add(item1);

        // Act
        GalleryStretchService.SetStretch(vm, "Square");

        // Assert
        Assert.Equal("Uniform", vm.GalleryStretch.Value);
        Assert.Equal(100.0, item1.ItemWidth.Value);
    }

    [Fact]
    public void SetStretch_FillSquare_UpdatesPropertyAndItems()
    {
        // Arrange
        var vm = new GalleryViewModel();
        var item1 = new GalleryItemViewModel();
        item1.ItemHeight.Value = 100;
        vm.GalleryItems.Value.Add(item1);

        // Act
        GalleryStretchService.SetStretch(vm, "FillSquare");

        // Assert
        Assert.Equal("Fill", vm.GalleryStretch.Value);
        Assert.Equal(100.0, item1.ItemWidth.Value);
    }
}
