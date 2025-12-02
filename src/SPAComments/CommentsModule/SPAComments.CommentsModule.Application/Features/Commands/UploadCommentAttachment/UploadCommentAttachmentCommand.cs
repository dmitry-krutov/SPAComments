using SPAComments.Core.Abstractions;

namespace SPAComments.CommentsModule.Application.Features.Commands.UploadCommentAttachment;

public sealed class UploadCommentAttachmentCommand : ICommand
{
    public required Stream Content { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }

    public required long Length { get; init; }
}