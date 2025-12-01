namespace FileService.Contracts;

public sealed class UploadFileResponse
{
    public required StoredFileDto File { get; init; }
}