using FluentValidation;
using SPAComments.Core.Validation;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Application.Features.Queries.GetLatest;

public class GetLatestCommentsQueryValidator : AbstractValidator<GetLatestCommentsQuery>
{
    public GetLatestCommentsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithError(Error.Validation(
                "comments.latest.page.min",
                "Page must be greater than zero",
                "Page"));

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithError(Error.Validation(
                "comments.latest.pageSize.range",
                "PageSize must be between 1 and 100",
                "PageSize"));
    }
}
