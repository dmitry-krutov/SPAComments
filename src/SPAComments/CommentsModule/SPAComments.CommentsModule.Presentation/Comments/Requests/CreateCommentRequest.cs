using SPAComments.CommentsModule.Application.Features.Commands.CreateComment;
using SPAComments.Core.Mappings;

namespace SPAComments.CommentsModule.Presentation.Comments.Requests;

public class CreateCommentRequest : IMapTo<CreateCommentCommand>
{
    public Guid? ParentId { get; init; }

    public required string UserName { get; init; }

    public required string Email { get; init; }

    public string? HomePage { get; init; }

    public required string Text { get; init; }

    public required Guid CaptchaId { get; init; }

    public required string CaptchaAnswer { get; init; }

    public List<Guid>? AttachmentIds { get; init; }
}