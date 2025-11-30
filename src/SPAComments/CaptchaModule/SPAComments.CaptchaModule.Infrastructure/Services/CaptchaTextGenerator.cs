using System.Security.Cryptography;
using System.Text;
using SPAComments.CaptchaModule.Application.Services;

namespace SPAComments.CaptchaModule.Infrastructure.Services;

internal sealed class CaptchaTextGenerator : ICaptchaTextGenerator
{
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();

    public string Generate(int length)
    {
        var bytes = new byte[length];
        Rng.GetBytes(bytes);

        var sb = new StringBuilder(length);
        for (var i = 0; i < length; i++)
        {
            var index = bytes[i] % AllowedChars.Length;
            sb.Append(AllowedChars[index]);
        }

        return sb.ToString();
    }
}