using FileService.MongoDataAccess;
using Minio;
using MongoDB.Driver;

namespace FileService;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(
        this IServiceCollection services,
        ConfigurationManager configurations)
    {
        services.AddSingleton(new MongoClient(configurations.GetConnectionString("MongoConnection")));

        services.AddScoped<FilesRepository>();

        return services;
    }

    public static IServiceCollection AddMinio(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioOptions>(
            configuration.GetSection(MinioOptions.MINIO));

        services.AddMinio(options =>
        {
            var minioOptions = configuration.GetSection(MinioOptions.MINIO).Get<MinioOptions>()
                               ?? throw new ApplicationException("Missing minio configuration");

            options.WithEndpoint(minioOptions.Endpoint);

            options.WithCredentials(minioOptions.AccessKey, minioOptions.SecretKey);
            options.WithSSL(minioOptions.WithSsl);
        });

        return services;
    }
}