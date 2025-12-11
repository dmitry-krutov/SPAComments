namespace FileService;

public class MinioOptions
{
    public const string MINIO = "Minio";

    public required string Bucket { get; init; }

    public string Endpoint { get; init; } = string.Empty;

    public string PublicBaseUrl { get; init; } = string.Empty;

    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;

    public bool WithSsl { get; init; } = false;
}