using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SPAComments.CommentsModule.Application.Features.Commands.CreateComment;
using SPAComments.CommentsModule.Application.Features.Commands.UploadCommentAttachment;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;
using SPAComments.CommentsModule.Presentation.Comments.Requests;
using SPAComments.Core.Abstractions;
using SPAComments.Framework;
using SPAComments.Framework.EndpointResults;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Presentation.Comments;

public class CommentsController(IMapper mapper) : ApplicationController
{
    [HttpPost]
    public async Task<EndpointResult<CommentDto>> AddComment(
        [FromBody] CreateCommentRequest request,
        [FromServices] ICommandHandler<CommentDto, CreateCommentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CreateCommentCommand>(request);
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPost("attachments")]
    public async Task<EndpointResult<UploadCommentAttachmentResult>> UploadAttachment(
        [FromForm] CreateCommentAttachmentRequest request,
        [FromServices] ICommandHandler<UploadCommentAttachmentResult, UploadCommentAttachmentCommand> handler,
        CancellationToken cancellationToken)
    {
        if (request.File is null)
        {
            var error = GeneralErrors.Validation.ValueIsRequired("file");
            return EndpointResult<UploadCommentAttachmentResult>.FromErrors(error.ToErrorList());
        }

        await using var processor = new FormFileProcessor();
        var uploadFile = processor.Process(request.File);

        var command = new UploadCommentAttachmentCommand
        {
            Content = uploadFile.Content,
            FileName = uploadFile.FileName,
            ContentType = uploadFile.ContentType,
            Length = uploadFile.Length,
        };

        return await handler.Handle(command, cancellationToken);
    }
}