using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SPAComments.CommentsModule.Application.Features.Commands.CreateComment;
using SPAComments.CommentsModule.Application.Features.Common;
using SPAComments.Core.Abstractions;

namespace SPAComments.CommentsModule.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCommentsModuleApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHandlers();

        return services;
    }

    private static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<CreateCommentCommandHandler>()
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Decorate(
            typeof(ICommandHandler<,>),
            typeof(HtmlSanitizeCommandHandlerDecorator<,>));

        return services;
    }
}