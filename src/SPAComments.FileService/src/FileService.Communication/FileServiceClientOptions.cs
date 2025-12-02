namespace FileService.Communication;

public sealed class FileServiceClientOptions
{
    public const string SECTION_NAME = "FileService";
    public string BaseAddress { get; init; } = null!;

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}