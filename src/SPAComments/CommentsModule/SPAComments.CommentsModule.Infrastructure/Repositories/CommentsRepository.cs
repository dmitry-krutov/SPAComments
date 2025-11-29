using CSharpFunctionalExtensions;
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
}