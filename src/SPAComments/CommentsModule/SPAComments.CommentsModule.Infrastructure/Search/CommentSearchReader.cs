using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Configuration;
using SPAComments.CommentsModule.Application.Features.Common;
using SPAComments.CommentsModule.Application.Features.Queries.Search;

namespace SPAComments.CommentsModule.Infrastructure.Search;

public sealed class CommentSearchReader : ICommentSearchReader
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public CommentSearchReader(ElasticsearchClient client, IConfiguration configuration)
    {
        _client = client;
        _indexName = configuration["Elasticsearch:CommentsIndex"] ?? "spa-comments-comments";
    }

    public async Task<PagedResult<CommentSearchItemDto>> SearchAsync(
        CommentSearchQuery query,
        CancellationToken ct = default)
    {
        var from = (query.Page - 1) * query.PageSize;
        if (from < 0) from = 0;

        var mustClauses = new List<Query>();

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            mustClauses.Add(new MatchQuery { Field = "text", Query = query.Text });
        }

        if (!string.IsNullOrWhiteSpace(query.UserName))
        {
            mustClauses.Add(new MatchQuery { Field = "userName", Query = query.UserName });
        }

        Query esQuery;

        if (mustClauses.Count > 0)
        {
            esQuery = new BoolQuery { Must = mustClauses };
        }
        else
        {
            esQuery = new MatchAllQuery();
        }

        var searchResponse = await _client.SearchAsync<CommentSearchDocument>(
            s => s
                .Indices(_indexName)
                .From(from)
                .Size(query.PageSize)
                .Query(esQuery)
                .Sort(srt =>
                {
                    if (string.Equals(query.SortBy, "userName", StringComparison.OrdinalIgnoreCase))
                    {
                        if (query.SortDesc)
                            srt.Field(f => f.Field("userName").Order(SortOrder.Desc));
                        else
                            srt.Field(f => f.Field("userName").Order(SortOrder.Asc));
                    }
                    else
                    {
                        if (query.SortDesc)
                            srt.Field(f => f.Field("createdAt").Order(SortOrder.Desc));
                        else
                            srt.Field(f => f.Field("createdAt").Order(SortOrder.Asc));
                    }
                }),
            ct);

        if (!searchResponse.IsValidResponse)
        {
            return new PagedResult<CommentSearchItemDto>
            {
                Items = Array.Empty<CommentSearchItemDto>(),
                TotalCount = 0,
                Page = query.Page,
                PageSize = query.PageSize
            };
        }

        var items = searchResponse.Hits
            .Where(h => h.Source is not null)
            .Select(hit => new CommentSearchItemDto
            {
                Id = hit.Source!.Id,
                UserName = hit.Source.UserName,
                Text = hit.Source.Text,
                CreatedAt = hit.Source.CreatedAt
            })
            .ToArray();

        var total = searchResponse.Total;

        return new PagedResult<CommentSearchItemDto>
        {
            Items = items, TotalCount = total, Page = query.Page, PageSize = query.PageSize
        };
    }
}