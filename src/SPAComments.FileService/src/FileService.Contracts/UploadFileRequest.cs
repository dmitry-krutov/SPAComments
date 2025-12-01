namespace FileService.Contracts;

public sealed class UploadFileRequest
{
    public StoredFileKind Kind { get; init; }

    public string? Category { get; init; }

    public ImageResizeOptions? Resize { get; init; }
}