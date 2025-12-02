using MediatR;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;

namespace SPAComments.CommentsModule.Application.Events.ApplicationEvents;

public sealed class CommentCreatedNotification : INotification
{
    public CommentCreatedNotification(CommentDto comment)
    {
        Comment = comment ?? throw new ArgumentNullException(nameof(comment));
    }

    public CommentDto Comment { get; }
}