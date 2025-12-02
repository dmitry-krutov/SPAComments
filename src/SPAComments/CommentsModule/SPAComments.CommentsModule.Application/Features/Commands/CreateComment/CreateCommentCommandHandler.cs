using CSharpFunctionalExtensions;
using FileService.Communication;
using MediatR;
using SPAComments.CaptchaModule.Application;
using SPAComments.CaptchaModule.Application.Services;
using SPAComments.CommentsModule.Application.Events;
using SPAComments.CommentsModule.Application.Events.ApplicationEvents;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.CommentsModule.Domain;
using SPAComments.CommentsModule.Domain.ValueObjects;
using SPAComments.Core.Abstractions;
using SPAComments.SharedKernel;
using SPAComments.SharedKernel.ValueObjects.Ids;

namespace SPAComments.CommentsModule.Application.Features.Commands.CreateComment;

public class CreateCommentCommandHandler : ICommandHandler<CommentDto, CreateCommentCommand>
{
    private const int PRESIGNED_TTL_SECONDS = 300;
    private readonly ICommentsRepository _repository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICaptchaService _captchaService;
    private readonly IFileServiceClient _fileServiceClient;
    private readonly IPublisher _publisher;

    public CreateCommentCommandHandler(
        ICommentsRepository repository,
        IDateTimeProvider dateTimeProvider,
        ICaptchaService captchaService,
        IFileServiceClient fileServiceClient,
        IPublisher publisher)
    {
        _repository = repository;
        _dateTimeProvider = dateTimeProvider;
        _captchaService = captchaService;
        _fileServiceClient = fileServiceClient;
        _publisher = publisher;
    }

    public async Task<Result<CommentDto, ErrorList>> Handle(
        CreateCommentCommand command,
        CancellationToken cancellationToken)
    {
        var captchaIsValid =
            await _captchaService.ValidateAsync(command.CaptchaId, command.CaptchaAnswer, cancellationToken);
        if (captchaIsValid == false)
            return CaptchaErrors.CaptchaInvalid.ToErrorList();

        var attachmentIds = (command.AttachmentIds ?? Array.Empty<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();

        var attachmentVoList = new List<CommentAttachment>();

        foreach (var id in attachmentIds)
        {
            var voResult = CommentAttachment.Create(id);
            if (voResult.IsFailure)
                return voResult.Error.ToErrorList();

            attachmentVoList.Add(voResult.Value);
        }

        var presignedList = Array.Empty<CommentAttachmentDto>();

        if (attachmentVoList.Count > 0)
        {
            var ids = attachmentVoList.Select(a => a.FileId).ToArray();

            var presignedResult = await _fileServiceClient.GetPresignedUrlsAsync(
                ids,
                PRESIGNED_TTL_SECONDS,
                cancellationToken);

            if (presignedResult.IsFailure)
                return presignedResult.Error.ToErrorList();

            var presigned = presignedResult.Value;

            if (presigned.Count != ids.Length)
            {
                var error = Error.Validation(
                    "comments.attachments.not-found",
                    "Some of the attachments do not exist in file service.");
                return error.ToErrorList();
            }

            presignedList = presigned
                .Select(x => new CommentAttachmentDto()
                {
                    FileId = x.FileId, Url = x.Url, ExpiresAtUtc = x.ExpiresAtUtc
                })
                .ToArray();
        }

        var comment = new Comment(
            CommentId.NewId(),
            command.ParentIdVo,
            command.UserNameVo,
            command.EmailVo,
            command.HomePageVo,
            command.TextVo,
            _dateTimeProvider.UtcNow,
            attachmentVoList);

        await _repository.Add(comment, cancellationToken);

        var result = new CommentDto
        {
            Id = comment.Id.Value,
            ParentId = comment.ParentCommentId?.Value,
            UserName = comment.UserName.Value,
            Email = comment.Email.Value,
            HomePage = comment.HomePage?.Value,
            Text = comment.Text.Value,
            CreatedAt = comment.CreatedAt,
            Attachments = presignedList
        };

        await _publisher.Publish(
            new CommentCreatedNotification(result),
            cancellationToken);

        return result;
    }
}