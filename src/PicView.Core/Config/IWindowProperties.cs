namespace PicView.Core.Config;

public interface IWindowProperties
{
    public int? Top { get; set; }
    public int? Left { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public bool Maximized { get; set; }
}