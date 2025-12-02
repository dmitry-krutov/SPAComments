namespace SPAComments.CommentsModule.Application.Features.Commands.CreateComment;

public sealed class CreateCommentResult
{
    public Guid Id { get; init; }

    public Guid? ParentId { get; init; }

    public string UserName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? HomePage { get; init; }

    public string Text { get; init; } = null!;

    public DateTime CreatedAt { get; init; }

    public IReadOnlyCollection<CommentAttachmentDto> Attachments { get; init; }
        = Array.Empty<CommentAttachmentDto>();
}