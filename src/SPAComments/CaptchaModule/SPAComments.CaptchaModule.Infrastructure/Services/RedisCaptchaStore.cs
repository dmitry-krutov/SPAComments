using Microsoft.Extensions.Caching.Distributed;
using SPAComments.CaptchaModule.Application.Services;

namespace SPAComments.CaptchaModule.Infrastructure.Services;

internal sealed class RedisCaptchaStore : ICaptchaStore
{
    private const string PREFIX = "captcha:";
    private readonly IDistributedCache _cache;

    public RedisCaptchaStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task StoreAsync(Guid id, string answer, TimeSpan ttl, CancellationToken ct = default)
    {
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = ttl };

        await _cache.SetStringAsync(PREFIX + id, answer, options, ct);
    }

    public async Task<bool> ValidateAsync(Guid id, string answer, CancellationToken ct = default)
    {
        var key = PREFIX + id;
        var stored = await _cache.GetStringAsync(key, ct);

        if (stored is null)
            return false;

        await _cache.RemoveAsync(key, ct);

        return string.Equals(stored, answer, StringComparison.OrdinalIgnoreCase);
    }
}