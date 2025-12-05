using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SPAComments.CommentsModule.Application.Features.Commands.CreateComment;
using SPAComments.CommentsModule.Application.Features.Commands.UploadCommentAttachment;
using SPAComments.CommentsModule.Application.Features.Common;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;
using SPAComments.CommentsModule.Application.Features.Queries.GetLatest;
using SPAComments.CommentsModule.Application.Features.Queries.Search;
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

    [HttpPost("search")]
    public async Task<EndpointResult<PagedResult<CommentSearchItemDto>>> Search(
        [FromBody] CommentSearchQuery query,
        [FromServices] IQueryHandlerWithResult<PagedResult<CommentSearchItemDto>, CommentSearchQuery> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(query, cancellationToken);
    }

    [HttpGet]
    public async Task<EndpointResult<PagedResult<CommentDto>>> GetWithPagination(
        [FromQuery] GetLatestCommentsQuery query,
        [FromServices] IQueryHandlerWithResult<PagedResult<CommentDto>, GetLatestCommentsQuery> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(query, cancellationToken);
    }
}