using SPAComments.CommentsModule.Domain.ValueObjects;
using SPAComments.Core.Abstractions;
using SPAComments.SharedKernel.ValueObjects.Ids;

namespace SPAComments.CommentsModule.Application.Features.Commands.CreateComment;

public class CreateCommentCommand : ICommand
{
    public required Guid? ParentId { get; init; }

    public required string UserName { get; init; }

    public required string Email { get; init; }

    public string? HomePage { get; init; }

    public required string Text { get; init; }


    public CommentId? ParentIdVo { get; set; }

    public UserName UserNameVo { get; set; } = null!;

    public Email EmailVo { get; set; } = null!;

    public HomePage? HomePageVo { get; set; } = null!;

    public Text TextVo { get; set; } = null!;
}