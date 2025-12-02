using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.Communication;

public static class DependencyInjection
{
    public static IServiceCollection AddFileServiceClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<FileServiceClientOptions>(
            configuration.GetSection(FileServiceClientOptions.SECTION_NAME));

        services.AddHttpClient<IFileServiceClient, FileServiceClient>();

        return services;
    }
}