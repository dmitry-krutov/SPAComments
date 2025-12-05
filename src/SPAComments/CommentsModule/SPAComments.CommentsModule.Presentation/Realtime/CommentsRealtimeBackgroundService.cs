using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;
using SPAComments.CommentsModule.Application.Interfaces;

namespace SPAComments.CommentsModule.Presentation.Realtime;

public sealed class CommentsRealtimeBackgroundService : BackgroundService
{
    private readonly ICommentsRealtimeQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CommentsRealtimeBackgroundService> _logger;

    public CommentsRealtimeBackgroundService(
        ICommentsRealtimeQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<CommentsRealtimeBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (CommentDto comment in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var notifier = scope.ServiceProvider
                    .GetRequiredService<ICommentsRealtimeNotifier>();

                await notifier.NotifyCommentCreatedAsync(comment, stoppingToken);
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