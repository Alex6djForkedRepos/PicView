using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PicView.Core.DebugTools;

namespace PicView.Avalonia.CustomControls;

public class ThumbImage : Image
{
    protected override Size MeasureOverride(Size availableSize)
    {
        Size? size = null;
        try
        {
            size = new Size();


        if (Source != null)
        {
            size = Stretch.CalculateSize(availableSize, Source.Size, StretchDirection);
        }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(ThumbImage), nameof(MeasureOverride), e);
        }

        return size ?? new Size();
    }
    
    protected override Size ArrangeOverride(Size finalSize)
    {
        try
        {
            if (Source != null)
            {
                var sourceSize = Source.Size;
                var result = Stretch.CalculateSize(finalSize, sourceSize);
                return result;
            }
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(nameof(ThumbImage), nameof(ArrangeOverride), e);
        }
        return new Size();
    }
}