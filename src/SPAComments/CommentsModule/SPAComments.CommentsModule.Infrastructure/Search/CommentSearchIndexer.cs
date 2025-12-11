using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SPAComments.CommentsModule.Application.Events.Integration;

namespace SPAComments.CommentsModule.Infrastructure.Search;

public sealed class CommentSearchIndexer : ICommentSearchIndexer
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;
    private readonly ILogger<CommentSearchIndexer> _logger;

    public CommentSearchIndexer(
        ElasticsearchClient client,
        IConfiguration configuration,
        ILogger<CommentSearchIndexer> logger)
    {
        _client = client;
        _logger = logger;
        _indexName = configuration["Elasticsearch:CommentsIndex"] ?? "spa-comments-comments";
    }

    public async Task IndexAsync(CommentCreatedIntegrationEvent @event, CancellationToken ct)
    {
        var doc = new CommentSearchDocument
        {
            Id = @event.CommentId,
            ParentId = @event.ParentId,
            UserName = @event.UserName,
            Email = @event.Email,
            HomePage = @event.HomePage,
            Text = @event.Text,
            CreatedAt = @event.CreatedAt,
            AttachmentIds = @event.AttachmentIds
        };

        var response = await _client.IndexAsync(doc, d => d
                .Index(_indexName)
                .Id(doc.Id),
            ct);

        if (!response.IsValidResponse)
        {
            _logger.LogError(
                "Failed to index comment {CommentId}. Details: {Details}",
                doc.Id,
                response.DebugInformation);
        }
    }

    public async Task ClearAsync(CancellationToken ct)
    {
        var response = await _client.Indices.DeleteAsync(_indexName, ct);

        if (response.IsValidResponse || response.ApiCallDetails?.HttpStatusCode == 404)
        {
            _logger.LogInformation("Cleared Elasticsearch index {IndexName}", _indexName);
            return;
        }

        _logger.LogError(
            "Failed to clear Elasticsearch index {IndexName}. Details: {Details}",
            _indexName,
            response.DebugInformation);
    }
}