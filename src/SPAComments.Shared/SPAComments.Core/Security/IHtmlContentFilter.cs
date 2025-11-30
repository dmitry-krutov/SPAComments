namespace SPAComments.Core.Security;

public interface IHtmlContentFilter
{
    string Sanitize(string rawHtml);
}