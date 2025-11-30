namespace SPAComments.CaptchaModule.Infrastructure.Options;

public class CaptchaImageOptions
{
    public const string SECTION_NAME = "Captcha:Image";

    public int Width { get; set; } = 200;

    public int Height { get; set; } = 60;

    public int FontSize { get; set; } = 32;

    public string FontFamily { get; set; } = "Arial";

    public int NoiseLines { get; set; } = 5;

    public int NoiseDots { get; set; } = 50;

    public int MaxRotationDegrees { get; set; } = 20;

    public int MaxOffsetX { get; set; } = 5;

    public int MaxOffsetY { get; set; } = 5;

    public bool UseRandomTextColors { get; set; } = true;
}