using SPAComments.CaptchaModule.Application.Models;

namespace SPAComments.CaptchaModule.Application.Services;

public interface ICaptchaService
{
    Task<CaptchaCreateResult> CreateAsync(CancellationToken cancellationToken = default);

    Task<bool> ValidateAsync(
        Guid captchaId,
        string answer,
        CancellationToken cancellationToken = default);
}