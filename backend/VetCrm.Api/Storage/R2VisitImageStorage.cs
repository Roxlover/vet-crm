using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace VetCrm.Api.Storage;

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

        var creds = new BasicAWSCredentials(_opt.AccessKey, _opt.SecretKey);
        _s3 = new AmazonS3Client(creds, cfg);
    }

    public async Task<string> UploadVisitImageAsync(int visitId, Stream stream, string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            contentType = "application/octet-stream";

        var ext = contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png"  => ".png",
            "image/webp" => ".webp",
            "image/gif"  => ".gif",
            _ => ""
        };

        var key = $"visits/{visitId}/{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{ext}";

        // Stream -> byte[] (chunked/trailer riskini azaltmak için)
        byte[] data;
        await using (var ms = new MemoryStream())
        {
            await stream.CopyToAsync(ms);
            data = ms.ToArray();
        }

        var mem = new MemoryStream(data);

        var req = new PutObjectRequest
        {
            BucketName = _opt.Bucket,
            Key = key,
            InputStream = mem,
            ContentType = contentType,
            AutoCloseStream = true
        };

        // ESKİ SDK’larda ContentLength property yok; Headers üzerinden verilir.
        // (Headers property varsa compile eder)
        req.Headers.ContentLength = data.LongLength;

        // Eğer SDK’nızda varsa bu satır chunked encoding’i kapatır ve R2’yi rahatlatır.
        // Compile hata verirse bu satırı silin.
        req.UseChunkEncoding = false;

        await _s3.PutObjectAsync(req);

        return $"{_opt.PublicBaseUrl.TrimEnd('/')}/{key}";
    }
}
