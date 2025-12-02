using CSharpFunctionalExtensions;
using FileService.Contracts;
using SPAComments.SharedKernel;

namespace FileService.Communication;

public interface IFileServiceClient
{
    Task<Result<StoredFileDto, Error>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        UploadFileRequest meta,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyCollection<FilePresignedUrlDto>, Error>> GetPresignedUrlsAsync(
        IReadOnlyCollection<Guid> fileIds,
        int ttlSeconds,
        CancellationToken cancellationToken);
}