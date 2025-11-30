namespace SPAComments.CaptchaModule.Application.Models;

public sealed record CaptchaCreateResult(
    Guid Id,
    byte[] ImageBytes,
    string ContentType);