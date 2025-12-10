using System;
using System.Linq;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using SPAComments.CommentsModule.Application.Features.Common;
using SPAComments.CommentsModule.Application.Features.Queries.GetById;
using SPAComments.CommentsModule.Application.Features.Queries.GetLatest;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.CommentsModule.Domain;
using SPAComments.CommentsModule.Infrastructure.DbContexts;
using SPAComments.SharedKernel;
using SPAComments.SharedKernel.ValueObjects.Ids;

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

        // 1. Пагинация только по корневым комментариям
        var rootsQuery = _context.CommentsQueryable
            .AsNoTracking()
            .Where(c => c.ParentCommentId == null)
            .OrderByDescending(c => c.CreatedAt);

        var totalCount = await rootsQuery.LongCountAsync(cancellationToken);

        var rootComments = await rootsQuery
            .Skip(from)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        if (rootComments.Count == 0)
        {
            return new PagedResult<LatestCommentReadModel>
            {
                Items = Array.Empty<LatestCommentReadModel>(),
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        // 2. Заберём всех некорневых детей одним запросом
        var allNonRootComments = await _context.CommentsQueryable
            .AsNoTracking()
            .Where(c => c.ParentCommentId != null)
            .ToListAsync(cancellationToken);

        // 3. Построим lookup parentId -> children
        var childrenByParentId = allNonRootComments
            .GroupBy(c => c.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 4. BFS/DFS по дереву только для выбранных корней
        var allCommentsForPage = new List<Comment>(rootComments);
        var queue = new Queue<Guid>(rootComments.Select(r => r.Id.Value));

        while (queue.Count > 0)
        {
            var parentId = queue.Dequeue();

            if (!childrenByParentId.TryGetValue(parentId, out var children))
                continue;

            foreach (var child in children)
            {
                allCommentsForPage.Add(child);
                queue.Enqueue(child.Id.Value);
            }
        }

        // 5. Сортировка и маппинг в ReadModel
        var items = allCommentsForPage
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new LatestCommentReadModel
            {
                Id = c.Id.Value,
                ParentId = c.ParentCommentId != null ? c.ParentCommentId.Value : (Guid?)null,
                UserName = c.UserName.Value,
                Email = c.Email.Value,
                HomePage = c.HomePage != null ? c.HomePage.Value : null,
                Text = c.Text.Value,
                CreatedAt = c.CreatedAt,
                AttachmentFileIds = c.Attachments.Select(a => a.FileId).ToArray()
            })
            .ToArray();

        return new PagedResult<LatestCommentReadModel>
        {
            Items = items,
            TotalCount = totalCount, // именно количество корней
            Page = query.Page,
            PageSize = query.PageSize
        };
    }


    public async Task<Result<CommentReadModel, Error>> ReadByIdAsync(
        CommentId id,
        CancellationToken cancellationToken)
    {
        var comment = await _context.CommentsQueryable
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CommentReadModel
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
            .FirstOrDefaultAsync(cancellationToken);

        if (comment is null)
        {
            var error = Error.NotFound(
                "comments.get-by-id.not-found",
                "Comment not found");
            return Result.Failure<CommentReadModel, Error>(error);
        }

        return comment;
    }
}