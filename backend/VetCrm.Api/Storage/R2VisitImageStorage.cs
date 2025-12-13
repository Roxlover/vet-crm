using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using VetCrm.Infrastructure.Storage;

namespace VetCrm.Api.Storage;

public class R2Options
{
    public string AccountId { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string Bucket { get; set; } = default!;
    public string PublicBaseUrl { get; set; } = default!;
}

public class R2VisitImageStorage : IR2Storage
{
    private readonly R2Options _opt;
    private readonly IAmazonS3 _s3;

    public R2VisitImageStorage(IOptions<R2Options> opt)
    {
        _opt = opt.Value;

        var endpoint = $"https://{_opt.AccountId}.r2.cloudflarestorage.com";

        var cfg = new AmazonS3Config
        {
            ServiceURL = endpoint,
            ForcePathStyle = true
        };

        _s3 = new AmazonS3Client(_opt.AccessKey, _opt.SecretKey, cfg);
    }

    // Interface imzası birebir: (visitId, stream, contentType)
    public async Task<string> UploadVisitImageAsync(int visitId, Stream stream, string contentType)
    {
        // contentType boş gelirse fallback
        if (string.IsNullOrWhiteSpace(contentType))
            contentType = "application/octet-stream";

        // Uzantı contentType'dan türet (isteğe bağlı ama iyi)
        var ext = contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png"  => ".png",
            "image/webp" => ".webp",
            "image/gif"  => ".gif",
            _ => ""
        };

        var key = $"visits/{visitId}/{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";

        var req = new PutObjectRequest
        {
            BucketName = _opt.Bucket,
            Key = key,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3.PutObjectAsync(req);

        return $"{_opt.PublicBaseUrl.TrimEnd('/')}/{key}";
    }
}
