using CSharpFunctionalExtensions;
using FileService.Communication;
using FileService.Contracts;
using SPAComments.Core.Abstractions;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Application.Features.Commands.UploadCommentAttachment;

public sealed class UploadCommentAttachmentCommandHandler
    : ICommandHandler<UploadCommentAttachmentResult, UploadCommentAttachmentCommand>
{
    private readonly IFileServiceClient _fileServiceClient;

    public UploadCommentAttachmentCommandHandler(IFileServiceClient fileServiceClient)
    {
        _fileServiceClient = fileServiceClient;
    }

    public async Task<Result<UploadCommentAttachmentResult, ErrorList>> Handle(
        UploadCommentAttachmentCommand command,
        CancellationToken cancellationToken)
    {
        var contentType = (command.ContentType ?? string.Empty).ToLowerInvariant();

        var isImage = IsAllowedImageContentType(contentType);
        var isText = IsAllowedTextContentType(contentType);

        if (!isImage && !isText)
        {
            var error = Error.Validation(
                "comments.attachments.invalid-content-type",
                $"File content type '{contentType}' is not allowed.");

            return error.ToErrorList();
        }

        if (isText && command.Length > 100 * 1024)
        {
            var error = Error.Validation(
                "comments.attachments.text-too-large",
                "Text attachments must not exceed 100 KB.");

            return error.ToErrorList();
        }

        var kind = isImage ? StoredFileKind.Image : StoredFileKind.Text;

        var meta = new UploadFileRequest
        {
            Kind = kind,
            Category = "comments",
            Resize = isImage
                ? new ImageResizeOptions { MaxWidth = 320, MaxHeight = 240, KeepAspectRatio = true }
                : null
        };

        var uploadResult = await _fileServiceClient.UploadFileAsync(
            command.Content,
            command.FileName,
            contentType,
            meta,
            cancellationToken);

        if (uploadResult.IsFailure)
            return uploadResult.Error.ToErrorList();

        var stored = uploadResult.Value;

        var result = new UploadCommentAttachmentResult
        {
            FileId = stored.Id,
            Kind = stored.Kind,
            ContentType = stored.ContentType,
            Size = stored.Size,
            Width = stored.Width,
            Height = stored.Height
        };

        return result;
    }

    private static bool IsAllowedImageContentType(string contentType) =>
        contentType is "image/png"
            or "image/jpeg"
            or "image/jpg"
            or "image/gif";

    private static bool IsAllowedTextContentType(string contentType) =>
        contentType is "text/plain" or "text/utf-8";
}