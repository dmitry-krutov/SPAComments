namespace SPAComments.CommentsModule.Application.Features.Queries.GetLatest;

public sealed class LatestCommentReadModel
{
    public Guid Id { get; init; }

    public Guid? ParentId { get; init; }

    public string UserName { get; init; } = null!;

    public string Email { get; init; } = null!;

    public string? HomePage { get; init; }

    public string Text { get; init; } = null!;

    public DateTime CreatedAt { get; init; }

    public IReadOnlyCollection<Guid> AttachmentFileIds { get; init; }
        = Array.Empty<Guid>();
}
