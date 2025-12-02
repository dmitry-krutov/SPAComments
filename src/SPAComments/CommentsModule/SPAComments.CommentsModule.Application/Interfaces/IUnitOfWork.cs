using System.Data.Common;

namespace SPAComments.CommentsModule.Application.Interfaces;

public interface IUnitOfWork
{
    Task<DbTransaction> BeginTransaction(CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}