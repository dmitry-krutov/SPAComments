using CSharpFunctionalExtensions;
using SPAComments.CommentsModule.Application.Features.Common;
using SPAComments.CommentsModule.Application.Features.Queries.GetById;
using SPAComments.CommentsModule.Application.Features.Queries.GetLatest;
using SPAComments.CommentsModule.Domain;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Application.Interfaces;

public interface ICommentsRepository
{
    Task<Result<Guid, Error>> Add(Comment comment, CancellationToken cancellationToken);

    Task<PagedResult<LatestCommentReadModel>> ReadLatestAsync(
        GetLatestCommentsQuery query,
        CancellationToken cancellationToken);

    Task<Result<CommentReadModel, Error>> ReadByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}