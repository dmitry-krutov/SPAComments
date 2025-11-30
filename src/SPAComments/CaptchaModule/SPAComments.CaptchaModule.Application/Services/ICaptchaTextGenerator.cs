namespace SPAComments.CaptchaModule.Application.Services;

public interface ICaptchaTextGenerator
{
    string Generate(int length);
}