using ImageMagick;

namespace PicView.Core.Navigation.Tiff;

public class TiffNavigationInfo : IDisposable
{
    public int PageCount { get; set; }
    public int CurrentPage { get; set; }

    public MagickImageCollection? Pages { get; set; }

    #region IDisposable

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        if (Pages == null)
        {
            return;
        }

        Pages.Dispose();
        Pages = null;
    }

    #endregion
}