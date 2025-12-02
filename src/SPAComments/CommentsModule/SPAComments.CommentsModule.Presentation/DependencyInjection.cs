using Microsoft.Extensions.DependencyInjection;
using SPAComments.CommentsModule.Application.Interfaces;
using SPAComments.CommentsModule.Presentation.Realtime;

namespace SPAComments.CommentsModule.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddCommentsModulePresentation(this IServiceCollection services)
    {
        services.AddSignalR();

        services.AddSingleton<ICommentsRealtimeQueue, CommentsRealtimeQueue>();

        services.AddScoped<ICommentsRealtimeNotifier, SignalRCommentsRealtimeNotifier>();

        services.AddHostedService<CommentsRealtimeBackgroundService>();

        return services;
    }
}