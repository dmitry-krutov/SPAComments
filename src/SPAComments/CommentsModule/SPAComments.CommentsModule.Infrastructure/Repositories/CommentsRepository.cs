using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SPAComments.CommentsModule.Application.Features.Common;
using SPAComments.CommentsModule.Application.Features.Queries.GetLatest;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.CommentsModule.Domain;
using SPAComments.CommentsModule.Infrastructure.DbContexts;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Infrastructure.Repositories;

public class CommentsRepository : ICommentsRepository
{
    private readonly CommentsDbContext _context;

    public CommentsRepository(CommentsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid, Error>> Add(Comment comment, CancellationToken cancellationToken)
    {
        await _context.Comments.AddAsync(comment, cancellationToken);
        return comment.Id.Value;
    }

    public async Task<PagedResult<LatestCommentReadModel>> ReadLatestAsync(
        GetLatestCommentsQuery query,
        CancellationToken cancellationToken)
    {
        var from = (query.Page - 1) * query.PageSize;
        if (from < 0)
            from = 0;

        var baseQuery = _context.CommentsQueryable
            .AsNoTracking()
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await baseQuery.LongCountAsync(cancellationToken);

        var items = await baseQuery
            .Skip(from)
            .Take(query.PageSize)
            .Select(c => new LatestCommentReadModel
            {
                Id = c.Id.Value,
                ParentId = c.ParentCommentId != null ? c.ParentCommentId.Value : null,
                UserName = c.UserName.Value,
                Email = c.Email.Value,
                HomePage = c.HomePage != null ? c.HomePage.Value : null,
                Text = c.Text.Value,
                CreatedAt = c.CreatedAt,
                AttachmentFileIds = c.Attachments.Select(a => a.FileId).ToArray()
            })
            .ToArrayAsync(cancellationToken);

        return new PagedResult<LatestCommentReadModel>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }
}