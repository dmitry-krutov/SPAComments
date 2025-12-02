using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using Microsoft.Extensions.Options;
using SPAComments.SharedKernel;

namespace FileService.Communication;

public sealed class FileServiceClient : IFileServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public FileServiceClient(HttpClient httpClient, IOptions<FileServiceClientOptions> options)
    {
        _httpClient = httpClient;

        var cfg = options.Value;
        if (string.IsNullOrWhiteSpace(cfg.BaseAddress))
            throw new ApplicationException("FileServiceClientOptions.BaseAddress is not configured");

        _httpClient.BaseAddress = new Uri(cfg.BaseAddress, UriKind.Absolute);
        _httpClient.Timeout = cfg.Timeout;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase, PropertyNameCaseInsensitive = true
        };
    }

    public async Task<Result<StoredFileDto, Error>> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        UploadFileRequest meta,
        CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();

        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var metaJson = JsonSerializer.Serialize(meta, _jsonOptions);
        var metaContent = new StringContent(metaJson, Encoding.UTF8, "application/json");
        content.Add(metaContent, "meta");

        using var request = new HttpRequestMessage(HttpMethod.Post, "files") { Content = content };

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return FailHttp("file-service.upload", response.StatusCode);

        var bodyStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var uploadResponse = await JsonSerializer.DeserializeAsync<UploadFileResponse>(
            bodyStream,
            _jsonOptions,
            cancellationToken);

        if (uploadResponse is null || uploadResponse.File is null)
            return Error.Internal("file-service.upload.parse", "Failed to parse upload response");

        return uploadResponse.File;
    }

    public async Task<Result<IReadOnlyCollection<FilePresignedUrlDto>, Error>> GetPresignedUrlsAsync(
        IReadOnlyCollection<Guid> fileIds,
        int ttlSeconds,
        CancellationToken cancellationToken)
    {
        if (fileIds == null || fileIds.Count == 0 || ttlSeconds <= 0)
            return Error.Validation("file-service.presigned.invalid-request", "Invalid presigned url request");

        var requestDto = new GetFilesPresignedUrlRequest { FileIds = fileIds, TtlSeconds = ttlSeconds };

        var json = JsonSerializer.Serialize(requestDto, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "files/presigned") { Content = content };

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return FailHttp("file-service.presigned", response.StatusCode);

        var bodyStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var presignedResponse = await JsonSerializer.DeserializeAsync<GetFilesPresignedUrlResponse>(
            bodyStream,
            _jsonOptions,
            cancellationToken);

        if (presignedResponse is null)
            return Error.Internal("file-service.presigned.parse", "Failed to parse presigned urls response");

        var files = presignedResponse.Files.ToArray();
        return Result.Success<IReadOnlyCollection<FilePresignedUrlDto>, Error>(files);
    }

    private static Error FailHttp(string codePrefix, HttpStatusCode statusCode)
    {
        var code = $"{codePrefix}.{(int)statusCode}";
        var message = $"File service responded with HTTP {(int)statusCode} ({statusCode}).";
        return Error.Failure(code, message);
    }
}