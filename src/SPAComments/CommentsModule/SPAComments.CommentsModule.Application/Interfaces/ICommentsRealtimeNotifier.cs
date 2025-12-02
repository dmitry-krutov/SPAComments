using SPAComments.CommentsModule.Application.Features.Commands.CreateComment;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;

namespace SPAComments.CommentsModule.Application.Interfaces;

public interface ICommentsRealtimeNotifier
{
    Task NotifyCommentCreatedAsync(
        CommentDto comment,
        CancellationToken cancellationToken = default);
}