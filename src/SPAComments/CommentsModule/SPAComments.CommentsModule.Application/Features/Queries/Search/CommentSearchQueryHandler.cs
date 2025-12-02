using CSharpFunctionalExtensions;
using SPAComments.CommentsModule.Application.Features.Common;
using SPAComments.Core.Abstractions;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Application.Features.Queries.Search;

public class CommentSearchQueryHandler
    : IQueryHandlerWithResult<PagedResult<CommentSearchItemDto>, CommentSearchQuery>
{
    private readonly ICommentSearchReader _reader;

    public CommentSearchQueryHandler(ICommentSearchReader reader)
    {
        _reader = reader;
    }

    public async Task<Result<PagedResult<CommentSearchItemDto>, ErrorList>> Handle(
        CommentSearchQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _reader.SearchAsync(query, cancellationToken);

        return Result.Success<PagedResult<CommentSearchItemDto>, ErrorList>(result);
    }
}