using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SPAComments.CommentsModule.Domain;

namespace SPAComments.CommentsModule.Infrastructure.DbContexts;

public class CommentsDbContext : DbContext
{
    public CommentsDbContext(DbContextOptions<CommentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("issues");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CommentsDbContext).Assembly);
    }

    private ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(builder => { builder.AddConsole(); });
}