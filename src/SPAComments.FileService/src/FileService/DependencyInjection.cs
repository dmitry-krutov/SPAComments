using Amazon.S3;
using FileService.MongoDataAccess;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FileService;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mongoConnectionString = configuration.GetConnectionString("Mongo")
                                    ?? throw new ApplicationException("Missing Mongo connection string");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnectionString));
        services.AddScoped<FileMongoDbContext>();
        services.AddScoped<IFilesRepository, FilesRepository>();

        return services;
    }

    public static IServiceCollection AddStorage(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MinioOptions>(
            configuration.GetSection(MinioOptions.MINIO));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var minioOptions = sp.GetRequiredService<IOptions<MinioOptions>>().Value;

            var s3Config = new AmazonS3Config
            {
                ServiceURL = minioOptions.Endpoint, ForcePathStyle = true, UseHttp = !minioOptions.WithSsl
            };

            return new AmazonS3Client(
                minioOptions.AccessKey,
                minioOptions.SecretKey,
                s3Config);
        });

        return services;
    }
}