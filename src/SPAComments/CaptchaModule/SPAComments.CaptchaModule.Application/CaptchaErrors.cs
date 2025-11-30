using SPAComments.SharedKernel;

namespace SPAComments.CaptchaModule.Application;

public static class CaptchaErrors
{
    public static readonly Error CaptchaIdRequired =
        Error.Validation("captcha.id.required", "CaptchaId is required", "CaptchaId");

    public static readonly Error CaptchaAnswerRequired =
        Error.Validation("captcha.answer.required", "Captcha answer is required", "CaptchaAnswer");

    public static readonly Error CaptchaInvalid =
        Error.Validation("captcha.invalid", "Captcha answer is incorrect", "CaptchaAnswer");
}