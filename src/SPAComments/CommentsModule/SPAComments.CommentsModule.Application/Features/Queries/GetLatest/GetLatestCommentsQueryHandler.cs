using CSharpFunctionalExtensions;
using FileService.Communication;
using SPAComments.CommentsModule.Application.Features.Common;
using SPAComments.CommentsModule.Application.Features.Common.Dtos;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.Core.Abstractions;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Application.Features.Queries.GetLatest;

public sealed class GetLatestCommentsQueryHandler
    : IQueryHandlerWithResult<PagedResult<CommentDto>, GetLatestCommentsQuery>
{
    private const int PRESIGNED_TTL_SECONDS = 300;
    private readonly ICommentsRepository _repository;
    private readonly IFileServiceClient _fileServiceClient;

    public GetLatestCommentsQueryHandler(
        ICommentsRepository repository,
        IFileServiceClient fileServiceClient)
    {
        _repository = repository;
        _fileServiceClient = fileServiceClient;
    }

    public async Task<Result<PagedResult<CommentDto>, ErrorList>> Handle(
        GetLatestCommentsQuery query,
        CancellationToken cancellationToken)
    {
        var readResult = await _repository.ReadLatestAsync(query, cancellationToken);

        var attachmentIds = readResult.Items
            .SelectMany(c => c.AttachmentFileIds)
            .Distinct()
            .ToArray();

        var attachmentUrlMap = new Dictionary<Guid, CommentAttachmentDto>();

        if (attachmentIds.Length > 0)
        {
            var presignedResult = await _fileServiceClient.GetPresignedUrlsAsync(
                attachmentIds,
                PRESIGNED_TTL_SECONDS,
                cancellationToken);

            if (presignedResult.IsFailure)
                return presignedResult.Error.ToErrorList();

            var presigned = presignedResult.Value;

            if (presigned.Count != attachmentIds.Length)
            {
                var error = Error.Validation(
                    "comments.latest.attachments.not-found",
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

            if (attachmentIds.Any(id => attachmentUrlMap.ContainsKey(id) == false))
            {
                var error = Error.Validation(
                    "comments.latest.attachments.not-found",
                    "Some of the attachments do not exist in file service.");
                return error.ToErrorList();
            }
        }

        var items = readResult.Items
            .Select(comment => new CommentDto
            {
                Id = comment.Id,
                ParentId = comment.ParentId,
                UserName = comment.UserName,
                Email = comment.Email,
                HomePage = comment.HomePage,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt,
                Attachments = comment.AttachmentFileIds
                    .Select(id => attachmentUrlMap[id])
                    .ToArray()
            })
            .ToArray();

        var result = new PagedResult<CommentDto>
        {
            Items = items,
            TotalCount = readResult.TotalCount,
            Page = readResult.Page,
            PageSize = readResult.PageSize
        };

        return Result.Success<PagedResult<CommentDto>, ErrorList>(result);
    }
}
