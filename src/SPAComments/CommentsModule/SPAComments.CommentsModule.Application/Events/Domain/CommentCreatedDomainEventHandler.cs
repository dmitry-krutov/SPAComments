using MassTransit;
using MediatR;
using SPAComments.CommentsModule.Application.Events.Integration;
using SPAComments.CommentsModule.Domain.Events;

namespace SPAComments.CommentsModule.Application.Events.Domain;

public class CommentCreatedDomainEventHandler
    : INotificationHandler<CommentCreatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;

    public CommentCreatedDomainEventHandler(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CommentCreatedDomainEvent notification, CancellationToken ct)
    {
        var integrationEvent = new CommentCreatedIntegrationEvent(
            notification.CommentId,
            notification.ParentId,
            notification.UserName,
            notification.Email,
            notification.HomePage,
            notification.Text,
            notification.CreatedAt,
            notification.AttachmentIds);

        await _publishEndpoint.Publish(integrationEvent, ct);
    }
}