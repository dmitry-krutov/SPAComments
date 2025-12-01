namespace FileService.Contracts;

public sealed class GetFilesPresignedUrlResponse
{
    public IReadOnlyCollection<FilePresignedUrlDto> Files { get; init; }
        = Array.Empty<FilePresignedUrlDto>();
}