using CSharpFunctionalExtensions;
using SPAComments.CommentsModule.Domain;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Application.Interfaces;

public interface ICommentsRepository
{
    Task<Result<Guid, Error>> Add(Comment comment, CancellationToken cancellationToken);
}