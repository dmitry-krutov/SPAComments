using System.Text.Json;
using Amazon.S3;
using Amazon.S3.Model;
using FileService.Contracts;
using FileService.Endpoints;
using FileService.MongoDataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SPAComments.Framework.EndpointResults;

namespace FileService.Features;

public static class UploadFile
{
    public sealed record UploadFileForm(
        IFormFile File,
        string Meta);

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("files", Handler)
                .Accepts<IFormFile>("multipart/form-data")
                .Produces<UploadFileResponse>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest);
        }
    }

    private static async Task<EndpointResult<UploadFileResponse>> Handler(
        [FromForm] UploadFileForm form,
        IAmazonS3 s3Client,
        IFilesRepository filesRepository,
        IOptions<MinioOptions> minioOptions,
        CancellationToken cancellationToken)
    {
        if (form.File is null || string.IsNullOrWhiteSpace(form.Meta))
            return EndpointResult<UploadFileResponse>.FromErrors(Errors.Files.FailUpload().ToErrorList());

        UploadFileRequest meta;
        try
        {
            meta = JsonSerializer.Deserialize<UploadFileRequest>(
                       form.Meta,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
                   throw new InvalidOperationException();
        }
        catch
        {
            return EndpointResult<UploadFileResponse>.FromErrors(Errors.Files.FailUpload().ToErrorList());
        }

        var contentType = form.File.ContentType;

        if (meta.Kind == StoredFileKind.Image)
        {
            if (!IsAllowedImageContentType(contentType))
                return EndpointResult<UploadFileResponse>.FromErrors(Errors.Files.FailUpload().ToErrorList());
        }
        else if (meta.Kind == StoredFileKind.Text)
        {
            if (!IsAllowedTextContentType(contentType))
                return EndpointResult<UploadFileResponse>.FromErrors(Errors.Files.FailUpload().ToErrorList());
        }

        byte[] data;
        int? width = null;
        int? height = null;

        await using (var inputStream = form.File.OpenReadStream())
        {
            if (meta.Kind == StoredFileKind.Image && meta.Resize is not null)
            {
                using var image = await Image.LoadAsync(inputStream, cancellationToken);

                image.Mutate(x =>
                {
                    var maxW = meta.Resize.MaxWidth ?? image.Width;
                    var maxH = meta.Resize.MaxHeight ?? image.Height;

                    x.Resize(new ResizeOptions
                    {
                        Mode = meta.Resize.KeepAspectRatio ? ResizeMode.Max : ResizeMode.Stretch,
                        Size = new Size(maxW, maxH)
                    });
                });

                width = image.Width;
                height = image.Height;

                using var ms = new MemoryStream();
                var encoder = GetEncoder(contentType);
                await image.SaveAsync(ms, encoder, cancellationToken);
                data = ms.ToArray();

                contentType = encoder switch
                {
                    PngEncoder => "image/png",
                    JpegEncoder => "image/jpeg",
                    _ => contentType
                };
            }
            else
            {
                using var ms = new MemoryStream();
                await inputStream.CopyToAsync(ms, cancellationToken);
                data = ms.ToArray();
            }
        }

        var bucket = minioOptions.Value.Bucket;

        var category = string.IsNullOrWhiteSpace(meta.Category) ? "files" : meta.Category.Trim();
        var key = BuildObjectKey(category, id: Guid.NewGuid());

        var id = Guid.Parse(key.Split('/').Last());

        try
        {
            using var uploadStream = new MemoryStream(data);

            var putRequest = new PutObjectRequest
            {
                BucketName = bucket, Key = key, InputStream = uploadStream, ContentType = contentType
            };

            await s3Client.PutObjectAsync(putRequest, cancellationToken);
        }
        catch
        {
            return EndpointResult<UploadFileResponse>.FromErrors(Errors.Files.FailUpload().ToErrorList());
        }

        var fileData = new FileData
        {
            Id = id,
            StoragePath = key,
            UploadDate = DateTime.UtcNow,
            FileSize = data.LongLength,
            ContentType = contentType
        };

        var repoResult = await filesRepository.Add(fileData, cancellationToken);
        if (repoResult.IsFailure)
            return EndpointResult<UploadFileResponse>.FromErrors(Errors.Files.FailUpload().ToErrorList());

        var dto = new StoredFileDto
        {
            Id = id,
            Kind = meta.Kind,
            ContentType = contentType,
            Size = data.LongLength,
            Width = meta.Kind == StoredFileKind.Image ? width : null,
            Height = meta.Kind == StoredFileKind.Image ? height : null
        };

        var response = new UploadFileResponse { File = dto };

        return EndpointResult<UploadFileResponse>.Created($"/files/{id}", response);
    }

    private static bool IsAllowedImageContentType(string contentType) =>
        contentType is "image/png"
            or "image/jpeg"
            or "image/jpg"
            or "image/gif";

    private static bool IsAllowedTextContentType(string contentType) =>
        contentType is "text/plain" or "text/utf-8";

    private static IImageEncoder GetEncoder(string contentType) =>
        contentType switch
        {
            "image/jpeg" or "image/jpg" => new JpegEncoder(),
            _ => new PngEncoder()
        };

    private static string BuildObjectKey(string category, Guid id)
    {
        var now = DateTime.UtcNow;
        return $"{category}/{now:yyyy}/{now:MM}/{now:dd}/{id:N}";
    }
}