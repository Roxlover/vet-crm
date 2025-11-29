using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using VetCrm.Infrastructure.Storage;

namespace VetCrm.Api.Storage  // projendeki namespace'e göre ayarla
{
    public class LocalVisitImageStorage : IR2Storage
    {
        private readonly IWebHostEnvironment _env;

        public LocalVisitImageStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<string> UploadVisitImageAsync(int visitId, Stream stream, string contentType)
        {
            // wwwroot yolunu bul
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
            }

            // wwwroot/visit-images/{visitId}/
            var folder = Path.Combine(webRoot, "visit-images", visitId.ToString());
            Directory.CreateDirectory(folder);

            var ext = GetExtension(contentType);
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            await using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await stream.CopyToAsync(fileStream);
            }

            // Browser’ın isteyeceği path
            // Örn: /visit-images/40/abc123.png
            return $"/visit-images/{visitId}/{fileName}";
        }

        private static string GetExtension(string? contentType) =>
            contentType switch
            {
                "image/jpeg" or "image/jpg" => ".jpg",
                "image/png"                  => ".png",
                "image/gif"                  => ".gif",
                _                            => ".bin"
            };
    }
}
