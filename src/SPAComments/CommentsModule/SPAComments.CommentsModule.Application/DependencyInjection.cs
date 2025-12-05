using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SPAComments.CommentsModule.Application.Features.Commands.CreateComment;
using SPAComments.CommentsModule.Application.Features.Common;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.Core.Abstractions;

namespace SPAComments.CommentsModule.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCommentsModuleApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHandlers();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<CreateCommentCommandHandler>();
        });

        services.AddValidatorsFromAssemblyContaining<CreateCommentCommandValidator>();

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
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.AssignableTo(typeof(IQueryHandlerWithResult<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.Decorate(
            typeof(ICommandHandler<,>),
            typeof(HtmlSanitizeCommandHandlerDecorator<,>));

        return services;
    }
}