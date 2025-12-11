using SPAComments.CommentsModule.Application.Events.Integration;

namespace SPAComments.CommentsModule.Infrastructure.Search;

public interface ICommentSearchIndexer
{
    Task IndexAsync(CommentCreatedIntegrationEvent @event, CancellationToken ct);

    Task ClearAsync(CancellationToken ct);
}