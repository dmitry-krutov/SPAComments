using Microsoft.AspNetCore.Http;

namespace SPAComments.CommentsModule.Presentation.Comments.Requests;

public sealed class CreateCommentAttachmentRequest
{
    public IFormFile File { get; init; } = null!;
}