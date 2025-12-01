namespace FileService.Contracts;

public sealed class StoredFileDto
{
    public Guid Id { get; init; }

    public StoredFileKind Kind { get; init; }

    public string ContentType { get; init; } = null!;

    public long Size { get; init; }

    public int? Width { get; init; }

    public int? Height { get; init; }
}