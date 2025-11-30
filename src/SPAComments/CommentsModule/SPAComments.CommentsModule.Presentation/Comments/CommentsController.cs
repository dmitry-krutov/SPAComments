using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SPAComments.CommentsModule.Application.Features.Commands.CreateComment;
using SPAComments.CommentsModule.Presentation.Comments.Requests;
using SPAComments.Core.Abstractions;
using SPAComments.Framework;
using SPAComments.Framework.EndpointResults;

namespace SPAComments.CommentsModule.Presentation.Comments;

public class CommentsController(IMapper mapper) : ApplicationController
{
    [HttpPost]
    public async Task<EndpointResult<Guid>> AddComment(
        [FromBody] CreateCommentRequest request,
        [FromServices] ICommandHandler<Guid, CreateCommentCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = mapper.Map<CreateCommentCommand>(request);
        return await handler.Handle(command, cancellationToken);
    }
}