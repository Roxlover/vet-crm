namespace VetCrm.Api.Storage;

public class R2Options
{
    public string AccountId { get; set; } = default!;
    public string AccessKey { get; set; } = default!;
    public string SecretKey { get; set; } = default!;
    public string Bucket { get; set; } = default!;
    public string PublicBaseUrl { get; set; } = default!;

    // Fallbacks in case appsettings.json uses AWS terminology
    public string AccessKeyId { set { AccessKey = value; } }
    public string SecretAccessKey { set { SecretKey = value; } }
}
