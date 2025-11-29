using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Domain.ValueObjects;

public sealed class Email : ComparableValueObject
{
    public const int MAX_LENGTH = 320;

    private static readonly Regex EmailPattern =
        new(@"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$", RegexOptions.Compiled);

    private Email(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Email, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.Validation.ValueIsRequired(nameof(Email));

        var trimmed = value.Trim();

        if (trimmed.Length > MAX_LENGTH)
            return GeneralErrors.Validation.ValueTooLong(nameof(Email), MAX_LENGTH);

        if (!EmailPattern.IsMatch(trimmed))
            return GeneralErrors.Validation.InvalidFormat(nameof(Email), "Must be a valid email address");

        return new Email(trimmed);
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}