using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.CommentsModule.Infrastructure.DbContexts;

namespace SPAComments.CommentsModule.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly CommentsDbContext _dbContext;

    public UnitOfWork(CommentsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DbTransaction> BeginTransaction(CancellationToken cancellationToken = default)
    {
        IDbContextTransaction transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        return transaction.GetDbTransaction();
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _dbContext.SaveChangesAsync(ct);
}