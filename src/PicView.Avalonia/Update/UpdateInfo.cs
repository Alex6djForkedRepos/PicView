namespace PicView.Avalonia.Update;

public class UpdateInfo
{
    public required string Version { get; set; }
    
    ////////////\\\\\\\\\\\\
    ///  Windows versions \\\
    ////////////\\\\\\\\\\\\\
    public required string X64Portable { get; set; }
    public required string X64Install { get; set; }
    public required string Arm64Portable { get; set; }
    public required string Arm64Install { get; set; }
    
    
    ////////////\\\\\\\\\\\
    ///  macOS versions \\\
    ////////////\\\\\\\\\\\\
    public required string MacIntel { get; set; }
    public required string MacArm64 { get; set; }
}
