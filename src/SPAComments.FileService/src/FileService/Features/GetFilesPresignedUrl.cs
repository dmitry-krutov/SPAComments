using FileService.Contracts;
using FileService.Endpoints;
using FileService.MongoDataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SPAComments.Framework.EndpointResults;

namespace FileService.Features;

public static class GetFilesPresignedUrl
{
    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("files/presigned", Handler)
                .Produces<GetFilesPresignedUrlResponse>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest);
        }

        private static async Task<EndpointResult<GetFilesPresignedUrlResponse>> Handler(
            [FromBody] GetFilesPresignedUrlRequest request,
            IFilesRepository filesRepository,
            IOptions<MinioOptions> minioOptions,
            CancellationToken cancellationToken)
        {
            if (request.FileIds is null || request.FileIds.Count == 0 || request.TtlSeconds <= 0)
            {
                return EndpointResult<GetFilesPresignedUrlResponse>
                    .FromErrors(Errors.Files.FailGetPresignedUrl().ToErrorList());
            }

            var options = minioOptions.Value;
            var bucket = options.Bucket;
            var publicBaseUrl = options.PublicBaseUrl.TrimEnd('/');

            var files = await filesRepository.Get(request.FileIds, cancellationToken);

            if (files.Count == 0)
            {
                return EndpointResult<GetFilesPresignedUrlResponse>
                    .FromErrors(Errors.Files.FailGetPresignedUrl().ToErrorList());
            }

            var result = new List<FilePresignedUrlDto>(files.Count);

            foreach (var file in files)
            {
                var url = $"{publicBaseUrl}/{bucket}/{file.StoragePath}";

                result.Add(new FilePresignedUrlDto { FileId = file.Id, Url = url, ExpiresAtUtc = DateTime.MaxValue });
            }

            var response = new GetFilesPresignedUrlResponse { Files = result };

            return EndpointResult<GetFilesPresignedUrlResponse>.Ok(response);
        }
    }
}