using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;

namespace VetCrm.Api.Storage;

public static class ImageOptimizer
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".tiff" };

    public static bool IsImage(string contentType, string? fileName = null)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return Array.Exists(AllowedExtensions, e => e == ext);
        }

        var ct = contentType.ToLowerInvariant();
        return ct.StartsWith("image/") && !ct.Contains("gif");
    }

    public static async Task<(Stream stream, string contentType, string extension)> OptimizeToWebpAsync(Stream inputStream)
    {
        var outputStream = new MemoryStream();
        try
        {
            if (inputStream.CanSeek)
            {
                inputStream.Position = 0;
            }

            using var image = await SixLabors.ImageSharp.Image.LoadAsync(inputStream);
            
            // Note: Per user request, we are NOT applying any max dimension boundaries or resizing.
            // We only compress and encode the image as WebP format.

            var encoder = new WebpEncoder
            {
                Quality = 80
            };
            
            await image.SaveAsync(outputStream, encoder);
            outputStream.Position = 0;

            return (outputStream, "image/webp", ".webp");
        }
        catch
        {
            if (inputStream.CanSeek)
            {
                inputStream.Position = 0;
            }
            outputStream.Dispose();
            
            var fallbackStream = new MemoryStream();
            await inputStream.CopyToAsync(fallbackStream);
            fallbackStream.Position = 0;
            
            return (fallbackStream, "application/octet-stream", "");
        }
    }
}
