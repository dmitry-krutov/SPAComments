namespace SPAComments.CaptchaModule.Application.Services;

public interface ICaptchaImageRenderer
{
    byte[] Render(string text);
}