using System.Threading.Channels;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;
using SPAComments.CommentsModule.Application.Interfaces;

namespace SPAComments.CommentsModule.Presentation.Realtime;

public sealed class CommentsRealtimeQueue : ICommentsRealtimeQueue
{
    private readonly Channel<CommentDto> _channel;

    public CommentsRealtimeQueue()
    {
        var options = new BoundedChannelOptions(1000)
        {
            SingleReader = true, SingleWriter = false, FullMode = BoundedChannelFullMode.Wait
        };

        _channel = Channel.CreateBounded<CommentDto>(options);
    }

    public ValueTask EnqueueAsync(CommentDto comment, CancellationToken cancellationToken = default)
        => _channel.Writer.WriteAsync(comment, cancellationToken);

    public IAsyncEnumerable<CommentDto> ReadAllAsync(CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);
}