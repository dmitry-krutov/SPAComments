using CSharpFunctionalExtensions;
using SPAComments.SharedKernel;

namespace SPAComments.CommentsModule.Domain.ValueObjects;

public sealed class CommentAttachment : ComparableValueObject
{
    public Guid FileId { get; }

    private CommentAttachment(Guid fileId)
    {
        FileId = fileId;
    }

    public static Result<CommentAttachment, Error> Create(Guid fileId)
    {
        if (fileId == Guid.Empty)
            return GeneralErrors.Validation.ValueIsRequired(nameof(FileId));

        return new CommentAttachment(fileId);
    }

    protected override IEnumerable<IComparable> GetComparableEqualityComponents()
    {
        yield return FileId;
    }
}