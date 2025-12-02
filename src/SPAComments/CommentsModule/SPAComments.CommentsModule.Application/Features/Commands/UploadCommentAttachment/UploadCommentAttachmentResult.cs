using FileService.Contracts;

namespace SPAComments.CommentsModule.Application.Features.Commands.UploadCommentAttachment;

public sealed class UploadCommentAttachmentResult
{
    public Guid FileId { get; init; }

    public StoredFileKind Kind { get; init; }

    public string ContentType { get; init; } = null!;

    public long Size { get; init; }

    public int? Width { get; init; }

    public int? Height { get; init; }
}
