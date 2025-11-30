namespace SPAComments.CaptchaModule.Presentation.Models;

public sealed record CaptchaResponse(
    Guid Id,
    string ImageBase64,
    string ContentType);