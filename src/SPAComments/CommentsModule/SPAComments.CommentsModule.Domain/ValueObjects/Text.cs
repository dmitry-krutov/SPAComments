using CSharpFunctionalExtensions;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Domain.ValueObjects;

public sealed class Text : ComparableValueObject
{
    public const int MIN_LENGTH = 1;
    public const int MAX_LENGTH = 2000;

    private Text(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<Text, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return GeneralErrors.Validation.ValueIsRequired(nameof(Text));

        var trimmed = value.Trim();

        if (trimmed.Length < MIN_LENGTH)
            return GeneralErrors.Validation.ValueTooSmall(nameof(Text), MIN_LENGTH);

        if (trimmed.Length > MAX_LENGTH)
            return GeneralErrors.Validation.ValueTooLarge(nameof(Text), MAX_LENGTH);

        return new Text(trimmed);
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}