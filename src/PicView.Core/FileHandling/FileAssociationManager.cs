using System.Runtime.InteropServices;

namespace PicView.Core.FileHandling;

public static class FileAssociationManager
{
    public static void AssociateAllSupportedFiles()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            foreach (var ext in SupportedFiles.FileExtensions)
            {
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            foreach (var ext in SupportedFiles.FileExtensions)
            {
            }
        }
    }
        
    public static void UnassociateAllSupportedFiles()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            foreach (var ext in SupportedFiles.FileExtensions)
            {
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            foreach (var ext in SupportedFiles.FileExtensions)
            {
            }
        }
    }

    public static async Task<bool> AssociateFile(string fileExtension)
    {
        await Task.CompletedTask;
        return true;
    }
}
