using FluentValidation;
using SPAComments.Core.Validation;

namespace SPAComments.CommentsModule.Application.Features.Queries.Search;

public class CommentSearchQueryValidator : AbstractValidator<CommentSearchQuery>
{
    public CommentSearchQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithError(CommentSearchErrors.PageMustBeGreaterThanZero);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithError(CommentSearchErrors.PageSizeOutOfRange);

        RuleFor(x => x.Text)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Text));

        RuleFor(x => x.UserName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));
    }
}