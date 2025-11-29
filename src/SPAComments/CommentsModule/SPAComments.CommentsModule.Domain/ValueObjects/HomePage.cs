using CSharpFunctionalExtensions;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Domain.ValueObjects;

public sealed class HomePage : ComparableValueObject
{
    public const int MAX_LENGTH = 2048;

    private HomePage(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<HomePage, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.Validation.ValueIsRequired(nameof(HomePage));

        var trimmed = value.Trim();

        if (trimmed.Length > MAX_LENGTH)
            return GeneralErrors.Validation.ValueTooLong(nameof(HomePage), MAX_LENGTH);

        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri))
            return GeneralErrors.Validation.InvalidFormat(nameof(HomePage), "Invalid URL");

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return GeneralErrors.Validation.InvalidFormat(nameof(HomePage), "URL must start with http:// or https://");

        return new HomePage(trimmed);
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}