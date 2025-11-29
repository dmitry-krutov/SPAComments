using CSharpFunctionalExtensions;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.CommentsModule.Domain;
using SPAComments.Core.Abstractions;
using SPAComments.SharedKernel;
using SPAComments.SharedKernel.ValueObjects.Ids;

namespace SPAComments.CommentsModule.Application.Features.Commands.CreateComment;

public class CreateCommentCommandHandler : ICommandHandler<Guid, CreateCommentCommand>
{
    private readonly ICommentsRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateCommentCommandHandler(
        ICommentsRepository repository,
        IDateTimeProvider dateTimeProvider)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        CreateCommentCommand command,
        CancellationToken cancellationToken)
    {
        var comment = new Comment(
            CommentId.NewId(),
            command.ParentIdVo,
            command.UserNameVo,
            command.EmailVo,
            command.HomePageVo,
            command.TextVo,
            _dateTimeProvider.UtcNow);

        await _repository.Add(comment, cancellationToken);

        return comment.Id.Value;
    }
}