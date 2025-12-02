namespace SPAComments.CommentsModule.Application.Features;

public sealed class CommentAttachmentDto
{
    public Guid FileId { get; init; }

    public string Url { get; init; } = null!;

    public DateTime ExpiresAtUtc { get; init; }
}