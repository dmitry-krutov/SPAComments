using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.CommentsModule.Infrastructure.DbContexts;
using SPAComments.CommentsModule.Infrastructure.Repositories;
using SPAComments.Core.Abstractions;
using SPAComments.Core.Security;

namespace SPAComments.CommentsModule.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCommentsModuleInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommentContentFiltering();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        services.AddScoped<ICommentsRepository, CommentsRepository>();

        var connectionString = configuration.GetConnectionString("CommentsDb")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'CommentsDb' is not configured.");
        services.AddDbContext<CommentsDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.UseSnakeCaseNamingConvention();
        });

        return services;
    }

    private static IServiceCollection AddCommentContentFiltering(this IServiceCollection services)
    {
        services.AddSingleton(provider =>
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Clear();
            sanitizer.AllowedTags.Add("a");
            sanitizer.AllowedTags.Add("code");
            sanitizer.AllowedTags.Add("i");
            sanitizer.AllowedTags.Add("strong");

            sanitizer.AllowedAttributes.Clear();
            sanitizer.AllowedAttributes.Add("href");
            sanitizer.AllowedAttributes.Add("title");

            sanitizer.AllowedSchemes.Clear();
            sanitizer.AllowedSchemes.Add("http");
            sanitizer.AllowedSchemes.Add("https");

            sanitizer.KeepChildNodes = true;

            return sanitizer;
        });

        services.AddSingleton<IHtmlContentFilter, GanssHtmlContentFilter>();

        return services;
    }
}