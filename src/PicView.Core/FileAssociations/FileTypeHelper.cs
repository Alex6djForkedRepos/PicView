using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using PicView.Core.FileHandling;
using PicView.Core.Localization;

namespace PicView.Core.FileAssociations;

public static class FileTypeHelper
{
    public static FileTypeGroup[] GetFileTypes()
    {
        var groups = new[]
        {
            new FileTypeGroup(TranslationManager.Translation.Normal, [
                new FileTypeItem("Joint Photographic Experts Group", [".jpg", ".jpeg", ".jpe"]),
                new FileTypeItem("JPEG File Interchange Format", [".jfif"]),
                new FileTypeItem("Portable Network Graphics", [".png"]),
                new FileTypeItem("Windows Bitmap", [".bmp"]),
                new FileTypeItem("Graphics Interchange Format", [".gif"]),
                new FileTypeItem("WebP", [".webp"]),
                new FileTypeItem("Wireless Bitmap", [".wbmp"]),
                new FileTypeItem("Advanced Video Interlace Format", [".avif"]),
                new FileTypeItem("Icon", [".ico"])
            ]),
            
            new FileTypeGroup(TranslationManager.GetTranslation("Graphics"), [
                new FileTypeItem("Scalable Vector Graphics", [".svg", ".svgz"]),
                new FileTypeItem("Photoshop", [".psd", ".psb"]),
                new FileTypeItem("XCF", [".xcf"]),
                new FileTypeItem("Tagged Image File Format", [".tif", ".tiff"]),
                new FileTypeItem("High-Enhanced Image File", [".heic", ".heif"]),
                new FileTypeItem("JPEG XL", [".jxl"]),
                new FileTypeItem("JPEG 2000", [".jp2"]),
                new FileTypeItem("High Dynamic Range", [".hdr"]),
                new FileTypeItem("Quite OK Image", [".qoi"]),
                new FileTypeItem("Direct Draw Surface", [".dds"]),
                new FileTypeItem("Truevision Targa", [".tga"]),
                new FileTypeItem("Industrial Light & Magic OpenEXR", [".exr"])
            ]),
            
            new FileTypeGroup(TranslationManager.GetTranslation("Raw"), [
                new FileTypeItem("Raw", [".raw"]),
                new FileTypeItem("Framed Raster", [".3fr"]),
                new FileTypeItem("Sony Digital Camera RAW", [".arw"]),
                new FileTypeItem("Canon Digital Camera RAW", [".cr2, .cr3, .crw"]),
                new FileTypeItem("Kodak Raw", [".dcr", ".kdc"]),
                new FileTypeItem("Digital Negative RAW", [".dng"]),
                new FileTypeItem("Epson Raw Image", [".erf"]),
                new FileTypeItem("Minolta Raw Image", [".mdc"]),
                new FileTypeItem("Nikon Raw Image", [".nef"]),
                new FileTypeItem("Mamiya Raw Image", [".mef"]),
                new FileTypeItem("Leaf/Aptus/Mamiya MOS Raw Image", [".mos"]),
                new FileTypeItem("Minolta Dimage RAW", [".mrw"]),
                new FileTypeItem("Nikon Raw Image", [".nef"]),
                new FileTypeItem("Nokia RAW Bitmap", [".nrw"]),
                new FileTypeItem("Olympus Raw Image", [".orf"]),
                new FileTypeItem("Pentax Raw Image", [".pef"]),
                new FileTypeItem("Sony SRF Raw", [".srf"]),
                new FileTypeItem("Sigma Foveon X3", [".x3f"]),
                new FileTypeItem("Kodak FlashPix Bitmap", [".fpx"]),
                new FileTypeItem("Kodak PhotoCD Bitmap", [".pcd"]),
                new FileTypeItem("Kodak Raw", [".dcr"]),
                new FileTypeItem("Windows Metafile Image", [".wmf", ".emf"]),
            ]),
            
            new FileTypeGroup(TranslationManager.GetTranslation("Uncommon"), [
                new FileTypeItem("Wordperfect Graphics", [".wpg"]),
                new FileTypeItem("Paintbrush bitmap graphics", [".pcx"]),
                new FileTypeItem("X Bitmap", [".xbm"]),
                new FileTypeItem("PX PixMap Bitmap", [".xpm"]),
                new FileTypeItem("Dr. Halo ", [".cut"]),
                new FileTypeItem("Truevision Thumb", [".thm"]),
                new FileTypeItem("Portable GrayMap Bitmap", [".ppm"]),
                new FileTypeItem("Portable PixMap Bitmap", [".pbm"]),
                new FileTypeItem("Base64", [".b64"])
            ]),
            
            new FileTypeGroup(TranslationManager.GetTranslation("Archives"), [
                new FileTypeItem("Zip", [".zip"], null),
                new FileTypeItem("Rar", [".rar"], null),
                new FileTypeItem("Gzip", [".gzip"], null),
                new FileTypeItem("CDisplay Archived Comic Book", [".cbr, .cbz, .cb7"])
            ], null)
        };

        return groups;
    }
    
   
    public static async Task<bool> SetFileAssociations(ReadOnlyObservableCollection<FileTypeGroup> groups)
    {
        try
        {
            // If we're on Windows, check for admin permissions
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !IsAdministrator())
            {
                // Build list of extensions to associate
                var extensionsToAssociate = new List<string>();
                
                foreach (var group in groups)
                {
                    foreach (var fileType in group.FileTypes)
                    {
                        if (!fileType.IsSelected.HasValue || !fileType.IsSelected.Value) 
                            continue;
                        
                        foreach (var extension in fileType.Extensions)
                        {
                            // Make sure to properly handle extensions that contain commas
                            var individualExtensions = extension.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        
                            foreach (var ext in individualExtensions)
                            {
                                var cleanExt = ext.Trim();
                                if (!cleanExt.StartsWith('.'))
                                    cleanExt = "." + cleanExt;
                                
                                extensionsToAssociate.Add(cleanExt);
                            }
                        }
                    }
                }
                
                if (extensionsToAssociate.Count > 0)
                {
                    // Create command arguments - keep argument string shorter to avoid issues
                    string associateArg = "associate:" + string.Join(",", extensionsToAssociate);
                    
                    // Start new process with elevated permissions
                    return StartProcessWithElevatedPermission(associateArg);
                }
                
                return true; // Nothing to do
            }
            
            // Standard processing path (non-Windows or already has admin rights)
            foreach (var group in groups)
            {
                foreach (var fileType in group.FileTypes)
                {
                    if (!fileType.IsSelected.HasValue) 
                        continue;
                    
                    foreach (var extension in fileType.Extensions)
                    {
                        var individualExtensions = extension.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        
                        foreach (var ext in individualExtensions)
                        {
                            var cleanExt = ext.Trim();
                            if (!cleanExt.StartsWith('.'))
                                cleanExt = "." + cleanExt;
                
                            if (fileType.IsSelected.Value)
                                await FileAssociationManager.AssociateFile(cleanExt);
                            else
                                await FileAssociationManager.UnassociateFile(cleanExt);
                        }
                    }
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            // Log the exception or handle it appropriately
            Debug.WriteLine($"Error in SetFileAssociations: {ex}");
            return false;
        }
    }
    
    private static bool IsAdministrator()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return false;
            
        // Check if running as administrator
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
    
    private static bool StartProcessWithElevatedPermission(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule?.FileName,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas" // This requests elevated permissions
            };
            
            Process.Start(startInfo);
            return true;
        }
        catch (Exception ex)
        {
            // User declined the UAC prompt or other error
            Debug.WriteLine($"Failed to start elevated process: {ex.Message}");
            return false;
        }
    }
    
    public static async Task ProcessFileAssociationArguments(string arg)
    {
        try
        {
            if (arg.StartsWith("associate:", StringComparison.OrdinalIgnoreCase))
            {
                string extensionsString = arg["associate:".Length..];
                if (string.IsNullOrWhiteSpace(extensionsString))
                    return;
                    
                var extensions = extensionsString
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(ext => ext.Trim())
                    .ToArray();
                    
                foreach (var extension in extensions)
                {
                    await FileAssociationManager.AssociateFile(extension);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error processing file association arguments: {ex}");
        }
    }
}
