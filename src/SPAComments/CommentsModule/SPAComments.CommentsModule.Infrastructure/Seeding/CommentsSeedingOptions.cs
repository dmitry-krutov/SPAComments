namespace SPAComments.CommentsModule.Infrastructure.Seeding;

public sealed class CommentsSeedingOptions
{
    public bool EnableSeeding { get; set; }

    public bool ClearCommentsBeforeSeeding { get; set; }

    public bool ClearElasticsearchBeforeSeeding { get; set; }
}