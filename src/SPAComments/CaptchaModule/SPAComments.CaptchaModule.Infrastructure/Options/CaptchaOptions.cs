namespace SPAComments.CaptchaModule.Infrastructure.Options;

public sealed class CaptchaOptions
{
    public const string SECTION_NAME = "Captcha";

    public int TextLength { get; set; } = 6;

    public TimeSpan LifeTime { get; set; } = TimeSpan.FromMinutes(5);
}