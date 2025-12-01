namespace FileService.Contracts;

public sealed class GetFilesPresignedUrlRequest
{
    public IReadOnlyCollection<Guid> FileIds { get; init; } = Array.Empty<Guid>();

    public int TtlSeconds { get; init; } = 300;
}