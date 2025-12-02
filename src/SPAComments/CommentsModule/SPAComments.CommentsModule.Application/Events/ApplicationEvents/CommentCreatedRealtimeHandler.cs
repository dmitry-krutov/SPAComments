using MediatR;
using SPAComments.CommentsModule.Application.Interfaces;

namespace SPAComments.CommentsModule.Application.Events.ApplicationEvents;

public sealed class CommentCreatedRealtimeHandler
    : INotificationHandler<CommentCreatedNotification>
{
    private readonly ICommentsRealtimeQueue _queue;

    public CommentCreatedRealtimeHandler(ICommentsRealtimeQueue queue)
    {
        _queue = queue;
    }

    public Task Handle(CommentCreatedNotification notification, CancellationToken cancellationToken)
    {
        return _queue.EnqueueAsync(notification.Comment, cancellationToken).AsTask();
    }
}