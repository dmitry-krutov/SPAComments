using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Domain.ValueObjects;

public class UserName : ComparableValueObject
{
    public const int MIN_LENGTH = 3;
    public const int MAX_LENGTH = 20;

    private static readonly Regex AllowedPattern =
        new(@"^[A-Za-z0-9]+$", RegexOptions.Compiled);

    private UserName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<UserName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.Validation.ValueIsRequired(nameof(UserName));

        if (value.Length < MIN_LENGTH)
            return GeneralErrors.Validation.ValueTooSmall(nameof(UserName), MIN_LENGTH);

        if (value.Length > MAX_LENGTH)
            return GeneralErrors.Validation.ValueTooLarge(nameof(UserName), MAX_LENGTH);

        if (!AllowedPattern.IsMatch(value))
        {
            return GeneralErrors.Validation
                .InvalidFormat(nameof(UserName), "Only Latin letters and digits are allowed");
        }

        return new UserName(value);
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}