using CSharpFunctionalExtensions;
using FluentValidation;
using SPAComments.CommentsModule.Domain.ValueObjects;
using SPAComments.Core.Validation;
using SPAComments.SharedKernel;
using SPAComments.SharedKernel.ValueObjects.Ids;

namespace SPAComments.CommentsModule.Application.Features.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.ParentId)
            .MustBeValueObject(CommentId.TryCreate, (cmd, vo) => cmd.ParentIdVo = vo)
            .When(x => x.ParentId is not null);

        RuleFor(x => x.UserName)
            .MustBeValueObject(UserName.Create, (cmd, vo) => cmd.UserNameVo = vo);

        RuleFor(x => x.Email)
            .MustBeValueObject(Email.Create, (cmd, vo) => cmd.EmailVo = vo);

        RuleFor(x => x.HomePage)
            .MustBeValueObject(HomePage.Create!, (cmd, vo) => cmd.HomePageVo = vo)
            .When(x => !string.IsNullOrWhiteSpace(x.HomePage));

        RuleFor(x => x.Text)
            .MustBeValueObject(Text.Create, (cmd, vo) => cmd.TextVo = vo);
    }
}