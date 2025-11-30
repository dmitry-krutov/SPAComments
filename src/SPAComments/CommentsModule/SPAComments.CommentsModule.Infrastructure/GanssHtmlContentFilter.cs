using Ganss.Xss;
using SPAComments.Core.Security;

namespace SPAComments.CommentsModule.Infrastructure;

internal sealed class GanssHtmlContentFilter : IHtmlContentFilter
{
    private readonly HtmlSanitizer _sanitizer;

    public GanssHtmlContentFilter(HtmlSanitizer sanitizer)
    {
        _sanitizer = sanitizer;
    }

    public string Sanitize(string rawHtml)
    {
        if (string.IsNullOrWhiteSpace(rawHtml))
            return string.Empty;

        return _sanitizer.Sanitize(rawHtml);
    }
}