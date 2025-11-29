using CSharpFunctionalExtensions;

namespace SPAComments.SharedKernel.ValueObjects.Ids;

public class CommentId : ComparableValueObject
{
    private CommentId(Guid value)
    {
        Value = value;
    }

    public Guid Value { get; }

    public static CommentId NewId() => new(Guid.NewGuid());

    public static CommentId Empty() => new(Guid.Empty);

    public static CommentId Create(Guid id) => new(id);

    public static Result<CommentId, Error> TryCreate(Guid id) =>
        Result.Success<CommentId, Error>(new(id));

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return Value;
    }
}