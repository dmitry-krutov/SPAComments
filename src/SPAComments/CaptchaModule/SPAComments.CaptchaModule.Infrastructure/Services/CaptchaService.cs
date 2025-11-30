using Microsoft.Extensions.Options;
using SPAComments.CaptchaModule.Application.Models;
using SPAComments.CaptchaModule.Application.Services;
using SPAComments.CaptchaModule.Infrastructure.Options;

namespace SPAComments.CaptchaModule.Infrastructure.Services;

internal sealed class CaptchaService : ICaptchaService
{
    private readonly ICaptchaTextGenerator _textGenerator;
    private readonly ICaptchaStore _store;
    private readonly ICaptchaImageRenderer _renderer;
    private readonly CaptchaOptions _options;

    public CaptchaService(
        ICaptchaTextGenerator textGenerator,
        ICaptchaStore store,
        ICaptchaImageRenderer renderer,
        IOptions<CaptchaOptions> options)
    {
        _textGenerator = textGenerator;
        _store = store;
        _renderer = renderer;
        _options = options.Value;
    }

    public async Task<CaptchaCreateResult> CreateAsync(CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var text = _textGenerator.Generate(_options.TextLength);

        await _store.StoreAsync(id, text, _options.LifeTime, cancellationToken);

        var imageBytes = _renderer.Render(text);

        return new CaptchaCreateResult(id, imageBytes, "image/png");
    }

    public Task<bool> ValidateAsync(Guid captchaId, string answer, CancellationToken cancellationToken = default)
        => _store.ValidateAsync(captchaId, answer, cancellationToken);
}