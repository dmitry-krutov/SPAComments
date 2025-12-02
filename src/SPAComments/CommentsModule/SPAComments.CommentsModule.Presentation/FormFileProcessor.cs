using Microsoft.AspNetCore.Http;

namespace SPAComments.CommentsModule.Presentation;

public sealed class FormFileProcessor : IAsyncDisposable
{
    private UploadFileDto? _fileDto;

    public UploadFileDto Process(IFormFile file)
    {
        var stream = file.OpenReadStream();

        _fileDto = new UploadFileDto(
            stream,
            file.FileName,
            file.ContentType ?? "application/octet-stream",
            file.Length);

        return _fileDto;
    }

    public async ValueTask DisposeAsync()
    {
        if (_fileDto is not null)
        {
            await _fileDto.Content.DisposeAsync();
        }
    }
}

public sealed record UploadFileDto(
    Stream Content,
    string FileName,
    string ContentType,
    long Length);