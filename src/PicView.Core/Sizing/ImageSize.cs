namespace PicView.Core.Sizing;

public readonly struct ImageSize(
    double width,
    double height,
    double secondaryWidth,
    double scrollViewerWidth,
    double scrollViewerHeight,
    double titleMaxWidth,
    double margin,
    double aspectRatio)
{
    public double TitleMaxWidth { get; } = titleMaxWidth;
    public double Width { get; } = width;
    public double Height { get; } = height;

    public double ScrollViewerWidth { get; } = scrollViewerWidth;
    public double ScrollViewerHeight { get; } = scrollViewerHeight;

    public double SecondaryWidth { get; } = secondaryWidth;
    public double Margin { get; } = margin;

    public double AspectRatio { get; } = aspectRatio;
}