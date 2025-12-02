using SPAComments.CommentsModule.Application.Features.Common;

namespace SPAComments.CommentsModule.Application.Features.Queries.Search;

public interface ICommentSearchReader
{
    Task<PagedResult<CommentSearchItemDto>> SearchAsync(
        CommentSearchQuery query,
        CancellationToken ct = default);
}
