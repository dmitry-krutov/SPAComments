using SPAComments.Core.Abstractions;

namespace SPAComments.CommentsModule.Application.Features.Queries.GetLatest;

public sealed class GetLatestCommentsQuery : IQuery
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
