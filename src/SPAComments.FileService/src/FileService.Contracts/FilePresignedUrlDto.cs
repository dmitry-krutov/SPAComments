namespace FileService.Contracts;

public sealed class FilePresignedUrlDto
{
    public Guid FileId { get; init; }

    public string Url { get; init; } = null!;

    public DateTime ExpiresAtUtc { get; init; }
}