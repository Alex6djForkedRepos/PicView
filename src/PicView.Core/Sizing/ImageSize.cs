namespace PicView.Core.Sizing;

public readonly record struct ImageSize(
    double Width,
    double Height,
    double SecondaryWidth,
    double ScrollViewerWidth,
    double ScrollViewerHeight,
    double TitleMaxWidth,
    double Margin,
    double AspectRatio);