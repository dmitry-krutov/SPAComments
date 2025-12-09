using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using FileService.Communication;
using SPAComments.CommentsModule.Application.Features.Common;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.Core.Abstractions;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Application.Features.Queries.GetById;

public sealed class GetCommentByIdQueryHandler
    : IQueryHandlerWithResult<CommentDto, GetCommentByIdQuery>
{
    private const int PRESIGNED_TTL_SECONDS = 300;

    private readonly ICommentsRepository _repository;
    private readonly IFileServiceClient _fileServiceClient;

    public GetCommentByIdQueryHandler(
        ICommentsRepository repository,
        IFileServiceClient fileServiceClient)
    {
        _repository = repository;
        _fileServiceClient = fileServiceClient;
    }

    public async Task<Result<CommentDto, ErrorList>> Handle(
        GetCommentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var readResult = await _repository.ReadByIdAsync(query.Id, cancellationToken);
        if (readResult.IsFailure)
            return readResult.Error.ToErrorList();

        var readModel = readResult.Value;

        var attachmentUrlMap = new Dictionary<Guid, CommentAttachmentDto>();

        if (readModel.AttachmentFileIds.Count > 0)
        {
            var presignedResult = await _fileServiceClient.GetPresignedUrlsAsync(
                readModel.AttachmentFileIds.ToArray(),
                PRESIGNED_TTL_SECONDS,
                cancellationToken);

            if (presignedResult.IsFailure)
                return presignedResult.Error.ToErrorList();

            var presigned = presignedResult.Value;

            if (presigned.Count != readModel.AttachmentFileIds.Count)
            {
                var error = Error.Validation(
                    "comments.get-by-id.attachments.not-found",
                    "Some of the attachments do not exist in file service.");
                return error.ToErrorList();
            }

            attachmentUrlMap = presigned.ToDictionary(
                p => p.FileId,
                p => new CommentAttachmentDto
                {
                    FileId = p.FileId,
                    Url = p.Url,
                    ExpiresAtUtc = p.ExpiresAtUtc
                });

            if (readModel.AttachmentFileIds.Any(id => attachmentUrlMap.ContainsKey(id) == false))
            {
                var error = Error.Validation(
                    "comments.get-by-id.attachments.not-found",
                    "Some of the attachments do not exist in file service.");
                return error.ToErrorList();
            }
        }

        var result = new CommentDto
        {
            Id = readModel.Id,
            ParentId = readModel.ParentId,
            UserName = readModel.UserName,
            Email = readModel.Email,
            HomePage = readModel.HomePage,
            Text = readModel.Text,
            CreatedAt = readModel.CreatedAt,
            Attachments = readModel.AttachmentFileIds
                .Select(id => attachmentUrlMap[id])
                .ToArray()
        };

        return Result.Success<CommentDto, ErrorList>(result);
    }
}
