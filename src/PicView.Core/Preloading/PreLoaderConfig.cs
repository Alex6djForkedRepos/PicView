namespace PicView.Core.Preloading;

public class PreLoaderConfig
{
    public static int PositiveIterations => Settings.Navigation.PositiveIterations;
    public static int NegativeIterations => Settings.Navigation.NegativeIterations;
    
    // Total items to preload forward and backward, plus the current item and a buffer.
    public static int MaxCount => PositiveIterations + NegativeIterations + 2; 

    // Leave a few cores for the UI thread and other system processes to ensure responsiveness.
    public int MaxParallelism { get; } = Math.Max(1, Environment.ProcessorCount - 3);
}