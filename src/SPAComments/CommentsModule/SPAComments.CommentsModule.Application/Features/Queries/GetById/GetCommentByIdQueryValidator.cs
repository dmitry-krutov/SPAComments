using FluentValidation;
using SPAComments.Core.Validation;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Application.Features.Queries.GetById;

public sealed class GetCommentByIdQueryValidator : AbstractValidator<GetCommentByIdQuery>
{
    public GetCommentByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithError(Error.Validation(
                "comments.get-by-id.id.required",
                "Id is required",
                "Id"));
    }
}
