namespace FileService.Contracts;

public sealed class ImageResizeOptions
{
    public int? MaxWidth { get; init; }

    public int? MaxHeight { get; init; }

    public bool KeepAspectRatio { get; init; } = true;
}