using SPAComments.Core.Abstractions;

namespace SPAComments.CommentsModule.Application.Features.Queries.Search;

public sealed class CommentSearchQuery : IQuery
{
    public string? Text { get; init; }

    public string? UserName { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;

    public string? SortBy { get; init; } = "createdAt";

    public bool SortDesc { get; init; } = true;
}