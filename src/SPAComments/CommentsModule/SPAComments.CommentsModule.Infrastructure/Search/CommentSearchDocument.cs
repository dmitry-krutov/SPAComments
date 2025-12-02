namespace SPAComments.CommentsModule.Infrastructure.Search;

public class CommentSearchDocument
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? HomePage { get; set; }

    public string Text { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public IReadOnlyCollection<Guid> AttachmentIds { get; set; } = Array.Empty<Guid>();
}