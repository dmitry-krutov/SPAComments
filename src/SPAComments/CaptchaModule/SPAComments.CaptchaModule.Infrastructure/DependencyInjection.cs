using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SPAComments.CaptchaModule.Application.Services;
using SPAComments.CaptchaModule.Infrastructure.Options;
using SPAComments.CaptchaModule.Infrastructure.Services;

namespace SPAComments.CaptchaModule.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCaptchaModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<CaptchaOptions>(configuration.GetSection(CaptchaOptions.SECTION_NAME));
        services.Configure<CaptchaImageOptions>(
            configuration.GetSection(CaptchaImageOptions.SECTION_NAME));

        services.AddSingleton<ICaptchaTextGenerator, CaptchaTextGenerator>();
        services.AddSingleton<ICaptchaImageRenderer, SkiaCaptchaImageRenderer>();
        services.AddSingleton<ICaptchaStore, RedisCaptchaStore>();
        services.AddSingleton<ICaptchaService, CaptchaService>();

        return services;
    }
}