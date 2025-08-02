using System.Globalization;
using ImageMagick;
using PicView.Core.DebugTools;
using PicView.Core.FileHandling;

namespace PicView.Core.Exif;

public static class ExifFunctions
{
    /// <summary>
    /// Determines whether the specified file is an EXIF-supported image type.
    /// </summary>
    /// <param name="fileInfo">The file information of the image to check.</param>
    /// <returns>True if the file is an EXIF-supported image type; otherwise, false.</returns>
    public static bool IsExifImage(this FileInfo fileInfo)
    {
        if (fileInfo is null)
        {
            return false;
        }

        return fileInfo.Extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or
                ".tif" or ".tiff" or
                ".dng" or // Adobe Digital Negative
                ".cr2" or ".cr3" or // Canon RAW
                ".nef" or // Nikon RAW
                ".arw" or // Sony RAW
                ".orf" or // Olympus RAW
                ".rw2" or // Panasonic RAW
                ".pef" or // Pentax RAW
                ".raf" or // Fujifilm RAW
                ".srw" or // Samsung RAW
                ".heif" or ".heic" // High Efficiency Image Format
                => true,

            _ => false
        };
    }

    public static async Task<bool> TryUpdateImageProfileAsync(FileInfo fileInfo, Func<MagickImage, bool> updateAction,
        string callingClassName, string callingMethodName)
    {
        try
        {
            using var magickImage = new MagickImage(fileInfo);

            // If the update action returns false, it means no changes were made that require saving.
            if (!updateAction(magickImage))
            {
                return false;
            }

            await using var fileStream = FileStreamUtils.GetOptimizedFileStream(fileInfo, true);
            await magickImage.WriteAsync(fileStream);
            return true;
        }
        catch (Exception e)
        {
            DebugHelper.LogDebug(callingClassName, callingMethodName, e);
            return false;
        }
    }
    
    public static bool TryParseRational(string input, out Rational result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(input)) return false;

        // Handle fraction format "num/den"
        if (input.Contains('/'))
        {
            var parts = input.Split('/');
            if (parts.Length == 2 && double.TryParse(parts[0], CultureInfo.InvariantCulture, out var num) && double.TryParse(parts[1], CultureInfo.InvariantCulture, out var den))
            {
                if (den == 0) return false;
                result = new Rational((uint)num, (uint)den);
                return true;
            }
        }

        // Handle decimal format
        if (!double.TryParse(input, CultureInfo.InvariantCulture, out var val))
        {
            return false;
        }

        result = new Rational(val);
        return true;

    }

    /// <summary>
    /// Tries to parse a string into a SignedRational.
    /// </summary>
    /// <param name="input">The string to parse (e.g., "-10/20" or "-0.5").</param>
    /// <param name="result">The parsed SignedRational.</param>
    /// <returns>True if parsing was successful, otherwise false.</returns>
    public static bool TryParseSignedRational(string input, out SignedRational result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        // Handle fraction format "num/den"
        if (input.Contains('/'))
        {
            var parts = input.Split('/');
            if (parts.Length == 2 && 
                int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var numerator) &&
                int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var denominator))
            {
                if (denominator == 0)
                {
                    return false; // Avoid division by zero
                }

                result = new SignedRational(numerator, denominator, false);
                return true;
            }
        }

        // Handle decimal format
        if (!double.TryParse(input, CultureInfo.InvariantCulture, out var val))
        {
            return false;
        }

        result = new SignedRational(val);
        return true;

    }
}