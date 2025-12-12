using R3;

namespace PicView.Core.ViewModels;

public class TitleViewModel
{
    public BindableReactiveProperty<string>? Title { get; } = new();

    public BindableReactiveProperty<string>? TitleTooltip { get; } = new();
    public BindableReactiveProperty<string>? WindowTitle { get; } = new();

}