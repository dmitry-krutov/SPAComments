using MassTransit;
using SPAComments.CommentsModule.Application.Events.Integration;
using SPAComments.CommentsModule.Infrastructure.Search;

namespace SPAComments.CommentsModule.Infrastructure.Messaging;

public class CommentCreatedIntegrationEventConsumer
    : IConsumer<CommentCreatedIntegrationEvent>
{
    private readonly ICommentSearchIndexer _indexer;

    public CommentCreatedIntegrationEventConsumer(ICommentSearchIndexer indexer)
    {
        _indexer = indexer;
    }

    public Task Consume(ConsumeContext<CommentCreatedIntegrationEvent> context)
    {
        return _indexer.IndexAsync(context.Message, context.CancellationToken);
    }
}