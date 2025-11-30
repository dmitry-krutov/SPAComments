using Microsoft.Extensions.Options;
using SkiaSharp;
using SPAComments.CaptchaModule.Application.Services;
using SPAComments.CaptchaModule.Infrastructure.Options;

namespace SPAComments.CaptchaModule.Infrastructure.Services;

internal sealed class SkiaCaptchaImageRenderer : ICaptchaImageRenderer
{
    private readonly CaptchaImageOptions _options;

    public SkiaCaptchaImageRenderer(IOptions<CaptchaImageOptions> options)
    {
        _options = options.Value;
    }

    public byte[] Render(string text)
    {
        var info = new SKImageInfo(_options.Width, _options.Height);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;

        canvas.Clear(SKColors.White);

        DrawNoise(canvas);

        DrawText(canvas, text);

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);

        return data.ToArray();
    }

    private static SKColor RandomColor(Random rnd, int min, int max)
    {
        byte Next() => (byte)rnd.Next(min, max + 1);
        return new SKColor(Next(), Next(), Next());
    }

    private void DrawNoise(SKCanvas canvas)
    {
        var rnd = Random.Shared;

        using (var linePaint = new SKPaint())
        {
            linePaint.IsAntialias = true;
            linePaint.StrokeWidth = 1;
            for (var i = 0; i < _options.NoiseLines; i++)
            {
                linePaint.Color = RandomColor(rnd, min: 150, max: 220);

                var x1 = rnd.Next(0, _options.Width);
                var y1 = rnd.Next(0, _options.Height);
                var x2 = rnd.Next(0, _options.Width);
                var y2 = rnd.Next(0, _options.Height);

                canvas.DrawLine(x1, y1, x2, y2, linePaint);
            }
        }

        using (var dotPaint = new SKPaint())
        {
            dotPaint.IsAntialias = true;
            dotPaint.StrokeWidth = 1;
            for (var i = 0; i < _options.NoiseDots; i++)
            {
                dotPaint.Color = RandomColor(rnd, min: 170, max: 230);
                var x = rnd.Next(0, _options.Width);
                var y = rnd.Next(0, _options.Height);
                canvas.DrawPoint(x, y, dotPaint);
            }
        }
    }

    private void DrawText(SKCanvas canvas, string text)
    {
        var rnd = Random.Shared;

        using var typeface = SKTypeface.FromFamilyName(
            _options.FontFamily,
            SKFontStyle.Bold);

        using var font = new SKFont(typeface, _options.FontSize);

        using var paint = new SKPaint();
        paint.IsAntialias = true;

        var charCount = text.Length;
        var charSpacing = _options.Width / (charCount + 1);
        var baseY = _options.Height * 0.65f;

        for (var i = 0; i < charCount; i++)
        {
            var ch = text[i].ToString();

            paint.Color = _options.UseRandomTextColors
                ? RandomColor(rnd, min: 0, max: 150)
                : new SKColor(50, 50, 50);

            var x = (i + 1) * charSpacing;

            var offsetX = rnd.Next(-_options.MaxOffsetX, _options.MaxOffsetX + 1);
            var offsetY = rnd.Next(-_options.MaxOffsetY, _options.MaxOffsetY + 1);

            var finalX = x + offsetX;
            var finalY = baseY + offsetY;

            var angle = rnd.Next(-_options.MaxRotationDegrees, _options.MaxRotationDegrees + 1);

            canvas.Save();
            canvas.RotateDegrees(angle, finalX, finalY);

            canvas.DrawText(
                ch,
                finalX,
                finalY,
                SKTextAlign.Left,
                font,
                paint);

            canvas.Restore();
        }
    }
}