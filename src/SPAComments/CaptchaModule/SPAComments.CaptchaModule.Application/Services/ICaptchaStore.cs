namespace SPAComments.CaptchaModule.Application.Services;

public interface ICaptchaStore
{
    Task StoreAsync(Guid id, string answer, TimeSpan ttl, CancellationToken ct = default);

    Task<bool> ValidateAsync(Guid id, string answer, CancellationToken ct = default);
}