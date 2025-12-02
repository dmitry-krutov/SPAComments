using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;
using SPAComments.CommentsModule.Application.Interfaces;

namespace SPAComments.CommentsModule.Presentation.Realtime;

public sealed class CommentsRealtimeBackgroundService : BackgroundService
{
    private readonly ICommentsRealtimeQueue _queue;
    private readonly ICommentsRealtimeNotifier _notifier;
    private readonly ILogger<CommentsRealtimeBackgroundService> _logger;

    public CommentsRealtimeBackgroundService(
        ICommentsRealtimeQueue queue,
        ICommentsRealtimeNotifier notifier,
        ILogger<CommentsRealtimeBackgroundService> logger)
    {
        _queue = queue;
        _notifier = notifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (CommentDto comment in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _notifier.NotifyCommentCreatedAsync(comment, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while sending realtime notification for comment {CommentId}",
                    comment.Id);
            }
        }
    }
}