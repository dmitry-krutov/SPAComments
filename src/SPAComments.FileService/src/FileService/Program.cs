using Amazon.S3;
using FileService;
using FileService.Endpoints;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRepositories(builder.Configuration)
    .AddStorage(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpoints();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Docker"))
{
    app.UseHttpsRedirection();
}
app.MapEndpoints();

await EnsureBucketExists(app.Services);

app.Run();

static async Task EnsureBucketExists(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var s3Client = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
    var options = scope.ServiceProvider.GetRequiredService<IOptions<MinioOptions>>().Value;

    var bucketExists = await s3Client.DoesS3BucketExistAsync(options.Bucket);
    if (!bucketExists)
    {
        await s3Client.PutBucketAsync(options.Bucket);
    }
}
