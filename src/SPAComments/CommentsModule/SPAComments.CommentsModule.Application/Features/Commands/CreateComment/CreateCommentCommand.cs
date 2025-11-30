using SPAComments.CommentsModule.Application.Common;
using SPAComments.CommentsModule.Domain.ValueObjects;
using SPAComments.Core.Abstractions;
using SPAComments.SharedKernel.ValueObjects.Ids;

namespace SPAComments.CommentsModule.Application.Features.Commands.CreateComment;

public class CreateCommentCommand : ICommand, IHasCommentText
{
    public required Guid? ParentId { get; init; }

    public required string UserName { get; init; }

    public required string Email { get; init; }

    public string? HomePage { get; init; }

    public required string Text { get; set; }

    public required Guid CaptchaId { get; init; }

    public required string CaptchaAnswer { get; init; }


    public CommentId? ParentIdVo { get; set; }

    public UserName UserNameVo { get; set; } = null!;

    public Email EmailVo { get; set; } = null!;

    public HomePage? HomePageVo { get; set; } = null!;

    public Text TextVo { get; set; } = null!;
}