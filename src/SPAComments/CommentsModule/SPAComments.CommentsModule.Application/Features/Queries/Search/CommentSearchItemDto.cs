namespace SPAComments.CommentsModule.Application.Features.Queries.Search;

public class CommentSearchItemDto
{
    public Guid Id { get; init; }

    public string UserName { get; init; } = null!;

    public string Text { get; init; } = null!;

    public DateTime CreatedAt { get; init; }
}