using CSharpFunctionalExtensions;
using SPAComments.SharedKernel;

namespace FileService.MongoDataAccess;

public interface IFilesRepository
{
    Task<Result<Guid, Error>> Add(FileData fileData, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<FileData>> Get(IEnumerable<Guid> fileIds, CancellationToken cancellationToken);

    Task<UnitResult<Error>> DeleteMany(IEnumerable<Guid> fileIds, CancellationToken cancellationToken);
}