using CSharpFunctionalExtensions;
using MongoDB.Driver;
using SPAComments.SharedKernel;

namespace FileService.MongoDataAccess;

public class FilesRepository : IFilesRepository
{
    private readonly FileMongoDbContext _dbContext;

    public FilesRepository(FileMongoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Guid, Error>> Add(FileData fileData, CancellationToken cancellationToken)
    {
        await _dbContext.Files.InsertOneAsync(fileData, cancellationToken: cancellationToken);

        return fileData.Id;
    }

    public async Task<IReadOnlyCollection<FileData>> Get(IEnumerable<Guid> fileIds, CancellationToken cancellationToken)
        => await _dbContext.Files.Find(f => fileIds.Contains(f.Id)).ToListAsync(cancellationToken);

    public async Task<UnitResult<Error>> DeleteMany(IEnumerable<Guid> fileIds, CancellationToken cancellationToken)
    {
        var deleteResult =
            await _dbContext.Files.DeleteManyAsync(f => fileIds.Contains(f.Id), cancellationToken: cancellationToken);

        if (deleteResult.DeletedCount == 0)
            return Errors.Files.FailRemove();

        return Result.Success<Error>();
    }
}