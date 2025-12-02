using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Application.Features.Queries.Search;

public static class CommentSearchErrors
{
    public static readonly Error PageMustBeGreaterThanZero =
        Error.Validation("comments.search.page.min", "Page must be greater than zero", "Page");

    public static readonly Error PageSizeOutOfRange =
        Error.Validation("comments.search.pageSize.range", "PageSize must be between 1 and 100", "PageSize");
}