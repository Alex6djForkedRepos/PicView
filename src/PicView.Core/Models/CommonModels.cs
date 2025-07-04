namespace PicView.Core.Models;

public static class CommonModels
{
    public enum Stretch
    {
        None,
        Fill,
        Uniform,
        UniformToFill
    } 
    
    public enum VerticalAlignment
    {
        Stretch,
        Top,
        Center,
        Bottom,
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
    }
}