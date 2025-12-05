using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SPAComments.CaptchaModule.Application.Services;
using SPAComments.CaptchaModule.Presentation.Models;
using SPAComments.SharedKernel;

namespace SPAComments.CaptchaModule.Presentation;

public static class CaptchaEndpoints
{
    public static IEndpointRouteBuilder MapCaptchaEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/captcha")
            .WithTags("Captcha");

        group.MapGet(string.Empty, async (
            ICaptchaService captchaService,
            CancellationToken ct) =>
        {
            var result = await captchaService.CreateAsync(ct);

            var response = new CaptchaResponse(
                result.Id,
                Convert.ToBase64String(result.ImageBytes),
                result.ContentType);

            return Results.Ok(Envelope.Ok(response));
        });

        return endpoints;
    }
}