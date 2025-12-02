using Microsoft.AspNetCore.SignalR;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.CommentsModule.Presentation.Hubs;

namespace SPAComments.CommentsModule.Presentation.Realtime;

public sealed class SignalRCommentsRealtimeNotifier : ICommentsRealtimeNotifier
{
    private readonly IHubContext<CommentsHub> _hubContext;

    public SignalRCommentsRealtimeNotifier(IHubContext<CommentsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyCommentCreatedAsync(
        CommentDto comment,
        CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All
            .SendAsync("CommentCreated", comment, cancellationToken);
    }
}

