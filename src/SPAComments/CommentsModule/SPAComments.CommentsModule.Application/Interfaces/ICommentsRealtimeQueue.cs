using SPAComments.CommentsModule.Application.Features.Common.Dtos;

namespace SPAComments.CommentsModule.Application.Interfaces;

public interface ICommentsRealtimeQueue
{
    ValueTask EnqueueAsync(CommentDto comment, CancellationToken cancellationToken = default);

    IAsyncEnumerable<CommentDto> ReadAllAsync(CancellationToken cancellationToken = default);
}